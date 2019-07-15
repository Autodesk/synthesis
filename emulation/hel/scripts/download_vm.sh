#!/bin/bash

# Download the emulator VM

FILE_URL="https://www.dropbox.com/s/knbc6r2wmh78vow/vm-package.zip?dl=0"

if [ ! -f vm-package.zip ] ; then
	printf "Begun downloading VM image\n"
	wget -O vm-package.zip $FILE_URL
	printf "Image successfully downloaded.\nPlease wait while the image is extracted.\n"
fi
mkdir vm-package -p;

if [ ! -d ./vm-package ] || [ ! -f vm-package/zImage ] || [ ! -f vm-package/rootfs.ext4 ] || [ ! -f vm-package/zynq-zed.dtb ]; then

	unzip vm-package.zip;

	printf "Successfully downloaded and extracted the image.\n"
fi

