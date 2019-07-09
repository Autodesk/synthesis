#!/bin/bash

# Checks if a project has already been built and builds it if necessary

# First argument is name of project
# Second argument is the directory to build from
# Third argument is a string containing the space-separated files to search for
# Fourth argument is a string containing the build command to run from the build directory

PROJECT_NAME=$1
SOURCE_DIR=$2

_SPACE_SEPARATED=$3
_SPACE_SEPARATED=${_SPACE_SEPARATED//;/\ }
REQUIRED_FILES=($_SPACE_SEPARATED)

BUILD_COMMAND=$4

# Exit script if any command fails
set -e
set -o pipefail

if [ ! -d "$SOURCE_DIR" ]; then
    echo -e "\e[31m$PROJECT_NAME build directory not found ($SOURCE_DIR)\e[0m"
    exit 1
fi

for FILE in ${REQUIRED_FILES[@]}; do
    if [ ! -f "$FILE" ]; then
        echo -e "\e[31m$PROJECT_NAME build file not found ($FILE)\e[0m"
        cd $SOURCE_DIR
        $BUILD_COMMAND
        exit 0
    fi
done

echo -e "\e[32m$PROJECT_NAME build files found. Skipping rebuild.\e[0m"
