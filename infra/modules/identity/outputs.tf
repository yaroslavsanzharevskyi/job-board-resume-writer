output "function_app_identity_id" {
  description = "Resource ID of the user-assigned managed identity"
  value       = azurerm_user_assigned_identity.function_app.id
}

output "function_app_principal_id" {
  description = "Object (principal) ID — used for Key Vault access policies and role assignments"
  value       = azurerm_user_assigned_identity.function_app.principal_id
}

output "function_app_client_id" {
  description = "Client ID — set as AZURE_CLIENT_ID in Function App for DefaultAzureCredential"
  value       = azurerm_user_assigned_identity.function_app.client_id
}
