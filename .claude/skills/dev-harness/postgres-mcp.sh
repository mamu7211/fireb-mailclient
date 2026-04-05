#!/usr/bin/env bash
# Wrapper that discovers the Aspire-managed Postgres password and launches the MCP server.
set -euo pipefail

CONTAINER=$(podman ps --format '{{.Names}}' 2>/dev/null | grep -i postgres | head -1)
if [ -z "$CONTAINER" ]; then
  echo "No Postgres container found. Is Aspire running?" >&2
  exit 1
fi

PASSWORD=$(podman exec "$CONTAINER" printenv POSTGRES_PASSWORD 2>/dev/null)
if [ -z "$PASSWORD" ]; then
  echo "Could not read POSTGRES_PASSWORD from container $CONTAINER" >&2
  exit 1
fi

exec npx -y @modelcontextprotocol/server-postgres "postgresql://postgres:${PASSWORD}@localhost:15432/mailclientdb"
