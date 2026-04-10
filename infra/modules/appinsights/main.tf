resource "azurerm_application_insights" "this" {
  name                = "appi-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  application_type    = "web"
  tags                = var.tags

  # Azure auto-assigns a Log Analytics workspace on creation (newer API versions).
  # The workspace_id is not in Terraform state but is set on the real resource,
  # so Terraform would attempt to null it out on every plan — which Azure rejects.
  lifecycle {
    ignore_changes = [workspace_id]
  }
}
