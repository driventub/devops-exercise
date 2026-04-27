# ── Resource Group ────────────────────────────────────────────────────────────

resource "azurerm_resource_group" "main" {
  name     = "rg-devops-${var.environment}"
  location = var.location
}

# ── Log Analytics (required by Container App Environment) ─────────────────────

resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-devops-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

# ── Container App Environment ────────────────────────────────────────────────

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-devops-${var.environment}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  logs_destination           = "log-analytics"
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
}

resource "azurerm_container_app" "api" {
  name                         = "ca-devops-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  secret {
    name  = "api-key"
    value = var.api_key
  }

  secret {
    name  = "jwt-secret"
    value = var.jwt_secret
  }

  secret {
    name  = "ghcr-token"
    value = var.ghcr_token
  }

  registry {
    server               = "ghcr.io"
    username             = var.ghcr_username
    password_secret_name = "ghcr-token"
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = 2
    max_replicas = 5

    container {
      name   = "devops-api"
      image  = "${var.ghcr_image}:${var.image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "ApiKey"
        secret_name = "api-key"
      }

      env {
        name        = "JwtSecret"
        secret_name = "jwt-secret"
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment == "prod" ? "Production" : "Staging"
      }

      liveness_probe {
        transport        = "HTTP"
        path             = "/health"
        port             = 8080
        initial_delay    = 5
        interval_seconds = 15
      }

      readiness_probe {
        transport        = "HTTP"
        path             = "/health"
        port             = 8080
        interval_seconds = 10
      }
    }

    # HTTP scale rule — for scaling when needed
    http_scale_rule {
      name                = "http-scale"
      concurrent_requests = "20"
    }
  }
}
