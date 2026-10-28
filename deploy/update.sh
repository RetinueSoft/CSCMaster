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

# 2. Settings: /root/common/.env (shared by all apps, e.g. POSTGRES_PASSWORD, SERVER_IP) + /root/common/env/cscmaster.env
#    (this app). /root/cscmaster/.env is generated from both; docker compose reads it.
ENV_FILE="$(app_env_path "$NAME")"
link_app_env "$NAME"
sync_env .env.example "$ENV_FILE" || true
load_common_env
# JWT signing key: a random one when empty (changing it later logs everyone out).
if [ -z "$(env_get "$ENV_FILE" JWT_KEY)" ]; then
    env_set "$ENV_FILE" JWT_KEY "$(od -An -N32 -tx1 /dev/urandom | tr -d ' \n')"
    log "generated a random JWT_KEY"
fi
# The API is public on https://<server-ip>:8084 (common Caddy), so it listens on 127.0.0.1:18084.
# Settings files made from the older template (api.cscmaster.online on 8084) are moved over once.
if [ "$(env_get "$ENV_FILE" API_HOST_PORT)" = 8084 ]; then
    env_set "$ENV_FILE" API_HOST_PORT 18084; log "API_HOST_PORT 8084 -> 18084 (Caddy serves 8084)"
fi
api_url="$(env_get "$ENV_FILE" API_PUBLIC_URL)"
if [ -z "$api_url" ] || [ "$api_url" = "https://api.cscmaster.online" ]; then
    [ -n "${SERVER_IP:-}" ] || die "SERVER_IP is empty in $COMMON_DIR/.env"
    env_set "$ENV_FILE" API_PUBLIC_URL "https://$SERVER_IP:8084"; log "API_PUBLIC_URL = https://$SERVER_IP:8084"
fi
env_set "$ENV_FILE" APP_IMAGE_TAG "$TAG"      # remember the chosen build for a later "docker compose up"
set -a; . ./.env; set +a
[ -n "${POSTGRES_PASSWORD:-}" ] || die "POSTGRES_PASSWORD is empty: set it in $COMMON_DIR/.env (shared by all apps)"
for v in JWT_KEY UI_PUBLIC_URL API_PUBLIC_URL; do
    [ -n "${!v:-}" ] || die "$v is empty in $ENV_FILE"
done

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
