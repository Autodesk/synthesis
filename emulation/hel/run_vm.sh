#!/bin/bash

./scripts/download_vm.sh

printf "Starting VM.\n"

if ! mkdir ./vm_lock &> /dev/null ; then
	printf "VM already running; vm_lock folder was found.\n"
else
	#(qemu-system-arm -M vexpress-a9 -dtb vm-package/zynq-zed.dtb -kernel vm-package/zImage -append "root=/dev/mmcblk0 rw roottype=ext4 console=ttyAMA0" --nographic -serial null -drive if=sd,driver=raw,cache=writeback,file=vm-package/rootfs.ext4 -redir tcp:10022::22 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	(qemu-system-arm \
	-machine xilinx-zynq-a9 \
	-cpu cortex-a9 \
	-m 2048 \
	-kernel vm-package/zImage \
	-dtb vm-package/zynq-zed.dtb \
	-display none \
	-serial null \
	-serial mon:stdio \
	-append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0" \
	-gdb tcp::5789 \
    -net user,hostfwd=tcp::10022-:22,hostfwd=tcp::11000-:11000,hostfwd=tcp::11001-:11001,hostfwd=tcp::5789-:5789,hostfwd=tcp::50052-:50052 \
	-net nic \
	-sd vm-package/rootfs.ext4 < /dev/null &> /dev/null; rm -rf ./vm_lock )&
	printf "VM successfully started. Please wait while it initializes.\n"
fi

until ssh -q -p 10022 lvuser@localhost exit; do
	printf "Waiting for connection.\n"
	sleep 1
done

printf "VM connections available.\n"
