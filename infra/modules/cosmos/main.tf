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
