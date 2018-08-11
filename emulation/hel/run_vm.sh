#!/bin/bash
SSH_COMMAND="ssh -p 10022 -t lvuser@localhost rm -rf /home/lvuser/FRCUserProgram"
SCP_COMMAND="scp -P 10022 ./user-code/FRCUserProgram lvuser@localhost:/home/lvuser"

./scripts/download_vm.sh
printf "Starting VM\n"
if ! mkdir ./vm_lock &> /dev/null ; then
	printf "VM already running; vm_lock folder was found.\nChecking VM status:"
	expect -c "spawn $SSH_COMMAND;"
	printf "VM Initialized\nCopying files.\n"
	expect -c "spawn $SCP_COMMAND;"
else
	#(qemu-system-arm -M vexpress-a9 -dtb vm-package/zynq-zed.dtb -kernel vm-package/zImage -append "root=/dev/mmcblk0 rw roottype=ext4 console=ttyAMA0" --nographic -serial null -drive if=sd,driver=raw,cache=writeback,file=vm-package/rootfs.ext4 -redir tcp:10022::22 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	(qemu-system-arm -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel vm-package/zImage -dtb vm-package/zynq-zed.dtb -display none -serial null -serial mon:stdio -localtime -append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0" -redir tcp:10022::22  -redir tcp:11000::11000 -redir tcp:11001::11001 -redir tcp:2354::2354 -sd vm-package/rootfs.ext4 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	printf "VM Successfully started. Please wait while it initializes\n"
	sleep 10 && expect -c "spawn $SSH_COMMAND;"
	expect -c "spawn $SSH_COMMAND;"
	expect -c "spawn $SCP_COMMAND;"
fi
printf "File copied to VM. To execute, type the following command:\n\nssh -t lvuser@localhost \"./FRCUserProgram\" -p 10022\n"
