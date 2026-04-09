output "function_storage_account_name" {
  value = azurerm_storage_account.functions.name
}

output "function_storage_account_access_key" {
  value     = azurerm_storage_account.functions.primary_access_key
  sensitive = true
}

output "app_storage_account_name" {
  value = azurerm_storage_account.app.name
}

output "static_website_url" {
  value = azurerm_storage_account.app.primary_web_endpoint
}

output "cv_container_url" {
  value = "${azurerm_storage_account.app.primary_blob_endpoint}${azurerm_storage_container.cv_uploads.name}"
}

output "cv_container_name" {
  value = azurerm_storage_container.cv_uploads.name
}

output "resumes_container_name" {
  value = azurerm_storage_container.resumes.name
}
