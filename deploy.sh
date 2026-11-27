#!/usr/bin/env bash
#
# Deploys the PSE API on the server. Runs there, piped over SSH by the Deploy workflow:
#   ssh user@server 'bash -s' < deploy.sh
#
# The API runs as its own Docker Compose project next to the Inviser stacks. Its compose
# file and env file (with the database credentials) live on the server, not in this repo.
#
# Overrides (environment):
#   PSE_DEPLOY_DIR   directory with the stack definitions (default: ~/inviser/deploy)
#   PSE_PROJECT      compose project name                 (default: inviser-pse)
#   PSE_SERVICE      compose service of the API           (default: app_pse)
#
set -euo pipefail

deploy_dir="${PSE_DEPLOY_DIR:-$HOME/inviser/deploy}"
project="${PSE_PROJECT:-inviser-pse}"
service="${PSE_SERVICE:-app_pse}"

echo "Deploy script started"
cd "$deploy_dir"

compose() {
  docker compose --project-name "$project" --env-file pse/.env -f pse/docker-compose.yml "$@"
}

compose config --quiet
compose pull "$service"
# Only the API is recreated; the shared proxy and database stacks are not touched
compose up -d --no-deps "$service"

echo "Waiting for /healthz"
for _ in $(seq 1 30); do
  if compose exec -T "$service" wget -q -O - http://127.0.0.1/healthz 2> /dev/null; then
    echo
    compose ps "$service"
    echo "Deploy script finished execution"
    exit 0
  fi
  sleep 2
done

echo "API did not become healthy, last log lines:" >&2
compose logs --tail 50 "$service" >&2
exit 1
