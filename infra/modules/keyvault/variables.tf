variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "prefix" {
  type = string
}

variable "environment" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "deployer_object_id" {
  description = "Object ID of the principal running Terraform (gets full secret CRUD)"
  type        = string
}

variable "function_app_principal_id" {
  description = "Object ID of the Function App managed identity (gets Get + List)"
  type        = string
}

variable "secrets" {
  description = "Map of secret name → value to store in Key Vault"
  type        = map(string)
  sensitive   = true
  default     = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
