# ── Storage account for Function App internal use ──────────────────────────────
resource "azurerm_storage_account" "functions" {
  name                     = "st${var.prefix}fn${var.environment}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
  tags                     = var.tags
   
}

# ── Storage account for CV uploads + static frontend ──────────────────────────
resource "azurerm_storage_account" "app" {
  name                     = "st${var.prefix}app${var.environment}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  # Enable static website hosting for the React/HTML frontend
  static_website {
    index_document     = "index.html"
    error_404_document = "index.html"
  }

  tags = var.tags
}

# ── Blob container for uploaded CVs ───────────────────────────────────────────
resource "azurerm_storage_container" "cv_uploads" {
  name                  = "cv-uploads"
  storage_account_name  = azurerm_storage_account.app.name
  container_access_type = "private"
}

# ── Blob container for generated resumes ─────────────────────────────────────
resource "azurerm_storage_container" "resumes" {
  name                  = "resumes"
  storage_account_name  = azurerm_storage_account.app.name
  container_access_type = "private"
}
