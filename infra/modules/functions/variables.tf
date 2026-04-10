variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "prefix" {
  type = string
}

variable "environment" {
  type = string
}

variable "storage_account_name" {
  type = string
}

variable "storage_account_access_key" {
  type      = string
  sensitive = true
}

variable "identity_id" {
  description = "User-assigned managed identity resource ID"
  type        = string
}

variable "client_id" {
  description = "User-assigned managed identity client ID (for DefaultAzureCredential)"
  type        = string
}

variable "keyvault_uri" {
  description = "Key Vault URI (https://<name>.vault.azure.net/)"
  type        = string
}

variable "cosmos_endpoint" {
  description = "Cosmos DB account endpoint URL — managed identity auth, no key needed"
  type        = string
}

variable "cosmos_database_name" {
  type = string
}

variable "blob_storage_url" {
  description = "App storage account blob endpoint (https://<account>.blob.core.windows.net)"
  type        = string
}

variable "blob_container_name" {
  description = "Name of the blob container used for resume storage"
  type        = string
}

variable "appinsights_connection_string" {
  description = "Application Insights connection string"
  type        = string
  sensitive   = true
}

variable "aad_backend_client_id" {
  description = "Client ID of the backend Azure AD app registration (for Easy Auth)"
  type        = string
}

variable "aad_backend_api_uri" {
  description = "Identifier URI of the backend app (e.g. api://jbrw-dev-resume-api) — used as the allowed audience in Easy Auth"
  type        = string
}

variable "tenant_id" {
  description = "Azure AD tenant ID"
  type        = string
}

variable "tags" {
  type    = map(string)
  default = {}
}
