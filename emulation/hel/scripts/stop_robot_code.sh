#!/bin/bash

# If connected to the VM, kill all FRC programs and runners

if ! ssh -q -p 10022 lvuser@localhost exit; then
	echo "Not connected"
else 
	ssh -p 10022 lvuser@localhost << EOF
		sudo killall frc_program_runner.sh >/dev/null 2>&1
		sudo killall java >/dev/null 2>&1
		sudo killall FRCUserProgram >/dev/null 2>&1
EOF
	echo "FRC programs and runner stopped"
fi
