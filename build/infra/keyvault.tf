resource "azurerm_key_vault" "key_vault" {
    name                        = "vNextAppKeyVaultYO"
    location                    = var.location
    resource_group_name         = azure_resource_group.resource_group.name
    enabled_for_disk_encryption = true
    sku_name                    = "standard"
    tenant_id                   = data.azurerm_client_config.current.tenant_id
    soft_delete_retention_days  = 7
    purge_protection_enabled    = false
}

resource "azurerm_key_vault_access_policy" "terraform_key_vault_access_policy" {
    key_vault_id = azurerm_key_vault.key_vault.id
    tenant_id    = data.azurerm_client_config.current.tenant_id
    object_id    = data.azurerm_client_config.current.object_id

    secret_permissions = [
        "Get", "Set", "Delete", "Purge", "Recover"
    ]
}

resource "azurerm_key_vault_access_policy" "functionapp_key_vault_access_policy" {
    key_vault_id = azurerm_key_vault.key_vault.id
    tenant_id    = azurerm_key_vault.key_vault.tenant_id
    object_id    = lookup(azurerm_function_app.function_app.identity[0], "principal_id")

    secret_permissions = [
        "Get"
    ]
}

resource "azurerm_key_vault_secret" "inventory_get_function_key" {
    name         = "InventoryApiGetFunctionKey"
    value        = var.inventory_api_get_function_key
    key_vault_id = azurerm_key_vault.key_vault.id

    depends_on = [azurerm_key_vault_access_policy.terraform_key_vault_access_policy]
}

resource "azurerm_key_vault_secret" "inventory_post_function_key" {
    name         = "InventoryApiPostFunctionKey"
    value        = var.inventory_api_post_function_key
    key_vault_id = azurerm_key_vault.key_vault.id

    depends_on = [azurerm_key_vault_access_policy.terraform_key_vault_access_policy]
}

resource "azurerm_key_vault_secret" "cosmos_primary_key" {
    name         = "CosmosPrimaryKey"
    value        = azurerm_cosmosdb_account.cosmos_db_account.primary_key
    key_vault_id = azurerm_key_vault.key_vault.id

    depends_on = [azurerm_key_vault_access_policy.terraform_key_vault_access_policy]
}
