#!/bin/bash

# Checks if a project has already been built
# $1 The project source - checks if it exists
# $2 The project output - checks if it exists
# $3 Build command
# $4 Build argument
# $5 Project name

if [ ! -d "$1" ] || [ ! -f "$2" ]; then
    echo -e "\e[31mBuild for $5 files not found\e[0m"
    cd $1;
    $3 $4;
    exit 0;
fi

echo -e "\e[32mBuild files for $5 found. Skipping rebuild\e[0m"
