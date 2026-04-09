variable "prefix" {
  description = "Short prefix used in all resource names"
  type        = string
}

variable "environment" {
  description = "Deployment environment"
  type        = string
}

variable "github_org" {
  description = "GitHub organisation or user name (e.g. 'my-org')"
  type        = string
}

variable "github_repo" {
  description = "GitHub repository name (e.g. 'job-board-resume-writer')"
  type        = string
}

variable "subscription_id" {
  description = "Azure subscription ID — used to scope role assignments"
  type        = string
}
