# Summer Café 2026

Employee-facing café app built with ASP.NET Core Blazor Server, MudBlazor, and Azure SQL.

## Current Azure deployment

This repository is already deployed to an existing Azure App Service:

- **Web app**: `summercafe2026`
- **Resource group**: `app`
- **Region**: `West Europe`
- **URL**: `https://summercafe2026.azurewebsites.net`
- **Runtime**: `DOTNETCORE|10.0`
- **App Service plan**: `summercafe2026-plan` (`F1`, Linux)
- **Database**: Azure SQL `sommercafe2026` / `free-sql-db-8725977`

The production app currently reads its SQL connection string from the App Service setting:

- `ConnectionStrings__SqlConnectionString`

## Local development

### Prerequisites

- .NET SDK 10
- Azure CLI (`az`) if you want to deploy updates
- Access to the Azure subscription that hosts `summercafe2026`

### Run locally

```zsh
cd "/Users/bjarte.lund/RiderProjects/summercafe2026"
dotnet run --project App
```

### Run tests

```zsh
cd "/Users/bjarte.lund/RiderProjects/summercafe2026"
dotnet test --solution "summercafe2026.slnx"
```

## Deploying an update to the existing App Service

This is the verified update path used for the current deployment.

### Option 1: Use the helper script

```zsh
cd "/Users/bjarte.lund/RiderProjects/summercafe2026"
./scripts/deploy-appservice-update.sh
```

The script will:
1. Publish `App/App.csproj` in `Release`
2. Zip the publish output
3. Deploy it to the existing Azure Web App using `az webapp deploy`

### Option 2: Run the steps manually

```zsh
cd "/Users/bjarte.lund/RiderProjects/summercafe2026"
dotnet publish "App/App.csproj" -c Release -o "/tmp/summercafe2026-publish"
```

```zsh
rm -f "/tmp/summercafe2026.zip"
cd "/tmp/summercafe2026-publish"
zip -r "/tmp/summercafe2026.zip" .
```

```zsh
az webapp deploy \
  --resource-group "app" \
  --name "summercafe2026" \
  --src-path "/tmp/summercafe2026.zip" \
  --type zip \
  --restart true \
  --track-status true \
  --async false
```

## Deployment prerequisites

Before deploying an update, make sure:

1. You are logged into Azure CLI:

```zsh
az login
az account show
```

2. The correct subscription is selected:

```zsh
az account set --subscription "summercafe2026"
```

3. The app still exists and is running:

```zsh
az webapp show \
  --resource-group "app" \
  --name "summercafe2026" \
  --query "{name:name,state:state,defaultHostName:defaultHostName,linuxFxVersion:siteConfig.linuxFxVersion}" \
  --output json
```

## Production configuration

Check the current production app settings:

```zsh
az webapp config appsettings list \
  --resource-group "app" \
  --name "summercafe2026" \
  --output json
```

At the time of writing, production includes:

- `ConnectionStrings__SqlConnectionString`
- `ASPNETCORE_ENVIRONMENT=Production`
- `WEBSITE_AUTH_AAD_ALLOWED_TENANTS`

## Database notes

The app currently uses Azure SQL with `Authentication=Active Directory Default;` in the connection string. The deployed App Service has a system-assigned managed identity enabled.

If database access breaks after changes, verify:

1. The managed identity still exists on the web app
2. The SQL database user/permissions still exist
3. The app setting `ConnectionStrings__SqlConnectionString` is still present

There is a helper tool in `tools/DbAdmin` that appears intended for database setup / identity user provisioning.

Example usage:

```zsh
cd "/Users/bjarte.lund/RiderProjects/summercafe2026"
dotnet run --project tools/DbAdmin/DbAdmin.csproj -- \
  server=sommercafe2026.database.windows.net \
  database=free-sql-db-8725977 \
  migrate=true
```

## Post-deploy checks

Recommended checks after each deployment:

```zsh
az webapp show \
  --resource-group "app" \
  --name "summercafe2026" \
  --query "{state:state,defaultHostName:defaultHostName}" \
  --output json
```

Then open:

- `https://summercafe2026.azurewebsites.net`

And verify:

- the app loads
- navigation works
- product/order/expense pages still load
- database-backed pages return data

## Notes / caveats

- The current App Service is on the **Free (`F1`)** plan.
- The current app reports `httpsOnly: false`; if you want, this can be tightened later.
- The app code uses `EnsureCreatedAsync()` in development, not production migrations. Schema changes should be handled deliberately before or alongside deployment.