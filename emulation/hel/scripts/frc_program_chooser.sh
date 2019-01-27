#!/bin/sh

# This script is used to run the FRCUserProgram within the emulator VM

PREFIX=$0 # File name of script running

while true; do
	if [ -f /home/lvuser/FRCUserProgram.jar ]; then #Run java user program
		echo "$PREFIX Executing Java FRCUserProgram"
		LD_LIBRARY_PATH=/home/lvuser /usr/lib/jvm/java-8-openjdk/bin/java -jar /home/lvuser/FRCUserProgram.jar
	elif [ -f /home/lvuser/FRCUserProgram ]; then #Run cpp user program
		echo "$PREFIX Executing C++ FRCUserProgram"
		chmod +x /home/lvuser/FRCUserProgram
		LD_LIBRARY_PATH=/home/lvuser /home/lvuser/FRCUserProgram
	else
		echo "No FRCUserProgram found. Sleeping for 5 seconds and then retrying."
		sleep 5;
	fi
done
