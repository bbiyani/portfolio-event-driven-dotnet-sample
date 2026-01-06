targetScope = 'subscription'

@description('Deployment environment name.')
@allowed([
  'staging'
  'prod'
])
param environment string

@description('Azure region for resources.')
param location string = 'uksouth'

@description('Prefix used for resource names.')
param namePrefix string = 'pensions360'

@description('Resource group name.')
param resourceGroupName string = '${namePrefix}-${environment}-rg'

@description('Shared tags applied to all resources.')
param tags object = {}

var suffix = uniqueString(subscription().id, resourceGroupName)
var nameSeed = toLower('${namePrefix}${environment}${suffix}')
var storageAccountName = take(nameSeed, 24)
var keyVaultName = take(nameSeed, 24)
var cosmosAccountName = take(nameSeed, 44)
var serviceBusNamespaceName = take(toLower('${namePrefix}-${environment}-sb-${suffix}'), 50)
var appServicePlanName = '${namePrefix}-${environment}-plan'
var apiAppName = take(toLower('${namePrefix}-${environment}-api-${suffix}'), 60)

module resourceGroup 'br/public:avm/res/resources/resource-group:0.4.0' = {
  name: 'resource-group'
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

module keyVault 'br/public:avm/res/key-vault/vault:0.5.3' = {
  name: 'key-vault'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    resourceGroup
  ]
  params: {
    name: keyVaultName
    location: location
    tags: tags
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    skuName: 'standard'
  }
}

module storageAccount 'br/public:avm/res/storage/storage-account:0.12.0' = {
  name: 'storage-account'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    resourceGroup
  ]
  params: {
    name: storageAccountName
    location: location
    tags: tags
    skuName: 'Standard_LRS'
    kind: 'StorageV2'
    accessTier: 'Hot'
  }
}

module cosmosAccount 'br/public:avm/res/document-db/database-account:0.7.2' = {
  name: 'cosmos-account'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    resourceGroup
  ]
  params: {
    name: cosmosAccountName
    location: location
    tags: tags
    databaseAccountOfferType: 'Standard'
    kind: 'GlobalDocumentDB'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
  }
}

module serviceBus 'br/public:avm/res/service-bus/namespace:0.8.0' = {
  name: 'service-bus'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    resourceGroup
  ]
  params: {
    name: serviceBusNamespaceName
    location: location
    tags: tags
    skuName: 'Standard'
  }
}

module appServicePlan 'br/public:avm/res/web/serverfarm:0.6.0' = {
  name: 'app-service-plan'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    resourceGroup
  ]
  params: {
    name: appServicePlanName
    location: location
    tags: tags
    skuName: 'P1v3'
    capacity: 1
    kind: 'app'
  }
}

module apiAppService 'br/public:avm/res/web/site:0.8.0' = {
  name: 'api-app-service'
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    appServicePlan
  ]
  params: {
    name: apiAppName
    location: location
    tags: tags
    serverFarmResourceId: appServicePlan.outputs.resourceId
    httpsOnly: true
  }
}

output resourceGroupId string = resourceGroup.outputs.resourceId
output keyVaultName string = keyVaultName
output storageAccountName string = storageAccountName
output cosmosAccountName string = cosmosAccountName
output serviceBusNamespaceName string = serviceBusNamespaceName
output apiAppName string = apiAppName
