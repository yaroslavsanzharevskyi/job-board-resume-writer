variable "prefix" {
  type = string
}

variable "environment" {
  type = string
}

variable "frontend_redirect_uris" {
  description = "SPA redirect URIs for the frontend app (e.g. Static Web App URL + /auth/callback)"
  type        = list(string)
}
