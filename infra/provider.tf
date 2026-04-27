terraform {
  required_version = ">= 1.14.9"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.70.0"
    }
  }

}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}