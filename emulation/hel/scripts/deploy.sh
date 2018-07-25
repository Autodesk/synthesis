#!/bin/bash

success=true
libhel_source=lib/libhel.so
user_program_source=user_code/FRCUserProgram

printf "Copying scripts/run_user_program.sh\n"
sshpass -p "adskbxd" scp -r -P 10022 scripts/run_user_program.sh synthesis@localhost:/home/synthesis

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

if [ "$success" = true ]; then
    printf "Files successfully copied\n"
fi
