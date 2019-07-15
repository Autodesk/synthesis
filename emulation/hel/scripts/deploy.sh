#!/bin/bash

# Deploys files to the emulator
# If no argument is specified, then deploy libhel.so, FRCUserProgram, and the tests
# If a file is specified, deploy that

success=true
libhel_source=lib/libhel.so
user_program_source=user-code/FRCUserProgram
tests_source=bin/tests

if [ $# -eq 0 ]; then
    if [ -f lib/libhel.so ]; then
        printf "Copying $libhel_source\n"
        scp -r -P 10022 $libhel_source lvuser@localhost:/home/lvuser
    else
        printf "Error: $libhel_source not found\n"
        success=false
    fi

    if [ -f $user_program_source ]; then
        printf "Copying $user_program_source\n"
        scp -r -P 10022 $user_program_source lvuser@localhost:/home/lvuser
    else
        printf "Error: $user_program_source not found\n"
        success=false
    fi

    if [ -d $tests_source ]; then
        printf "Copying $tests_source\n"
        scp -r -P 10022 $tests_source lvuser@localhost:/home/lvuser
    else
        printf "Error: $tests_source not found\n"
        success=false
    fi
else
    if [ -f $1 ] || [ -d $1 ]; then
        printf "Copying $1\n"
        scp -r -P 10022 $1 lvuser@localhost:/home/lvuser
    else
        printf "Error: $1 not found\n"
        success=false
    fi

fi

if [ "$success" = true ]; then
    printf "Files successfully copied\n"
fi
