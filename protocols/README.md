# Protobuf Transfer Protocols

The **mirabuf** folder is a pointer to a submodule which needs to be pulled down seperately

## Fetching

To pull down the submodule:
```
$ git submodule update --init --recursive
```

to sync with new changes:
```
$ git submodule sync --recursive
```

## Generate Protobuf Files

To run the following files or commands make sure that you are in the `synthesis/protocols` directory and not a child directory.

### Windows

- Run `proto_compile.bat` while in the protocols directory
    ```
    $ proto_compile.bat
    ``` 

### Linux / MacOS

- Run `proto_compile.sh` while in the protocols directory
    ```
    $ ./proto_compile.sh
    ```
