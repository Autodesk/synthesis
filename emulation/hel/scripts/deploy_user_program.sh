#!/bin/bash

# Deploys FRC user program with proper name

jar_target=FRCUserProgram.jar
cpp_target=FRCUserProgram
user_program_target=$cpp_target

if [ $# -eq 0 ]; then
    echo "Please specify source file"
    exit 1
else
    if [ -f $1 ] || [ -d $1 ]; then
        if [[ $1 == *.jar ]]; then # Pick target name regardless of source name
            user_program_target=$jar_target
        else
            user_program_target=$cpp_target
        fi
        printf "Chosen target $user_program_target\n"
    
        if ssh -p 10022 lvuser@localhost test -e /home/lvuser/$jar_target -o -e /home/lvuser/$cpp_target \> /dev/null 2\>\&1; then # Check for existing user programs
            read -p "There are existing files. Delete? [y/n] " -r # Prompt user to delete them
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                printf "Aborting\n"
                exit 1
            fi
            
            printf "Deleting existing programs\n"
            
            ssh -p 10022 lvuser@localhost rm -f $jar_target $cpp_target
        fi
        
        printf "Copying $1\n"
        scp -r -P 10022 $1 lvuser@localhost:/home/lvuser/$user_program_target
    
        printf "Files successfully copied\n"
        exit 0
    else
        printf "Error: $1 not found\n"
        exit 1
    fi
fi
