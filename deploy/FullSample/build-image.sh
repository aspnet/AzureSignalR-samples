#!/bin/bash

set -x

DIR=$(cd `dirname $0`; pwd)
pushd ${DIR}

cd ../..

if [ -n "$1" ]; then
    docker build -t signalr-sdk-fullsample:$1 -f deploy/FullSample/Dockerfile --build-arg SDKVersion=$1 .
fi

popd