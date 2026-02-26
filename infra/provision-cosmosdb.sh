#!/bin/bash
set -euo pipefail

# --- Step 1: Variabler ---
RESOURCE_GROUP="CloudDatabasesRG"
LOCATION="northeurope"
ACCOUNT_NAME="cosmosdb-hanita-bcd"
DATABASE_NAME="bookmarks_db"
COLLECTION_NAME="bookmarks"

echo "=== Steg 1: Skapar Resursgrupp '$RESOURCE_GROUP' ==="
az group create --name $RESOURCE_GROUP --location $LOCATION --output none

# --- Step 2: Skapa Cosmos DB Account ---
echo "=== Steg 2: Skapar CosmosDB-konto '$ACCOUNT_NAME' (Detta tar 5-8 min) ==="
az cosmosdb create \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind MongoDB \
  --server-version "4.2" \
  --capabilities EnableServerless \
  --locations regionName=$LOCATION failoverPriority=0 isZoneRedundant=false \
  --output none

# --- Step 3: Skapa Databas och Collection ---
echo "=== Steg 3: Skapar Databas och Collection ==="
az cosmosdb mongodb database create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --name $DATABASE_NAME --output none

az cosmosdb mongodb collection create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE_NAME \
  --name $COLLECTION_NAME \
  --shard "category" --output none

# --- Step 4: Retrieve Connection String ---
echo "=== Steg 4: Hämtar Connection String ==="
CONNECTION_STRING=$(az cosmosdb keys list \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" \
  --output tsv)

echo ""
echo "=== PROVISIONERING KLAR! ==="
echo "Account:    $ACCOUNT_NAME"
echo "Connection String:"
echo "$CONNECTION_STRING"