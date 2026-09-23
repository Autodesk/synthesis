#!/bin/bash

cd "$(dirname "$0")" || exit 1
mkdir -p ./certs
./bin/glueball --config-file ./config.toml --cert-dir ./certs

