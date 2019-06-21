#!/bin/bash

# Checks if a project has already been built
# $1 Project name
# $2 The project source - checks if it exists
# $3 The project output - checks if it exists
# Remaining arugments are the build command

if [ ! -d "$2" ] || [ ! -f "$3" ]; then
    echo -e "\e[31mBuild for $1 files not found\e[0m"
    cd $2;
    shift 3 # Ignore first three arguments
    $* # Run remaining arguments
    exit 0;
fi

echo -e "\e[32mBuild files for $1 found. Skipping rebuild\e[0m"
