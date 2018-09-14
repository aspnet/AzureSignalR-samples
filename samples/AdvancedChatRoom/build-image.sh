#!/bin/bash

set -x

DIR=$(cd `dirname $0`; pwd)
pushd ${DIR}

declare -a SDKVersion=("1.0.0-preview1-10009" "1.0.0-preview1-10011" "1.0.0-preview1-10015" "1.0.0-preview1-10200")

for version in "${SDKVersion[@]}"; do
    docker build -t signalr-advancedchatroom:$version --build-arg SDKVersion=$version .
done

popd
