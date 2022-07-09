# Protobuf Transfer Protocols

The **mirabuf** folder is a pointer to a submodule which needs to be pulled down seperately

## Fetching

` git submodule update --init --recursive `

to sync with new changes

` git submodule sync --recursive `

## Building

To run the following files or commands make sure that you are in the `synthesis/protocols` directory and not a child directory.

### Windows

- Run `proto_compile.bat` while in the protocols directory

or

` protoc -I=./mirabuf --python_out=../exporter/SynthesisFusionAddin/proto/proto_out ./mirabuf/*.proto `

### Linux

(To be filled in by someone with linux to verify)
