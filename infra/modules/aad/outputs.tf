output "backend_client_id" {
  description = "Client ID of the backend Azure AD app (used by Easy Auth)"
  value       = azuread_application.backend.client_id
}

output "frontend_client_id" {
  description = "Client ID of the frontend SPA Azure AD app (used by MSAL)"
  value       = azuread_application.frontend.client_id
}
