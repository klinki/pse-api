#!/usr/bin/env bash
#
# Copies PSE API data from the MySQL database into an (empty) PostgreSQL database
# whose schema was created from scripts/postgres/schema.sql.
#
# The data is streamed table by table: `mysql --batch` writes tab-separated rows
# (NULL as the literal NULL, \t \n \\ escaped), which PostgreSQL reads with
# COPY ... (FORMAT text, NULL 'NULL'). Afterwards the identity sequences are moved
# past the copied ids and row counts and checksums of both databases are compared.
#
# Clients are configured with their standard environment variables:
#   MySQL:      MYSQL_HOST, MYSQL_TCP_PORT, MYSQL_USER, MYSQL_PWD, MYSQL_DATABASE (default: pse)
#   PostgreSQL: PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE
#
# Both clients can be replaced by a command prefix, e.g. to run them in containers:
#   MYSQL_CLIENT="docker exec -i -e MYSQL_PWD legacy-mysql-db mysql -upse"
#   PSQL_CLIENT="docker exec -i -e PGPASSWORD postgres-db psql -U pse"
#
# Options (environment):
#   TRUNCATE=1   empty the PostgreSQL tables first (to re-run after a failed attempt)
#
set -euo pipefail

MYSQL_DATABASE="${MYSQL_DATABASE:-pse}"
read -r -a MYSQL_CMD <<< "${MYSQL_CLIENT:-mysql}"
read -r -a PSQL_CMD <<< "${PSQL_CLIENT:-psql}"

# table name | column list (same names in both databases)
TABLES=(
  'days|Day'
  'Stocks|Id,BIC,Name,ISIN'
  'trades|Id,ISIN,Name,BIC,Date,Close,Change,Previous,YearMin,YearMax,Volume,TradedAmount,LastTrade,MarketGroup,Mode,MarketCode,DayMin,DayMax,Open,LotSize'
)

log() { printf '[%s] %s\n' "$(date -u +%H:%M:%S)" "$*" >&2; }

my() { "${MYSQL_CMD[@]}" --batch --skip-column-names --quick --default-character-set=utf8mb4 "$MYSQL_DATABASE" "$@"; }
pg() { "${PSQL_CMD[@]}" -X -v ON_ERROR_STOP=1 --no-align --tuples-only --quiet "$@"; }

mysql_cols() { sed -E 's/([^,]+)/`\1`/g' <<< "$1"; }
pg_cols() { sed -E 's/([^,]+)/"\1"/g' <<< "$1"; }

log "Checking connections"
my -e 'SELECT 1' > /dev/null
pg -c 'SELECT 1' > /dev/null

if [[ "$(pg -c "SELECT count(*) FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" LIKE '%_InitialPostgreSql'")" != "1" ]]; then
  log "PostgreSQL schema is missing - apply scripts/postgres/schema.sql first"
  exit 1
fi

for entry in "${TABLES[@]}"; do
  table="${entry%%|*}"
  rows="$(pg -c "SELECT count(*) FROM \"$table\"")"
  if [[ "$rows" != "0" ]]; then
    if [[ "${TRUNCATE:-0}" == "1" ]]; then
      log "Truncating \"$table\" ($rows rows)"
      pg -c "TRUNCATE \"$table\""
    else
      log "Target table \"$table\" is not empty ($rows rows); re-run with TRUNCATE=1 to replace its data"
      exit 1
    fi
  fi
done

for entry in "${TABLES[@]}"; do
  table="${entry%%|*}"
  columns="${entry#*|}"
  log "Copying $table"
  my -e "SELECT $(mysql_cols "$columns") FROM \`$table\`" \
    | pg -c "COPY \"$table\" ($(pg_cols "$columns")) FROM STDIN WITH (FORMAT text, NULL 'NULL')"
done

log "Resetting identity sequences"
for table in Stocks trades; do
  pg -c "SELECT setval(pg_get_serial_sequence('\"$table\"', 'Id'), COALESCE(max(\"Id\"), 0) + 1, false) FROM \"$table\"" > /dev/null
done

log "Analyzing"
pg -c 'ANALYZE "days"; ANALYZE "Stocks"; ANALYZE "trades";'

log "Verifying"
# Same aggregates on both sides, rendered as text so they can be compared directly
VERIFY_MYSQL="
SELECT 'days', COUNT(*), COALESCE(MIN(\`Day\`), ''), COALESCE(MAX(\`Day\`), '') FROM \`days\`
UNION ALL SELECT 'Stocks', COUNT(*), COALESCE(SUM(\`Id\`), 0), COUNT(DISTINCT LOWER(\`ISIN\`)) FROM \`Stocks\`
UNION ALL SELECT 'trades', COUNT(*), COALESCE(SUM(\`Id\`), 0), COALESCE(SUM(\`Volume\`), 0) FROM \`trades\`
UNION ALL SELECT 'trades.prices', COUNT(DISTINCT \`ISIN\`, \`Date\`), CAST(COALESCE(SUM(\`Close\`), 0) AS DECIMAL(30,4)), CAST(COALESCE(SUM(\`TradedAmount\`), 0) AS DECIMAL(30,4)) FROM \`trades\`
UNION ALL SELECT 'trades.text', COUNT(\`Name\`), COALESCE(SUM(CHAR_LENGTH(CONCAT_WS('', \`ISIN\`, \`Name\`, \`BIC\`, \`MarketGroup\`, \`Mode\`, \`MarketCode\`))), 0), COALESCE(SUM(LENGTH(CONCAT_WS('', \`ISIN\`, \`Name\`, \`BIC\`, \`MarketGroup\`, \`Mode\`, \`MarketCode\`))), 0) FROM \`trades\`
UNION ALL SELECT 'Stocks.text', COUNT(\`Name\`), COALESCE(SUM(CHAR_LENGTH(CONCAT_WS('', \`BIC\`, \`Name\`, \`ISIN\`))), 0), COALESCE(SUM(LENGTH(CONCAT_WS('', \`BIC\`, \`Name\`, \`ISIN\`))), 0) FROM \`Stocks\`;"
VERIFY_PG="
SELECT 'days', count(*), COALESCE(min(\"Day\")::text, ''), COALESCE(max(\"Day\")::text, '') FROM \"days\"
UNION ALL SELECT 'Stocks', count(*), COALESCE(sum(\"Id\"), 0)::text, count(DISTINCT lower(\"ISIN\"))::text FROM \"Stocks\"
UNION ALL SELECT 'trades', count(*), COALESCE(sum(\"Id\"), 0)::text, COALESCE(sum(\"Volume\"), 0)::text FROM \"trades\"
UNION ALL SELECT 'trades.prices', count(DISTINCT (\"ISIN\", \"Date\")), COALESCE(sum(\"Close\"), 0)::numeric(30,4)::text, COALESCE(sum(\"TradedAmount\"), 0)::numeric(30,4)::text FROM \"trades\"
UNION ALL SELECT 'trades.text', count(\"Name\"), COALESCE(sum(char_length(concat_ws('', \"ISIN\", \"Name\", \"BIC\", \"MarketGroup\", \"Mode\", \"MarketCode\"))), 0)::text, COALESCE(sum(octet_length(concat_ws('', \"ISIN\", \"Name\", \"BIC\", \"MarketGroup\", \"Mode\", \"MarketCode\"))), 0)::text FROM \"trades\"
UNION ALL SELECT 'Stocks.text', count(\"Name\"), COALESCE(sum(char_length(concat_ws('', \"BIC\", \"Name\", \"ISIN\"))), 0)::text, COALESCE(sum(octet_length(concat_ws('', \"BIC\", \"Name\", \"ISIN\"))), 0)::text FROM \"Stocks\";"

mysql_result="$(my -e "$VERIFY_MYSQL")"
pg_result="$(pg --field-separator=$'\t' -c "$VERIFY_PG")"

printf 'MySQL:\n%s\n\nPostgreSQL:\n%s\n\n' "$mysql_result" "$pg_result" >&2

if [[ "$mysql_result" != "$pg_result" ]]; then
  log "MISMATCH between MySQL and PostgreSQL - do not switch the application over"
  diff <(echo "$mysql_result") <(echo "$pg_result") >&2 || true
  exit 2
fi

log "Done - data in PostgreSQL matches MySQL"
