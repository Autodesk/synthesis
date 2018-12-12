#!/bin/sh

# This script is used to run the FRCUserProgram within the emulator VM

PREFIX="frc_program_chooser.sh:"

function run_user_program(){ #Run user program infinitely until interrupted by user
	until LD_LIBRARY_PATH=/home/lvuser $1; do
		if [ $? -eq 134 ]; then #Check exit code for user interrupt to allow users to stop it manually (required to stop java user program)
			break;
		fi
		(>&2 printf "\n$PREFIX FRCUserProgram stopped with exit code $?. Respawning.\n\n")
		sleep 1
	done
}
while true; do
if [ -f /home/lvuser/FRCUserProgram.jar ]; then #Run java user program
	echo "$PREFIX Executing Java FRCUserProgram"
	LD_LIBRARY_PATH=/home/lvuser /usr/lib/jvm/java-8-openjdk/bin/java -jar /home/lvuser/FRCUserProgram.jar
	exit 0;
fi
if [ -f /home/lvuser/FRCUserProgram ]; then #Run cpp user program
	echo "$PREFIX Executing C++ FRCUserProgram"
	LD_LIBRARY_PATH=/home/lvuser /home/lvuser/FRCUserProgram
	exit 0;
fi

echo "No FRCUserProgram found. Sleeping for 5 seconds and then retrying."
sleep 5;
done
echo "No FRCUserProgram found. Exiting."
exit 1;
