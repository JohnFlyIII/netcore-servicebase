#!/usr/bin/env bash

scriptname=$(basename $0)
scriptpath=$(realpath $0)
repo_root="${SYSTEM_DEFAULTWORKINGDIRECTORY:-"$(dirname ${scriptpath%/*})"}"

function print_and_log {
  message="$@"
  logger "[$scriptname] $message"
  printf "[$scriptname] $message\n"
}

pushd $repo_root

print_and_log "dotnet build with Release config"

# builds
dotnet build ./src/ServiceBase.OData.Web/ServiceBase.OData.Web.csproj -c Release

