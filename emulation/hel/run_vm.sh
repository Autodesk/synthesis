#!/bin/bash

./scripts/download_vm.sh

printf "Starting VM.\n"

if ! mkdir ./vm_lock_native &> /dev/null ; then
	printf "Native VM already running; vm_lock_native folder was found.\n"
else
	(qemu-system-arm \
	-machine xilinx-zynq-a9 \
	-cpu cortex-a9 \
	-m 2048 \
	-kernel vm-package/kernel-native \
	-dtb vm-package/zynq-zed.dtb \
	-display none \
	-serial null \
	-serial mon:stdio \
	-append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0" \
	-gdb tcp::5789 \
    -net user,hostfwd=tcp::10022-:22,hostfwd=tcp::11000-:11000,hostfwd=tcp::11001-:11001,hostfwd=tcp::5789-:5789,hostfwd=tcp::50052-:50052 \
	-net nic \
	-sd vm-package/rootfs-native.ext4 < /dev/null &> /dev/null; rm -rf ./vm_lock_native )&
	printf "Native VM successfully started. Please wait while it initializes.\n"
fi

if ! mkdir ./vm_lock_java &> /dev/null ; then
	printf "Java VM already running; vm_lock_java folder was found.\n"
else
	(qemu-system-x86_64 \
	-m 2048 \
	-kernel vm-package/kernel-java \
	-nographic \
	-append "console=ttyPS0 root=/dev/sda rw" \
	-net user,hostfwd=tcp::10023-:22,hostfwd=tcp::50053-:50051 \
	-net nic \
	-hda vm-package/rootfs-java.ext4 < /dev/null &> /dev/null; rm -rf ./vm_lock_java )&
	printf "Java VM successfully started. Please wait while it initializes.\n"
fi

until ssh -q -p 10023 lvuser@localhost exit; do
	printf "Waiting for java connection.\n"
	sleep 1
done

until ssh -q -p 10022 lvuser@localhost exit; do
	printf "Waiting for native connection.\n"
	sleep 1
done

printf "VM connections available.\n"
