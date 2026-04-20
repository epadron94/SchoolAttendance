accountName="schoolattendanceaccount"
resourceGroup="SchoolAttendanceResourceGrp"
vaultName="SchoolAttendanceVault"
set -e

rollback()
{
    echo "An error occurred, rolling back.."
    for key in "${secrets_created[@]}";do
        echo "Deleting secret $key"
        az keyvault secret delete --name "$key" --vault-name $vaultName
    done
}
trap rollback ERR

json=$(az cosmosdb keys list --name $accountName --resource-group $resourceGroup --output json)
secrets_created=();
for key in $(echo "$json" |jq -r 'keys[]'); do
    value=$(echo "$json" | jq -r --arg k "$key" '.[$k]')
    echo "$key: $value"
    az keyvault secret set --value "$value" --name "$key" --vault-name $vaultName
    secrets_created+=("$key")
done
echo "done"