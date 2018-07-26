#!/bin/bash

if [ ! -d "$1" ] || [ ! -f "$2" ]; then
    echo "Build files not found"
    cd $1;
    $3 $4
fi
