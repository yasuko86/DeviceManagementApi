param (
    [Parameter(Mandatory=$true)][string]$Location
)

$ResourceGroupName = "terraform-state-deviceapi"
$StorageAccountName = "terraformdeviceapi"
$ContainerName = "tfstate"

if ($null -eq $(Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue)){
    Write-Host $ResourceGroupName 'Create a new resource group...'
    New-AzResourceGroup -Name $ResourceGroupName -Location $Location
}

if ($null -eq $(Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageAccountName -ErrorAction SilentlyContinue)){
    Write-Host $StorageAccountName 'Create a new storage account...'
    New-AzStorageAccount -ResourceGroupName $ResourceGroupName `
        -Name $StorageAccountName `
        -SkuName Standard_LRS `
        -Location $Location
}

$storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageAccountName -ErrorAction SilentlyContinue
if ($null -ne $storageAccount) {
    $context = $storageAccount.Context
    if ($null -eq $(Get-AzStorageContainer -Name $ContainerName -Context $context -ErrorAction SilentlyContinue)){
        Write-Host $ContainerName 'Create a new container...'
        New-AzStorageContaier -Name $ContainerName -Context $context -Permission Off
    }
}