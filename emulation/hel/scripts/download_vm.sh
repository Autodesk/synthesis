#!/bin/bash

# Download the emulator VM

FILE_URL="https://www.dropbox.com/s/knbc6r2wmh78vow/vm-package.zip"

if [ ! -f vm-package.zip ] ; then
	printf "Begun downloading VM images\n"
	wget -q --show-progress -O vm-package.zip $FILE_URL 
	printf "Images successfully downloaded.\n"
fi
mkdir vm-package -p;

if [ ! -d ./vm-package ] || [ ! -f vm-package/kernel-native ] || [ ! -f vm-package/rootfs-native.ext4 ] || [ ! -f vm-package/zynq-zed.dtb ] || [ ! -f vm-package/kernel-java ] || [ ! -f vm-package/rootfs-java.ext4 ] || [ ! -f vm-package/grpc-bridge ]; then
	printf "Please wait while the images are extracted.\n"

	unzip vm-package.zip;

	printf "Successfully extracted the images.\n"
fi

printf "VM images ready.\n"
