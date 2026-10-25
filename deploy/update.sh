#!/usr/bin/env bash
# Installs or updates CSCMaster (PostgreSQL + API + UI) on the server. No git clone needed.
# Put this file in an empty folder on the server (e.g. ~/cscmaster/update.sh); everything is kept next to it:
#   docker-compose.yml, .env.example, backup-db.sh, restore-db.sh, import-sqlserver.sh (downloaded), .env (yours), backups/
# HTTPS is done by the server's main Caddy, which proxies to the UI (UI_HOST_PORT) and API (API_HOST_PORT).
#
#   sudo ./update.sh              update to the newest version
#   sudo ./update.sh sha-1a2b3c4  run a specific build (rollback); tags: GitHub → Packages → cscmaster-api / cscmaster-ui
#
# Each run:
#   1. updates this script itself, then downloads the latest docker-compose.yml, .env.example
#      and the backup/restore scripts from GitHub (only these files, not the whole repository)
#   2. creates .env on the first run / adds new settings to it (your values are kept)
#   3. pulls the newest app image from ghcr.io
#   4. starts PostgreSQL and applies any new database migrations
#   5. restarts the containers that changed
#
# GitHub token: read from /root/.github_token_cscmaster (scopes: repo, read:packages), readable only by root:
#   echo '<token>' | sudo tee /root/.github_token_cscmaster >/dev/null && sudo chmod 600 /root/.github_token_cscmaster
set -euo pipefail

REPO="RetinueSoft/CSCMaster"
BRANCH="postgres-migration"
TOKEN_FILE="${GITHUB_TOKEN_FILE:-/root/.github_token_cscmaster}"

cd "$(dirname "$0")"
BASE="$(pwd)"
SELF="$(basename "$0")"
# GitHub path -> file name in this folder
FILES=("docker-compose.yml:docker-compose.yml" ".env.example:.env.example"
       "deploy/backup-db.sh:backup-db.sh" "deploy/restore-db.sh:restore-db.sh"
       "deploy/import-sqlserver.sh:import-sqlserver.sh")

if [ -d .git ] || [ -d ../.git ]; then
    echo "This is the source repository. Copy update.sh to the deployment folder on the server and run it there."
    exit 1
fi

if [ ! -r "$TOKEN_FILE" ]; then
    echo "Cannot read the GitHub token from $TOKEN_FILE (run with sudo, or create it:"
    echo "  echo '<token>' | sudo tee $TOKEN_FILE >/dev/null && sudo chmod 600 $TOKEN_FILE )"
    exit 1
fi
TOKEN="$(tr -d '[:space:]' < "$TOKEN_FILE")"

download() {   # download <repo path> <local file>
    curl -fsSL -H "Authorization: Bearer $TOKEN" -H "Accept: application/vnd.github.raw" \
         "https://api.github.com/repos/$REPO/contents/$1?ref=$BRANCH" -o "$2"
}

# 1a. Update this script first, then restart with the new version.
if [ -z "${CSCMASTER_SELF_UPDATED:-}" ]; then
    tmp="$(mktemp)"
    download deploy/update.sh "$tmp"
    if ! cmp -s "$tmp" "$SELF"; then
        echo "== $SELF changed, restarting with the new version"
        mv "$tmp" "$SELF"; chmod +x "$SELF"
        CSCMASTER_SELF_UPDATED=1 exec "./$SELF" "$@"
    fi
    rm -f "$tmp"
fi

# 1b. Download the deployment files.
echo "== Downloading files from $REPO ($BRANCH)"
mkdir -p backups
for entry in "${FILES[@]}"; do
    src="${entry%%:*}"; f="${entry#*:}"
    tmp="$(mktemp)"; download "$src" "$tmp"
    if [ -f "$f" ] && cmp -s "$tmp" "$f"; then rm -f "$tmp"; echo "   unchanged  $f"
    else mv "$tmp" "$f"; echo "   updated    $f"; fi
done
chmod +x backup-db.sh restore-db.sh import-sqlserver.sh
chmod 644 docker-compose.yml .env.example

# 2. .env: create on the first run, otherwise add settings that are new in .env.example.
if [ ! -f .env ]; then
    cp .env.example .env; chmod 600 .env
    echo
    echo "Created $BASE/.env. Fill in POSTGRES_PASSWORD and check UI_PUBLIC_URL / API_PUBLIC_URL (nano .env), then run ./$SELF again."
    exit 0
fi
added=0
while IFS= read -r line; do
    key="${line%%=*}"
    if ! grep -q "^${key}=" .env; then
        printf '%s\n' "$line" >> .env; echo "   added new setting to .env: $key"; added=1
    fi
done < <(grep -E '^[A-Za-z_][A-Za-z0-9_]*=' .env.example)
[ "$added" = 1 ] && echo "   (check the new settings in .env)"

# JWT signing key: create a random one if it is empty (changing it later logs everyone out).
if grep -q '^JWT_KEY=$' .env; then
    sed -i "s|^JWT_KEY=$|JWT_KEY=$(od -An -N32 -tx1 /dev/urandom | tr -d ' \n')|" .env
    echo "   generated a random JWT_KEY in .env"
fi

set -a; . ./.env; set +a
for v in POSTGRES_PASSWORD JWT_KEY; do
    [ -n "${!v:-}" ] || { echo "$v is empty in .env. Fill it in (nano .env) and run again."; exit 1; }
done

# azcopy (used by backup-db.sh to upload backups to Azure): install it if it is missing.
install_azcopy() {
    local arch url dir
    case "$(uname -m)" in
        x86_64|amd64)  url="https://aka.ms/downloadazcopy-v10-linux" ;;
        aarch64|arm64) url="https://aka.ms/downloadazcopy-v10-linux-arm64" ;;
        *) echo "   unknown CPU type $(uname -m); install azcopy by hand"; return 1 ;;
    esac
    dir="$(mktemp -d)"
    curl -fsSL -o "$dir/azcopy.tar.gz" "$url" \
        && tar -xzf "$dir/azcopy.tar.gz" -C "$dir" \
        && install -m 755 "$dir"/azcopy_linux_*/azcopy /usr/local/bin/azcopy
    local result=$?
    rm -rf "$dir"
    return $result
}
if ! command -v azcopy >/dev/null 2>&1 && [ ! -x /usr/local/bin/azcopy ]; then
    echo "== Installing azcopy (for backups to Azure)"
    if install_azcopy; then echo "   $(/usr/local/bin/azcopy --version)"
    else echo "   azcopy could not be installed; backups will stay local until it is (see DOCKER.md)"; fi
fi

# Remember the chosen version, so later "docker compose up" keeps using it.
TAG="${1:-latest}"
sed -i "s/^APP_IMAGE_TAG=.*/APP_IMAGE_TAG=$TAG/" .env
export APP_IMAGE_TAG="$TAG"

# 3. Pull images (uses the server's existing ghcr.io login: docker login ghcr.io).
echo "== Pulling images (app: $TAG)"
docker compose pull api ui

# 4. Database: start PostgreSQL, apply migrations.
echo "== Starting PostgreSQL"
docker compose up -d --wait postgres
echo "== Applying database migrations"
docker compose run --rm --no-deps --entrypoint ./efbundle api \
    --connection "Host=postgres;Port=5432;Database=${POSTGRES_DB:-cscmaster};Username=postgres;Password=$POSTGRES_PASSWORD"

# 5. Start / restart what changed.
echo "== Starting containers"
docker compose up -d --remove-orphans
docker image prune -f >/dev/null

echo "== Status"
docker compose ps
echo
docker compose logs --tail=10 api ui
echo
echo "Running images: $(docker inspect -f '{{.Config.Image}}' cscmaster-api) $(docker inspect -f '{{.Config.Image}}' cscmaster-ui)"
