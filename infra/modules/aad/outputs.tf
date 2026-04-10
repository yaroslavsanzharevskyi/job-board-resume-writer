output "backend_client_id" {
  description = "Client ID of the backend Azure AD app (used by Easy Auth)"
  value       = azuread_application.backend.client_id
}

output "frontend_client_id" {
  description = "Client ID of the frontend SPA Azure AD app (used by MSAL)"
  value       = azuread_application.frontend.client_id
}

output "backend_api_uri" {
  description = "Identifier URI of the backend app (used as Easy Auth allowed_audience)"
  value       = local.backend_api_uri
}

output "backend_api_scope" {
  description = "Full OAuth2 scope string the frontend requests (api://<uri>/api.access)"
  value       = local.backend_api_scope
}
