# ── Cosmos DB account ──────────────────────────────────────────────────────────
resource "azurerm_cosmosdb_account" "this" {
  name                = "cosmos-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  free_tier_enabled   = true

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  tags = var.tags
}

# ── Database ───────────────────────────────────────────────────────────────────
resource "azurerm_cosmosdb_sql_database" "this" {
  name                = "resumedb"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
}

# ── Containers ─────────────────────────────────────────────────────────────────

# Generated resumes — partition by jobId
resource "azurerm_cosmosdb_sql_container" "resumes" {
  name                = "resumes"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
  database_name       = azurerm_cosmosdb_sql_database.this.name
  partition_key_paths = ["/jobId"]

  default_ttl = 2592000   # 30 days in seconds

  indexing_policy {
    indexing_mode = "consistent"
    included_path { path = "/*" }
    excluded_path { path = "/_etag/?" }
  }
}

# ── RBAC — grant function app identity read/write access to data ───────────────
# "Cosmos DB Built-in Data Contributor" (id ends in ...0002) allows all data-plane
# operations including readMetadata, which the SDK calls on startup.
resource "azurerm_cosmosdb_sql_role_assignment" "function_app" {
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
  role_definition_id  = "${azurerm_cosmosdb_account.this.id}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002"
  principal_id        = var.function_app_principal_id
  scope               = azurerm_cosmosdb_account.this.id
}

# Job cache — partition by category
resource "azurerm_cosmosdb_sql_container" "jobs_cache" {
  name                = "jobs-cache"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
  database_name       = azurerm_cosmosdb_sql_database.this.name
  partition_key_paths = ["/category"]

  default_ttl = 3600   # 1 hour — refreshed from Databricks

  indexing_policy {
    indexing_mode = "consistent"
    included_path { path = "/*" }
    excluded_path { path = "/_etag/?" }
  }
}
