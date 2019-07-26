#!/bin/sh

# This script is used to run the FRCUserProgram within the emulator VM

PREFIX=$0 # File name of script running

mkdir -p logs

while true; do
	if [ -f /home/lvuser/FRCUserProgram.jar ]; then # Run java user program
		printf "$PREFIX Executing Java FRCUserProgram.\n"
		LD_LIBRARY_PATH=/home/lvuser LD_PRELOAD=/lib/libhel.so java -jar /home/lvuser/FRCUserProgram.jar &> logs/log.log
	elif [ -f /home/lvuser/FRCUserProgram ]; then # Run cpp user program
		printf "$PREFIX Executing C++ FRCUserProgram.\n"
		sudo chmod +x /home/lvuser/FRCUserProgram
		LD_LIBRARY_PATH=/home/lvuser /home/lvuser/FRCUserProgram &> logs/log.log
	else
		printf "$PREFIX: No FRCUserProgram found. Sleeping for 5 seconds and then retrying.\n"
		sleep 5;
	fi
done
