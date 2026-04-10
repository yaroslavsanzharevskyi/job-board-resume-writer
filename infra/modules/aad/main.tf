# ── Backend app registration (Function App) ───────────────────────────────────
# Exposes an OAuth2 scope that the frontend requests a token for.

resource "azuread_application" "backend" {
  display_name = "app-${var.prefix}-${var.environment}-backend"

  api {
    requested_access_token_version = 2

    oauth2_permission_scope {
      admin_consent_description  = "Allow the frontend to call the resume generator API"
      admin_consent_display_name = "API Access"
      enabled                    = true
      id                         = "00000000-0000-0000-0000-000000000001"
      type                       = "User"
      user_consent_description   = "Allow the app to call the resume generator API on your behalf"
      user_consent_display_name  = "API Access"
      value                      = "api.access"
    }
  }
}

resource "azuread_service_principal" "backend" {
  client_id = azuread_application.backend.client_id
}

# ── Frontend app registration (SPA) ───────────────────────────────────────────
# Single-page application; acquires tokens for the backend scope via MSAL.

resource "azuread_application" "frontend" {
  display_name = "app-${var.prefix}-${var.environment}-frontend"

  single_page_application {
    redirect_uris = var.frontend_redirect_uris
  }

  required_resource_access {
    resource_app_id = azuread_application.backend.client_id

    resource_access {
      id   = "00000000-0000-0000-0000-000000000001"
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "frontend" {
  client_id = azuread_application.frontend.client_id
}
