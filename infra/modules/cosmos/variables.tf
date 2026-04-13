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

variable "tags" {
  type    = map(string)
  default = {}
}

variable "function_app_principal_id" {
  type        = string
  description = "Object ID of the function app's managed identity — granted Cosmos DB Built-in Data Contributor"
}
