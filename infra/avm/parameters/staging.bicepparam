using '../main.bicep'

param environment = 'staging'
param location = 'uksouth'
param tags = {
  environment: 'staging'
  owner: 'portfolio'
  workload: 'pensions360'
}
