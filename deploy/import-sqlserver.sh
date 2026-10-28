#!/usr/bin/env bash
# Loads all data from a SQL Server backup (.bak) into the PostgreSQL database, on the server.
#
#   1. copy the .bak from the Windows server into ./backups (e.g. with scp or WinSCP)
#   2. sudo ./import-sqlserver.sh backups/CSCMaster.bak
#
# Starts a temporary SQL Server container, restores the backup into it, runs the copy tool
# (CSCMasterDataCopy, included in the API image) into PostgreSQL, then removes the temporary SQL Server.
# Replaces all data in PostgreSQL; the API and UI are stopped during the copy. Needs about 2 GB of free memory.
# Run update.sh first (PostgreSQL must be running with all migrations applied).
set -euo pipefail
# Run from the folder with docker-compose.yml (on the server, next to this script; in the repository, one up).
cd "$(dirname "$0")"; [ -f docker-compose.yml ] || cd ..

BAK="${1:?Usage: $0 backups/<file>.bak}"
NAME="$(basename "$BAK")"
[ -f "backups/$NAME" ] || { echo "backups/$NAME not found"; exit 1; }

set -a; . ./.env; set +a
DB="${POSTGRES_DB:-cscmaster}"
TEMP=cscmaster-sqltemp
SA_PASSWORD="Tmp$(od -An -N16 -tx1 /dev/urandom | tr -d ' \n')Aa1"   # random, only for the temporary SQL Server
NETWORK="$(docker inspect -f '{{range $name, $_ := .NetworkSettings.Networks}}{{$name}}{{end}}' cscmaster-postgres)"

read -r -p "Replace ALL data in PostgreSQL database '$DB' with the data from $NAME? Type yes: " answer
[ "$answer" = "yes" ] || { echo "Cancelled."; exit 1; }

cleanup() { docker rm -f "$TEMP" >/dev/null 2>&1 || true; }
trap cleanup EXIT

echo "== Starting a temporary SQL Server"
chmod a+r "backups/$NAME"
docker rm -f "$TEMP" >/dev/null 2>&1 || true
docker run -d --name "$TEMP" --network "$NETWORK" -e ACCEPT_EULA=Y -e MSSQL_PID=Express \
    -e MSSQL_SA_PASSWORD="$SA_PASSWORD" -v "$(pwd)/backups:/import:ro" mcr.microsoft.com/mssql/server:2025-latest >/dev/null
SQL=(docker exec -i "$TEMP" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -b)
for i in $(seq 1 60); do
    "${SQL[@]}" -Q "SELECT 1" >/dev/null 2>&1 && break
    [ "$i" = 60 ] && { echo "SQL Server did not start"; docker logs --tail 20 "$TEMP"; exit 1; }
    sleep 2
done

echo "== Restoring $NAME"
# A .bak file can hold several backups (SSMS appends by default): use the newest full backup.
POS=$("${SQL[@]}" -h -1 -W -s "|" -Q "SET NOCOUNT ON; RESTORE HEADERONLY FROM DISK = N'/import/$NAME'" \
  | awk -F'|' '$3=="1" { p=$6; d=$19 } END { print p "|" d }')
FILE_NO=${POS%%|*}
[ -n "$FILE_NO" ] || { echo "No full database backup found in $NAME"; exit 1; }
echo "   backup set $FILE_NO, taken ${POS#*|}"
MOVES=$("${SQL[@]}" -h -1 -W -s "|" -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'/import/$NAME' WITH FILE = $FILE_NO" \
  | awk -F'|' 'NF>2 && $1!="" {
      ext = ($3=="L") ? "_log.ldf" : (n++ ? "_" n ".ndf" : ".mdf");
      printf ", MOVE N'\''%s'\'' TO N'\''/var/opt/mssql/data/source%s'\''", $1, ext }')
"${SQL[@]}" -Q "RESTORE DATABASE [source] FROM DISK = N'/import/$NAME' WITH FILE = $FILE_NO, REPLACE, RECOVERY $MOVES" | tail -1

echo "== Copying the data into PostgreSQL (app stopped)"
docker compose stop api ui
set +e
docker compose run --rm --no-deps --entrypoint dotnet api datacopy/CSCMasterDataCopy.dll \
    "Server=$TEMP;Database=source;User Id=sa;Password=$SA_PASSWORD;Encrypt=False;TrustServerCertificate=True" \
    "Host=postgres;Port=5432;Database=$DB;Username=postgres;Password=$POSTGRES_PASSWORD;Command Timeout=0"
result=$?
set -e
docker compose start api ui

[ "$result" = 0 ] && echo "== Done: data imported, temporary SQL Server removed." \
                  || echo "== The copy reported problems (exit code $result); see the output above."
exit "$result"
