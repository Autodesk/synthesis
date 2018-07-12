#!/bin/bash
FILE_URL="https://dl.dropboxusercontent.com/s/339v2vep4yf99x0/synthesis_image.tar.gz?dl=0"

if [ ! -f synthesis_image.tar.gz ] ; then
	printf "Begun downloading VM image\n"
	wget -O synthesis_image.tar.gz $FILE_URL
	printf "Image successfully download.\nPlease wait while the image is extracted.\n"
fi
mkdir synthesis_image -p;

if [ ! -d ./synthesis_image ] || [ ! -f synthesis_image/zImage ] || [ ! -f synthesis_image/rootfs.cpio.gz ] || [ ! -f synthesis_image/rootfs.ext4 ] || [ ! -f synthesis_image/vexpress-v2p-ca9.dtb ]; then

	tar -xvzf synthesis_image.tar.gz --directory synthesis_image;
	cd synthesis_image;

	mv new_root_files/* . && rm -rf new_root_files;
	cd ../;

	printf "Successfully downloaded and extract image.\n"
fi

