# ── Key Vault ──────────────────────────────────────────────────────────────────
resource "azurerm_key_vault" "this" {
  name                        = "kv-${var.prefix}-${var.environment}"
  resource_group_name         = var.resource_group_name
  location                    = var.location
  tenant_id                   = var.tenant_id
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false   # set true for prod

  tags = var.tags
}

# ── Access policy: local deployer (skipped in CI where deployer_object_id is empty) ──
resource "azurerm_key_vault_access_policy" "deployer" {
  count = var.deployer_object_id != "" ? 1 : 0

  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = var.tenant_id
  object_id    = var.deployer_object_id

  secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
}

# ── Access policy: GitHub Actions SP ─────────────────────────────────────────
resource "azurerm_key_vault_access_policy" "github_actions" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = var.tenant_id
  object_id    = var.github_actions_object_id

  secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
}

# ── Access policy: Function App managed identity ───────────────────────────────
resource "azurerm_key_vault_access_policy" "function_app" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = var.tenant_id
  object_id    = var.function_app_principal_id

  secret_permissions = ["Get", "List"]
}

# ── Secrets ────────────────────────────────────────────────────────────────────
resource "azurerm_key_vault_secret" "secrets" {
  # Secret names are not sensitive; use nonsensitive() so Terraform can use them
  # as resource keys without exposing secret values in the plan.
  for_each = nonsensitive(toset(keys(var.secrets)))

  name         = each.key
  value        = var.secrets[each.key]
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [
    azurerm_key_vault_access_policy.deployer,
    azurerm_key_vault_access_policy.github_actions,
  ]
}
