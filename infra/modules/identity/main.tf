# ── User-assigned managed identity for the Function App ───────────────────────
resource "azurerm_user_assigned_identity" "function_app" {
  name                = "id-${var.prefix}-fn-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags
}

# ── Role: Storage Blob Data Contributor (CV uploads + resumes container) ──────
resource "azurerm_role_assignment" "function_blob_contributor" {
  scope                = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.function_app.principal_id
}

# ── Role: Cosmos DB Built-in Data Contributor ─────────────────────────────────
resource "azurerm_role_assignment" "function_cosmos_contributor" {
  scope                = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}"
  role_definition_name = "DocumentDB Account Contributor"
  principal_id         = azurerm_user_assigned_identity.function_app.principal_id
}

data "azurerm_client_config" "current" {}
