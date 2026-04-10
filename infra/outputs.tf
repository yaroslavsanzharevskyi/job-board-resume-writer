output "resource_group_name" {
  description = "Name of the main resource group"
  value       = module.resource_group.name
}

output "function_app_name" {
  description = "Name of the Azure Function App"
  value       = module.functions.function_app_name
}

output "function_app_default_hostname" {
  description = "Default hostname of the Function App"
  value       = module.functions.function_app_default_hostname
}

output "function_app_api_base_url" {
  description = "Base URL for API calls"
  value       = "https://${module.functions.function_app_default_hostname}/api"
}

output "static_website_url" {
  description = "Primary endpoint for the Static Web App (frontend)"
  value       = module.storage.static_website_url
}

output "cv_storage_container_url" {
  description = "URL of the blob container for uploaded CVs"
  value       = module.storage.cv_container_url
}

output "keyvault_name" {
  description = "Name of the Key Vault"
  value       = module.keyvault.name
}

output "keyvault_uri" {
  description = "URI of the Key Vault"
  value       = module.keyvault.vault_uri
}

output "cosmos_account_name" {
  description = "Cosmos DB account name"
  value       = module.cosmos.account_name
}

output "cosmos_endpoint" {
  description = "Cosmos DB endpoint"
  value       = module.cosmos.endpoint
}

output "function_app_principal_id" {
  description = "Managed identity principal ID of the Function App (for RBAC)"
  value       = module.identity.function_app_principal_id
}

# ── GitHub Actions SP ──────────────────────────────────────────────────────────

output "github_actions_client_id" {
  description = "AZURE_CLIENT_ID — add as a GitHub Actions secret"
  value       = module.github_sp.client_id
}

output "github_actions_tenant_id" {
  description = "AZURE_TENANT_ID — add as a GitHub Actions secret"
  value       = data.azurerm_client_config.current.tenant_id
}

output "github_actions_subscription_id" {
  description = "AZURE_SUBSCRIPTION_ID — add as a GitHub Actions secret"
  value       = data.azurerm_client_config.current.subscription_id
}

# ── Azure AD (MSAL / Easy Auth) ────────────────────────────────────────────────

output "aad_tenant_id" {
  description = "Azure AD tenant ID — set as VITE_AAD_TENANT_ID in the frontend build"
  value       = data.azurerm_client_config.current.tenant_id
}

output "aad_frontend_client_id" {
  description = "Frontend SPA Azure AD client ID — set as VITE_AAD_CLIENT_ID in the frontend build"
  value       = module.aad.frontend_client_id
}

output "aad_backend_client_id" {
  description = "Backend Azure AD client ID — used to construct the MSAL scope (api://<id>/api.access)"
  value       = module.aad.backend_client_id
}
