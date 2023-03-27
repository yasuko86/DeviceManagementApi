param (
    [Parameter(Mandatory=$true)][string]$Location
    [Parameter(Mandatory=$true)][string]$SubscriptionId
    [Parameter(Mandatory=$true)][string]$InventoryApiUrl
    [Parameter(Mandatory=$true)][string]$GetFunctionKey
    [Parameter(Mandatory=$true)][string]$PostFunctionKey
    [Parameter(Mandatory=$true)][string]$FunctionAppName
)

$TfResourceGroupName = "terraform-state-deviceapi"
$TfStorageAccountName = "terraformdeviceapi"
$TfContainerName = "tfstate"
$TfBlobKey = "terraform.tfstate"
$Workspace = "vNext"

Set-Location build/infra
terraform init -backend-config="subscription_id=$SubscriptionId" `
               -backend-config="resource_group_name=$TfResourceGroupName" `
               -backend-config="storage_account_name=$TfStorageAccountName" `
               -backend-config="container_name=$TfContainerName" `
               -backend-config="key=$TfBlobKey"

terraform workspace select "$Workspace"
if (!$?) {
    terraform workspace new "$Workspace"
    if (!$?) {
        exit 1
    }
}

terraform state replace-provider -auto-approve registry.terraform.io/-/asurerm registry.terraform.io/hashicorp/azurerm

terraform plan -var="location=$Location" `
               -var="inventory_api_url=$InventoryApiUrl" `
               -var="inventory_api_get_function_key=$GetFunctionKey" `
               -var="inventory_api_post_function_key=$PostFunctionKey" `
               -var="function_app_name=$FunctionAppName" `
               -out deviceapi.tfplan

if (!$?) {
    exit 1
}

terraform apply -var="location=$Location" `
               -var="inventory_api_url=$InventoryApiUrl" `
               -var="inventory_api_get_function_key=$GetFunctionKey" `
               -var="inventory_api_post_function_key=$PostFunctionKey" `   
               -var="function_app_name=$FunctionAppName"          

if (!$?) {
    exit 1
}

$ResourceGroup = & terraform output resource_group_name
$ResourceGroup = ResourceGroup.trim("""").trim("''")
Write-Host "##vso[task.setvariable variable=resource_group_name]$ResourceGroup"