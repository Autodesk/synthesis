#!/bin/bash

# Wait until connected to VM, then start FRC program runner

until ssh -q -p 10022 lvuser@localhost exit; do
	echo "Waiting for connection"
	sleep 5
done

echo "Connected"

ssh -q -p 10022 lvuser@localhost << EOF
	sudo killall frc_program_runner.sh >/dev/null 2>&1
	sudo killall java >/dev/null 2>&1
	sudo killall FRCUserProgram >/dev/null 2>&1
	nohup /home/lvuser/frc_program_runner.sh </dev/null >/dev/null 2>&1 &
	exit
EOF

echo "Started FRC program runner"
