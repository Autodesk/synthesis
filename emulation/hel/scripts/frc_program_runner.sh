#!/bin/sh

# This script is used to run the FRCUserProgram within the emulator VM

PREFIX=$0 # File name of script running

printf "$PREFIX Killing previously running ${0##*/}.\n"
for pid in $(pidof ${0##*/}); do
    if [ $pid != $$ ]; then
	kill -9 $pid &>/dev/null
    fi
done
killall FRCUserProgram FRCUserProgram.jar &>/dev/null

LOG_DIR=logs
LOG_FILE=$LOG_DIR/log.log

mkdir -p $LOG_DIR
printf "$PREFIX Clearing last log.\n"
echo -n "" > $LOG_FILE

while true; do
	if [ -f /home/lvuser/FRCUserProgram.jar ]; then # Run java user program
		printf "$PREFIX Executing Java FRCUserProgram - output redirected to log.\n"
		LD_LIBRARY_PATH=/home/lvuser LD_PRELOAD=/lib/libhel.so java -jar /home/lvuser/FRCUserProgram.jar &> $LOG_FILE
	elif [ -f /home/lvuser/FRCUserProgram ]; then # Run cpp user program
		printf "$PREFIX Executing C++ FRCUserProgram - output redirected to log.\n"
		sudo chmod +x /home/lvuser/FRCUserProgram
		LD_LIBRARY_PATH=/home/lvuser /home/lvuser/FRCUserProgram &> $LOG_FILE
	else
		printf "$PREFIX No FRCUserProgram found. Sleeping for 5 seconds and then retrying.\n"
		sleep 5;
	fi
done
