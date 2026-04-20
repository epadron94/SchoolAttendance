resourceGroup="SchoolAttendanceResourceGrp"
az group create  --location southcentralus --name SchoolAttendanceResourceGrp
az keyvault create --resource-group $resourceGroup --enable-rbac-authorization true --location southcentralus --name SchoolAttendanceVault
keyvault="SchoolAttendanceVault"
cosmosdbaccount="schoolattendanceaccount"
az cosmosdb sql database create --resource-group $resourceGroup --name SchoolAttendance --account-name $cosmosdbaccount
az cosmosdb sql container create --account-name $cosmosdbaccount --database-name SchoolAtte ktndance --name Students  --partition-key-path "/grade" --resource-group $resourceGroup
az cosmosdb sql container create --account-name $cosmosdbaccount --database-name SchoolAttendance --name Attendance --partition-key-path "/grade" --resource-group $resourceGroup

userId=$(az ad signed-in-user show --query id -o tsv)
keyvaultscope=$(az keyvault show --name $keyvault --query id -o tsv)
az role assignment create --assignee $userId --role "Key Vault Secrets User" --scope $keyvaultscope