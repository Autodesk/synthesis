#!/bin/bash

PREFIX=$0 # File name of script running

TEST_DIR=./tests

if [ -z "$(ls -A $TEST_DIR)" ]; then
    printf "$PREFIX: Test binary folder is empty.\n"
else
    for test in $TEST_DIR/*; do
        if ! $test --gtest_color=yes --gtest_catch_exceptions=0; then
            printf "\n\n$PREFIX: A test failed - exiting\n"
            exit 1
        fi
    done
fi
