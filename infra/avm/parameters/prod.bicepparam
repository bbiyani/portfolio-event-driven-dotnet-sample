using '../main.bicep'

param environment = 'prod'
param location = 'uksouth'
param tags = {
  environment: 'prod'
  owner: 'portfolio'
  workload: 'pensions360'
}
