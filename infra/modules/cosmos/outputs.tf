output "account_name" {
  value = azurerm_cosmosdb_account.this.name
}

output "endpoint" {
  value = azurerm_cosmosdb_account.this.endpoint
}

output "connection_string" {
  value     = azurerm_cosmosdb_account.this.primary_sql_connection_string
  sensitive = true
}

output "database_name" {
  value = azurerm_cosmosdb_sql_database.this.name
}

output "resumes_container_name" {
  value = azurerm_cosmosdb_sql_container.resumes.name
}

output "jobs_cache_container_name" {
  value = azurerm_cosmosdb_sql_container.jobs_cache.name
}
