#!/bin/bash

success=true
libhel_source=lib/libhel.so
user_program_source=user-code/FRCUserProgram

if [ $# -eq 0 ]; then
    if [ -f lib/libhel.so ]; then
        printf "Copying $libhel_source\n"
        sshpass -p "adskbxd" scp -r -P 10022 $libhel_source synthesis@localhost:/home/synthesis
    else
        printf "Error: $libhel_source not found\n"
        success=false
    fi

    if [ -f $user_program_source ]; then
        printf "Copying $user_program_source\n"
        sshpass -p "adskbxd" scp -r -P 10022 $user_program_source synthesis@localhost:/home/synthesis
    else
        printf "Error: $user_program_source not found\n"
        success=false
    fi
else
    if [ -f $1 ] || [ -d $1 ]; then
        printf "Copying $1\n"
        sshpass -p "adskbxd" scp -r -P 10022 $1 synthesis@localhost:/home/synthesis
    else
        printf "Error: $1 not found\n"
        success=false
    fi

fi

if [ "$success" = true ]; then
    printf "Files successfully copied\n"
fi
