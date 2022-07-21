#!/bin/sh
git submodule update --init --recursive
cd protocols
bash proto_compile.sh
cd ../exporter/SynthesisFusionAddin/proto
bash build.sh
cd ../../..
