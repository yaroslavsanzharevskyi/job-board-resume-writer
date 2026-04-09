output "client_id" {
  description = "Application (client) ID — set as AZURE_CLIENT_ID in GitHub secrets"
  value       = azuread_application.github_actions.client_id
}

output "service_principal_object_id" {
  description = "Object ID of the service principal"
  value       = azuread_service_principal.github_actions.object_id
}
