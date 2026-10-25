#!/usr/bin/env bash
# Restores a PostgreSQL backup made by backup-db.sh (or pg_dump --format=custom) into the container.
#
#   ./restore-db.sh backups/cscmaster_2026_10_25_020000.dump
#
# WARNING: replaces the whole database. The app is stopped during the restore and started again afterwards.
# To load data from the old SQL Server instead, use import-sqlserver.sh (see DOCKER.md).
set -euo pipefail
# Run from the folder with docker-compose.yml (on the server, next to this script; in the repository, one up).
cd "$(dirname "$0")"; [ -f docker-compose.yml ] || cd ..

FILE="${1:?Usage: $0 backups/<file>.dump}"
NAME="$(basename "$FILE")"
[ -f "backups/$NAME" ] || { echo "backups/$NAME not found"; exit 1; }

set -a; . ./.env; set +a
DB="${POSTGRES_DB:-cscmaster}"

read -r -p "Replace database '$DB' with $NAME? Type yes: " answer
[ "$answer" = "yes" ] || { echo "Cancelled."; exit 1; }

docker compose stop api ui
echo "Recreating database $DB ..."
docker exec cscmaster-postgres psql -U postgres -d postgres -v ON_ERROR_STOP=1 \
    -c "DROP DATABASE IF EXISTS \"$DB\" WITH (FORCE);" -c "CREATE DATABASE \"$DB\";"
echo "Restoring $NAME ..."
docker exec cscmaster-postgres pg_restore -U postgres -d "$DB" --no-owner --exit-on-error "/backups/$NAME"
docker compose start api ui
echo "Done."
