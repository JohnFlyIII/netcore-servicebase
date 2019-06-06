#!/usr/bin/env bash

scriptname=$(basename $0)
repo_root="${SYSTEM_DEFAULTWORKINGDIRECTORY:-"$(dirname $0)/.."}"

function print_and_log {
  message="$@"
  logger "[$scriptname] $message"
  printf "[$scriptname] $message\n"
}

cd $repo_root
print_and_log "dotnet build $repo_root"
dotnet build -c Release
