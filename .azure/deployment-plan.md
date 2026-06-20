# Azure Deployment Plan

## 1. Summary
- **Project**: `summercafe2026`
- **Mode**: MODIFY existing application
- **Prepared On**: 2026-06-20
- **Status**: Planning
- **Recommended Path**: Azure App Service (Linux) + Azure SQL Database

## 2. Application Analysis
- **App Type**: ASP.NET Core Blazor Server web app (`App/App.csproj`)
- **Runtime**: .NET 10 (`net10.0`)
- **Data Store**: Azure SQL Database via `SqlConnectionString`
- **Deployment Shape**: Single web app with server-side rendering and SQL connectivity
- **Authentication State**: App currently does not show Azure auth middleware in `App/Program.cs`; deployment plan assumes app hosting first, with auth hardening handled separately if required

## 3. Proposed Azure Architecture
- **Hosting**: Azure App Service (Linux)
- **Compute Plan**: Basic or Standard App Service Plan, depending on expected always-on needs
- **Database**: Existing Azure SQL Database reused
- **Identity**: System-assigned managed identity on the web app
- **Configuration**: App Service application setting for `ConnectionStrings__SqlConnectionString`
- **Observability**: Enable App Service logs and standard diagnostics

## 4. Deployment Recipe
- **Recipe Type**: AZCLI-first App Service deployment
- **Why**:
  - Repository does not yet contain `azure.yaml`, `infra/`, or Docker assets
  - App is a straightforward ASP.NET Core web app and can be published directly
  - Fastest safe path is to add deployment documentation and optional automation around App Service publish/deploy steps

## 5. Planned Work
1. Add deployment documentation in a root `README.md`
2. Optionally add lightweight deployment helper assets/scripts if they improve repeatability
3. Verify build/test locally
4. Confirm Azure subscription + location
5. Provision App Service resources
6. Publish and deploy the `App` project
7. Configure the SQL connection string in Azure
8. Smoke test the deployed endpoint

## 6. Required Azure Inputs
- Azure subscription
- Azure region
- Resource group name
- App Service app name (globally unique)
- Existing Azure SQL server/database confirmation

## 7. Risks / Notes
- .NET 10 support in App Service should be verified against the target region/runtime stack at deployment time
- Current connection string uses `Authentication=Active Directory Default;`, so deployment success depends on either:
  - App Service managed identity having access to the Azure SQL database, or
  - replacing with another supported secure auth mode
- Database user provisioning may be required after deploy; `tools/DbAdmin` appears intended to help with this

## 8. Validation Proof
- Pending

## 9. Approval
- Pending user approval