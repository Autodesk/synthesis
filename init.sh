#!/usr/bash
git submodule update --init --recursive
cd protocols
bash proto_compile.sh

