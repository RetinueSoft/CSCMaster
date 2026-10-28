#!/usr/bin/env bash
# Backs up the PostgreSQL database (Docker container) and uploads it to Azure Blob Storage.
# Linux version of the Windows DBBackup.bat:
#   1. pg_dump into ./backups (custom format, compressed)
#   2. upload the file to Azure with azcopy (SAS URL)
#   3. delete the local file after a successful upload
#
# Settings come from .env:
#   POSTGRES_DB (default cscmaster)
#   AZURE_BACKUP_SAS_URL   container SAS URL, e.g. https://<account>.blob.core.windows.net/<container>?sp=cw&...
#   BACKUP_KEEP_LOCAL      "true" to keep the local file after upload (default false)
#
#   sudo ./backup-db.sh
# Schedule with root's crontab, see DOCKER.md. Restore with restore-db.sh.
set -uo pipefail
# Run from the folder with docker-compose.yml (on the server, next to this script; in the repository, one up).
cd "$(dirname "$0")"; [ -f docker-compose.yml ] || cd ..

set -a; . ./.env; set +a
DB="${POSTGRES_DB:-cscmaster}"
CONTAINER=cscmaster-postgres
LOG_FILE="backups/backup.log"
AZCOPY="${AZCOPY:-$(command -v azcopy || echo /usr/local/bin/azcopy)}"

mkdir -p backups
log() { echo "$(date '+%Y-%m-%d %H:%M:%S') $*" >> "$LOG_FILE"; }

# Skip if a previous run is still going (same as "IgnoreNew" in the Windows task).
exec 9>"backups/.backup.lock"
if ! flock -n 9; then log "Previous backup still running, skipped"; exit 0; fi

NAME="${DB}_$(date +%Y_%m_%d_%H%M%S).dump"
HOST_FILE="backups/$NAME"

log "Start: $NAME"

# 1. Backup (written inside the container to /backups, which is ./backups here)
if ! docker exec "$CONTAINER" pg_dump -U postgres -d "$DB" --format=custom --file "/backups/$NAME" >> "$LOG_FILE" 2>&1 \
   || [ ! -s "$HOST_FILE" ]; then
    log "Backup FAILED"
    exit 1
fi
log "Backup SUCCESS ($(du -h "$HOST_FILE" | cut -f1))"

# 2. Upload to Azure
if [ -z "${AZURE_BACKUP_SAS_URL:-}" ]; then
    log "AZURE_BACKUP_SAS_URL not set, upload skipped (local file kept)"
    exit 0
fi
log "Uploading to Azure..."
if ! "$AZCOPY" copy "$HOST_FILE" "$AZURE_BACKUP_SAS_URL" --overwrite=true --log-level=ERROR >> "$LOG_FILE" 2>&1; then
    log "Upload FAILED (local file kept: $HOST_FILE)"
    exit 2
fi
log "Upload SUCCESS"

# 3. Delete local file
if [ "${BACKUP_KEEP_LOCAL:-false}" != "true" ]; then
    rm -f "$HOST_FILE" && log "Local file deleted" || log "Delete FAILED"
fi
log "End"
