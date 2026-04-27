variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
  sensitive   = true
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "East US 2"
}

variable "environment" {
  description = "Deployment environment: staging | prod"
  type        = string

  validation {
    condition     = contains(["staging", "prod"], var.environment)
    error_message = "environment must be 'staging' or 'prod'."
  }
}

variable "api_key" {
  description = "Static API key validated by the middleware (X-Parse-REST-API-Key)"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "Secret used for JWT signing/validation"
  type        = string
  sensitive   = true
}

variable "ghcr_username" {
  description = "GitHub username for pulling images from GHCR"
  type        = string
}

variable "ghcr_token" {
  description = "GitHub PAT with read:packages scope for GHCR"
  type        = string
  sensitive   = true
}

variable "ghcr_image" {
  description = "Full GHCR image path"
  type        = string
}

variable "image_tag" {
  description = "Docker image tag to deploy (e.g. sha-abc1234 or latest)"
  type        = string
  default     = "latest"
}
