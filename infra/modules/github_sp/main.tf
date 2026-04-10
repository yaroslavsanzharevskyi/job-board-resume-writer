# ── App registration ───────────────────────────────────────────────────────────
resource "azuread_application" "github_actions" {
  display_name = "sp-${var.prefix}-github-actions-${var.environment}"
}

# ── Service principal ──────────────────────────────────────────────────────────
resource "azuread_service_principal" "github_actions" {
  client_id = azuread_application.github_actions.client_id
}

# ── OIDC federated credentials ─────────────────────────────────────────────────
# Covers: push / workflow_dispatch on main branch
resource "azuread_application_federated_identity_credential" "main_branch" {
  application_id = azuread_application.github_actions.id
  display_name   = "github-actions-main-branch"
  description    = "GitHub Actions OIDC — push to main"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.github_org}/${var.github_repo}:ref:refs/heads/main"
}

# Covers: jobs that target environment: production
resource "azuread_application_federated_identity_credential" "production_env" {
  application_id = azuread_application.github_actions.id
  display_name   = "github-actions-production-env"
  description    = "GitHub Actions OIDC — environment: production"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.github_org}/${var.github_repo}:environment:production"
}

# ── Microsoft Graph — Application.ReadWrite.OwnedBy ───────────────────────────
# Allows the GitHub Actions SP to create and manage Azure AD app registrations
# that it owns (e.g. the backend/frontend app registrations for Easy Auth).

data "azuread_application_published_app_ids" "well_known" {}

data "azuread_service_principal" "msgraph" {
  client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
}

resource "azuread_app_role_assignment" "msgraph_app_readwrite_owned_by" {
  app_role_id         = data.azuread_service_principal.msgraph.app_role_ids["Application.ReadWrite.OwnedBy"]
  principal_object_id = azuread_service_principal.github_actions.object_id
  resource_object_id  = data.azuread_service_principal.msgraph.object_id
}

# ── Role assignments ───────────────────────────────────────────────────────────
# Contributor — create / update / delete all Azure resources
resource "azurerm_role_assignment" "contributor" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.github_actions.object_id
}

# User Access Administrator — required so Terraform can create role assignments
# (e.g. granting the Function App managed identity access to Blob / Cosmos)
resource "azurerm_role_assignment" "user_access_admin" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "User Access Administrator"
  principal_id         = azuread_service_principal.github_actions.object_id
}
