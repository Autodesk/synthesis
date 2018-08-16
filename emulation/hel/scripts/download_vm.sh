#!/bin/bash

# Download the emulator VM

FILE_URL="https://dl.dropboxusercontent.com/s/svvxunbh7akmw3y/vm-package.tar.gz?dl=0"

if [ ! -f vm-package.tar.gz ] ; then
	printf "Begun downloading VM image\n"
	wget -O vm-package.tar.gz $FILE_URL
	printf "Image successfully downloaded.\nPlease wait while the image is extracted.\n"
fi
mkdir vm-package -p;

if [ ! -d ./vm-package ] || [ ! -f vm-package/zImage ] || [ ! -f vm-package/rootfs.ext4 ] || [ ! -f vm-package/zynq-zed.dtb ]; then

	tar -xvzf vm-package.tar.gz --directory vm-package;
	cd vm-package;

	mv vm-package/* . && rm -rf vm-package;
	cd ../;

	printf "Successfully downloaded and extracted the image.\n"
fi

