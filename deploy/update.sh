#!/usr/bin/env bash
# Installs or updates CSCMaster on the server, from the release branch. Needs /root/common (RetinueSoft/VpsSetup);
# /root/common/setup.sh installs it the first time.
#
#   sudo /root/cscmaster/update.sh                 newest release build
#   sudo /root/cscmaster/update.sh --backup        back up the database first, then update
#   sudo /root/cscmaster/update.sh sha-1a2b3c4     run a specific build (rollback); tags: GitHub → Packages → cscmaster-api
#
# Each run: updates this script, downloads docker-compose.yml and .env.example from release, adds new settings
# to /root/common/env/cscmaster.env (your values are kept), pulls the images, applies database migrations and
# restarts what changed. HTTPS/domains: VpsSetup caddy/sites/cscmaster.caddy (proxies to 127.0.0.1:UI/API_HOST_PORT).
set -euo pipefail

NAME="cscmaster"
REPO="RetinueSoft/CSCMaster"

[ -f /root/common/lib.sh ] || { echo "/root/common is missing: run VpsSetup's setup.sh first"; exit 1; }
. /root/common/lib.sh

need_root "$@"
cd "$(dirname "$(readlink -f "$0")")"
[ -d .git ] || [ -d ../.git ] && die "this is the source repository; run it on the server as /root/$NAME/update.sh"
with_lock "$NAME"
load_token
self_update "$REPO" deploy/update.sh "$@"

BACKUP=0; TAG=latest
for arg in "$@"; do
    case "$arg" in
        --backup) BACKUP=1 ;;
        -*)       die "unknown option $arg" ;;
        *)        TAG="$arg" ;;
    esac
done

# 1. Deployment files from the release branch.
fetch_files "$REPO" "$RELEASE_BRANCH" \
    docker-compose.yml:docker-compose.yml \
    .env.example:.env.example \
    deploy/import-sqlserver.sh:import-sqlserver.sh
chmod 755 import-sqlserver.sh
mkdir -p backups

# 2. Settings: /root/common/env/cscmaster.env, linked as .env here.
ENV_FILE="$(app_env_path "$NAME")"
link_app_env "$NAME"
created=0
sync_env .env.example "$ENV_FILE" || created=1
# JWT signing key: a random one when empty (changing it later logs everyone out).
if [ -z "$(env_get "$ENV_FILE" JWT_KEY)" ]; then
    env_set "$ENV_FILE" JWT_KEY "$(od -An -N32 -tx1 /dev/urandom | tr -d ' \n')"
    log "generated a random JWT_KEY"
fi
if [ "$created" = 1 ]; then
    echo "Fill in POSTGRES_PASSWORD (check UI_PUBLIC_URL, API_PUBLIC_URL): nano $ENV_FILE, then run $0 again."
    exit 0
fi
set -a; . "$ENV_FILE"; set +a
for v in POSTGRES_PASSWORD JWT_KEY UI_PUBLIC_URL API_PUBLIC_URL; do
    [ -n "${!v:-}" ] || die "$v is empty in $ENV_FILE"
done

# Remember the chosen build, so a later "docker compose up" keeps using it.
env_set "$ENV_FILE" APP_IMAGE_TAG "$TAG"
export APP_IMAGE_TAG="$TAG"

# 3. Images.
log "pulling images ($TAG)"
ghcr_login
docker compose pull api ui

# 4. Database: start PostgreSQL, optional backup, migrations.
docker compose up -d --wait postgres
if [ "$BACKUP" = 1 ]; then
    /root/common/backup-db.sh "$NAME" || die "backup failed; nothing was updated"
fi
log "applying database migrations"
docker compose run --rm --no-deps --entrypoint ./efbundle api \
    --connection "Host=postgres;Port=5432;Database=${POSTGRES_DB:-cscmaster};Username=postgres;Password=$POSTGRES_PASSWORD"

# 5. Start / restart what changed.
docker compose up -d --remove-orphans
docker image prune -f >/dev/null

docker inspect -f '{{index .Config.Labels "org.opencontainers.image.revision"}}' cscmaster-api > .deployed_sha 2>/dev/null || true
docker compose ps
log "$NAME is running $(docker inspect -f '{{.Config.Image}}' cscmaster-api) + ui ($(cut -c1-7 .deployed_sha))"
