terraform {

    required_version = ">= 1.2.5"

    required_providers {
        azurerm = {
        source  = "hashicorp/azurerm"
        version = ">= 3.0.0"
        }
    }
    
    backend "azurerm" {
      resource_group_name  = "terraform-state-deviceapi"
      storage_account_name = "terraformdeviceapi"
      container_name       = "tfstate"
      key                  = "terraform.tfstate"
    }
}


provider "azurerm" {
  features {
    key_vault {
      purge_soft_deleted_secrets_on_destroy = true
      recover_soft_deleted_secrets          = true
    }
  }
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "resource_group" {
    name     = "vNextAssessment"
    location = var.location
}

resource "azurerm_app_service_plan" "app_service_plan" {
    name                = "vNextAppServicePlanYO"
    location            = var.location
    resource_group_name = azurerm_resource_group.resource_group.name
    kind                = "FunctionApp"

    sku {
        size = "Dynamic"
        tier = "Y1"
    }
}

resource "azurerm_storage_account" "storage_account" {
    name                     = "vnextfuncappstorageyo"
    location                 = var.location
    resource_group_name      = azurerm_resource_group.resource_group.name
    account_tier             = "Standard"
    account_replication_type = "LRS"
}

resource "azurerm_function_app" "function_app" {
    name                       = var.function_app_name
    location                   = var.location
    resource_group_name        = azurerm_resource_group.resource_group.name
    app_service_plan_id        = azurerm_app_service_plan.app_service_plan.id
    storage_account_name       = azurerm_storage_account.storage_account.name
    storage_account_access_key = azurerm_storage_account.storage_account.primary_access_key

    app_settings = {
        FUNCTIONS_WORKER_RUNTIME = "dotnet"
        InventoryServiceOptions__BaseUrl = var.inventory_api_url
        InventoryServiceOptions__GetFunctionKey = "@Microsoft.KeyVaule(SecretUri=${azurerm_key_vault.key_vault.vault_uri}secrets/${azurerm_key_vault_secret.inventory_get_function_key.name}/${azurerm_key_vault_secret.inventory_get_function_key.id})"
        InventoryServiceOptions__PostFunctionKey = "@Microsoft.KeyVaule(SecretUri=${azurerm_key_vault.key_vault.vault_uri}secrets/${azurerm_key_vault_secret.inventory_post_function_key.name}/${azurerm_key_vault_secret.inventory_post_function_key.id})"
        CosmosDbOptions__Uri = azurerm_cosmosdb_account.cosmos_db_account.endpoint
        CosmosDbOptions__Key = "@Microsoft.KeyVaule(SecretUri=${azurerm_key_vault.key_vault.vault_uri}secrets/${azurerm_key_vault_secret.cosmos_primary_key.name}/${azurerm_key_vault_secret.cosmos_primary_key.id})"
        CosmosDbOptions__DatabaseName = azurerm_cosmosdb_sql_database.cosmos_database.name
        CosmosDbOptions__ContainerName = "devices"
    }

    identity {
        type = "SystemAssigned"
    }

    site_config {
        dotnet_framework_version = "v6.0"
    }
}

resource "azurerm_cosmosdb_account" "cosmos_db_account" {
    name                = "vnextdevicesyo"
    location            = var.location
    resource_group_name = azurerm_resource_group.resource_group.name
    offer_type          = "Standard"

    enable_automatic_failover = true

    capabilities {
        name = "EnableServerless"
    }

    capabilities {
        name = "DisableRateLimitingResponses"
    }

    consistency_policy  {
        consistency_level = "ConsistentPrefix"
    }

    geo_location {
        location          = "westus"
        failover_priority = 1
    }
}

resource "azurerm_cosmosdb_sql_database" "cosmos_database" {
    name                 = "vNext"
    resource_group_name  = azurerm_resource_group.resource_group.name
    account_name         = azurerm_cosmosdb_account.cosmos_db_account.name
}