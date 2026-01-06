#!/usr/bin/env bash
set -euo pipefail

ENVIRONMENT=${1:-staging}
LOCATION=${2:-uksouth}
PARAM_FILE="infra/avm/parameters/${ENVIRONMENT}.bicepparam"

if [[ ! -f "${PARAM_FILE}" ]]; then
  echo "Parameter file not found: ${PARAM_FILE}" >&2
  echo "Valid environments: staging, prod" >&2
  exit 1
fi

az deployment sub create \
  --location "${LOCATION}" \
  --template-file infra/avm/main.bicep \
  --parameters "${PARAM_FILE}"
