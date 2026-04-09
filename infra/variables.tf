variable "prefix" {
  description = "Short prefix used in all resource names (e.g. 'jbrw')"
  type        = string
  default     = "jbrw"
}

variable "environment" {
  description = "Deployment environment: dev | staging | prod"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be one of: dev, staging, prod"
  }
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "eastus"
}

# ── Databricks ─────────────────────────────────────────────────────────────────

variable "databricks_host" {
  description = "Databricks workspace URL (https://<workspace>.azuredatabricks.net)"
  type        = string
  sensitive   = true
}

variable "databricks_token" {
  description = "Databricks personal access token"
  type        = string
  sensitive   = true
}

variable "databricks_http_path" {
  description = "SQL warehouse HTTP path (/sql/1.0/warehouses/<id>)"
  type        = string
  sensitive   = true
}

variable "databricks_catalog" {
  description = "Unity Catalog name (or hive_metastore)"
  type        = string
  default     = "hive_metastore"
}

variable "databricks_schema" {
  description = "Schema / database name inside the catalog"
  type        = string
  default     = "default"
}

variable "databricks_table" {
  description = "Delta table containing Adzuna job postings"
  type        = string
  default     = "adzuna_jobs"
}

# ── Azure AI Foundry ───────────────────────────────────────────────────────────

variable "foundry_endpoint" {
  description = "Azure OpenAI / Foundry endpoint URL"
  type        = string
  sensitive   = true
}

variable "foundry_api_key" {
  description = "Azure OpenAI / Foundry API key"
  type        = string
  sensitive   = true
}

variable "foundry_deployment_name" {
  description = "GPT-4o deployment name in Foundry"
  type        = string
  default     = "gpt-4o-resume"
}

# ── GitHub Actions SP ──────────────────────────────────────────────────────────

variable "github_org" {
  description = "GitHub organisation or user name (e.g. 'my-org')"
  type        = string
}

variable "github_repo" {
  description = "GitHub repository name (e.g. 'job-board-resume-writer')"
  type        = string
  default     = "job-board-resume-writer"
}
