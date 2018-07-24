#!/bin/bash
SSH_COMMAND="ssh -p 10022 -t synthesis@localhost rm -rf /home/synthesis/FRCUserProgram"
SCP_COMMAND="scp -P 10022 ./user_code/FRCUserProgram synthesis@localhost:/home/synthesis"
PASSWORD="adskbxd"

./scripts/download_vm.sh
printf "Starting VM\n"
if ! mkdir ./vm_lock > /dev/null ; then 
	printf "VM already running; vm_lock folder was found.\nChecking VM status:"
	expect -c "spawn $SSH_COMMAND; expect \"assword:\"; send \"$PASSWORD\r\"; interact" > /dev/null
	printf "VM Initialized\nCopying files.\n"
	expect -c "spawn $SCP_COMMAND; expect \"assword:\"; send \"$PASSWORD\r\"; interact"
else
	#(qemu-system-arm -M vexpress-a9 -dtb vm-package/zynq-zed.dtb -kernel vm-package/zImage -append "root=/dev/mmcblk0 rw roottype=ext4 console=ttyAMA0" --nographic -serial null -drive if=sd,driver=raw,cache=writeback,file=vm-package/rootfs.ext4 -redir tcp:10022::22 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	(qemu-system-arm -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel vm-package/zImage -dtb vm-package/zynq-zed.dtb -display none -serial null -serial mon:stdio -localtime -append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0" -redir tcp:10022::22  -sd vm-package/rootfs.ext4 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	printf "VM Successfully start. Please wait while it initializes\n"
	sleep 30 && expect -c "spawn $SSH_COMMAND; expect \"assword:\"; send \"$PASSWORD\r\"; interact"
	expect -c "spawn $SSH_COMMAND; expect \"assword:\"; send \"$PASSWORD\r\"; interact"
	expect -c "spawn $SCP_COMMAND; expect \"assword:\"; send \"$PASSWORD\r\"; interact"
fi
printf "File copied to VM. To execute, type the following command:\n\nssh -t synthesis@localhost \"./FRCUserProgram\" -p 10022\n"
