terraform {
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 3.90" }
    azuread = { source = "hashicorp/azuread", version = "~> 2.47" }
  }

  # Terraform state lives in Azure Blob (not your laptop)
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "tfstate21612"
    container_name       = "tfstate"
    key                  = "dev.resume-gen.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
  }
}

provider "azuread" {}

# ── Modules ────────────────────────────────────────────────────────────────────

module "resource_group" {
  source = "./modules/resource_group"

  name     = "rg-${var.prefix}-${var.environment}"
  location = var.location
  tags     = local.common_tags
}

module "identity" {
  source = "./modules/identity"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

module "keyvault" {
  source = "./modules/keyvault"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment
  tenant_id           = data.azurerm_client_config.current.tenant_id

  function_app_principal_id = module.identity.function_app_principal_id
  github_actions_object_id  = module.github_sp.service_principal_object_id
  deployer_object_id        = var.deployer_object_id

  secrets = {
    databricks-host            = var.databricks_host
    databricks-token           = var.databricks_token
    databricks-http-path       = var.databricks_http_path
    databricks-catalog         = var.databricks_catalog
    databricks-schema          = var.databricks_schema
    databricks-table           = var.databricks_table
    foundry-endpoint           = var.foundry_endpoint
    foundry-api-key            = var.foundry_api_key
    foundry-deployment-name    = var.foundry_deployment_name
  }

  tags = local.common_tags
}

module "appinsights" {
  source = "./modules/appinsights"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

module "storage" {
  source = "./modules/storage"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

module "cosmos" {
  source = "./modules/cosmos"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags

  function_app_principal_id = module.identity.function_app_principal_id
}

module "functions" {
  source = "./modules/functions"

  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  prefix              = var.prefix
  environment         = var.environment

  storage_account_name       = module.storage.function_storage_account_name
  storage_account_access_key = module.storage.function_storage_account_access_key
  identity_id                = module.identity.function_app_identity_id
  client_id                  = module.identity.function_app_client_id
  keyvault_uri               = module.keyvault.vault_uri

  cosmos_endpoint            = module.cosmos.endpoint
  cosmos_database_name       = module.cosmos.database_name

  blob_storage_url           = "https://${module.storage.app_storage_account_name}.blob.core.windows.net"
  blob_container_name        = module.storage.resumes_container_name

  appinsights_connection_string = module.appinsights.connection_string

  aad_backend_client_id = module.aad.backend_client_id
  aad_backend_api_uri   = module.aad.backend_api_uri
  tenant_id             = data.azurerm_client_config.current.tenant_id

  tags = local.common_tags
}

module "aad" {
  source = "./modules/aad"

  prefix      = var.prefix
  environment = var.environment

  frontend_redirect_uris = [
    module.storage.static_website_url,
    "http://localhost:5173/",
  ]
}

module "github_sp" {
  source = "./modules/github_sp"

  prefix          = var.prefix
  environment     = var.environment
  github_org      = var.github_org
  github_repo     = var.github_repo
  subscription_id = data.azurerm_client_config.current.subscription_id
}

# ── Data sources ───────────────────────────────────────────────────────────────

data "azurerm_client_config" "current" {}

# ── Locals ─────────────────────────────────────────────────────────────────────

locals {
  common_tags = {
    project     = "job-board-resume-writer"
    environment = var.environment
    managed_by  = "terraform"
  }
}
