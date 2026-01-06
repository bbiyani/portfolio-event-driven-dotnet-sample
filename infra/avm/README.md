# Infrastructure (Azure Verified Modules)

This folder provisions the Azure infrastructure for the sample using **Azure Verified Modules (AVM)** and Bicep.

## Resources provisioned

- Resource Group
- Key Vault (RBAC enabled)
- Storage Account (Blob Storage)
- Cosmos DB (SQL API)
- Service Bus (Standard)
- App Service Plan + App Service (API host)

## Parameters

The deployment is parameterized by environment and shared tags.

- `environment`: `staging` or `prod`
- `tags`: object applied to all resources
- `location`: Azure region
- `namePrefix`: prefix for resource names

Environment-specific parameter files live in `infra/avm/parameters/`.

## Required permissions

To deploy the infrastructure, the identity running the deployment needs:

- **Contributor** (or higher) on the target subscription (or management group) to create the resource group and resources.
- **Key Vault Contributor** or **Key Vault Administrator** on the resource group if you need to manage secrets after deployment.

## Deploy locally

Prerequisites:

- Azure CLI (`az`) authenticated to the target subscription.
- Bicep CLI available via `az bicep install` (optional; Azure CLI can build Bicep automatically).

Example command:

```bash
az deployment sub create \
  --location uksouth \
  --template-file infra/avm/main.bicep \
  --parameters infra/avm/parameters/staging.bicepparam
```

## Deploy via script

Use the helper script from the repo root:

```bash
./deploy/infra-deploy.sh staging uksouth
```

The script selects the matching parameter file and runs the subscription-scope deployment before app deployment.
