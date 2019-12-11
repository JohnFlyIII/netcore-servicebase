#!/usr/bin/env bash

scriptname=$(basename $0)
repo_root="${SYSTEM_DEFAULTWORKINGDIRECTORY:-"$(dirname $0)/.."}"
build_number=${BUILD_BUILDNUMBER:-"0"}
today=$(date +%F)
odata_log_filename="service-base-odata_${build_number}_${today}.trx"
function print_and_log {
  message="$@"
  logger "[$scriptname] $message"
  printf "[$scriptname] $message\n"
}

cd $repo_root

dotnet test tests/OData.Test/OData.Test.csproj --logger "trx;logfilename=$odata_log_filename"