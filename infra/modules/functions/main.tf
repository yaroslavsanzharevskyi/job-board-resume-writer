# ── App Service Plan (Consumption / Serverless) ───────────────────────────────
resource "azurerm_service_plan" "this" {
  name                = "asp-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "Y1"   # Consumption plan
  tags                = var.tags
}

# ── Function App ──────────────────────────────────────────────────────────────
resource "azurerm_linux_function_app" "this" {
  name                       = "func-${var.prefix}-${var.environment}"
  resource_group_name        = var.resource_group_name
  location                   = var.location
  service_plan_id            = azurerm_service_plan.this.id
  storage_account_name       = var.storage_account_name
  storage_account_access_key = var.storage_account_access_key

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  key_vault_reference_identity_id = var.identity_id

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
    cors {
      allowed_origins = ["*"]
    }
  }

  app_settings = {
    # Runtime
    FUNCTIONS_WORKER_RUNTIME                = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                = "1"
    AZURE_CLIENT_ID                         = var.client_id   # for DefaultAzureCredential
    APPLICATIONINSIGHTS_CONNECTION_STRING   = var.appinsights_connection_string

    # Secrets — resolved from Key Vault at runtime
    DATABRICKS_HOST            = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-host/)"
    DATABRICKS_TOKEN           = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-token/)"
    DATABRICKS_HTTP_PATH       = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-http-path/)"
    DATABRICKS_CATALOG         = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-catalog/)"
    DATABRICKS_SCHEMA          = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-schema/)"
    DATABRICKS_TABLE           = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/databricks-table/)"
    FOUNDRY_ENDPOINT           = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/foundry-endpoint/)"
    FOUNDRY_API_KEY            = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/foundry-api-key/)"
    FOUNDRY_DEPLOYMENT_NAME    = "@Microsoft.KeyVault(SecretUri=${var.keyvault_uri}secrets/foundry-deployment-name/)"

    # Cosmos DB — managed identity, no connection string
    COSMOS_ACCOUNT_ENDPOINT    = var.cosmos_endpoint
    COSMOS_DATABASE_NAME       = var.cosmos_database_name

    # Blob Storage — managed identity, no connection string
    BLOB_STORAGE_URL           = var.blob_storage_url
    BLOB_CONTAINER_NAME        = var.blob_container_name
  }

  tags = var.tags
}
