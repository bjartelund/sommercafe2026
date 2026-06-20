#!/usr/bin/env bash
set -euo pipefail

RESOURCE_GROUP="${RESOURCE_GROUP:-app}"
WEBAPP_NAME="${WEBAPP_NAME:-summercafe2026}"
PROJECT_PATH="${PROJECT_PATH:-App/App.csproj}"
CONFIGURATION="${CONFIGURATION:-Release}"
PUBLISH_DIR="${PUBLISH_DIR:-/tmp/summercafe2026-publish}"
ZIP_PATH="${ZIP_PATH:-/tmp/summercafe2026.zip}"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command not found: $1" >&2
    exit 1
  fi
}

require_command dotnet
require_command az
require_command zip

if ! az account show >/dev/null 2>&1; then
  echo "Azure CLI is not logged in. Run: az login" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"
rm -f "$ZIP_PATH"

echo "Publishing $PROJECT_PATH..."
dotnet publish "$REPO_ROOT/$PROJECT_PATH" -c "$CONFIGURATION" -o "$PUBLISH_DIR"

echo "Creating deployment zip..."
(
  cd "$PUBLISH_DIR"
  zip -qr "$ZIP_PATH" .
)

echo "Deploying to App Service '$WEBAPP_NAME' in resource group '$RESOURCE_GROUP'..."
az webapp deploy \
  --resource-group "$RESOURCE_GROUP" \
  --name "$WEBAPP_NAME" \
  --src-path "$ZIP_PATH" \
  --type zip \
  --restart true \
  --track-status true \
  --async false

echo "Deployment complete. App URL: https://$WEBAPP_NAME.azurewebsites.net"