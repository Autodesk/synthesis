# Building the VM Image

This is the process for building the emulator virtual machine in which user code will run as a part of Synthesis's emulation.

WARNING: This is not a simple process. It requires a Linux environment and knowledge of Linux command line utilities.

## Setup

This process assumes you are working off of a linux machine of some sort. A virtual machine will suffice for this example. For 
those who wish to build the image from source but do not have access to a linux environment, VirtualBox is a good free virtual 
machine software, and Ubuntu Linux is a user-friendly option for the Linux distribution.

The amount of space required to make the image is rather large. Due to the number of tools and software needed to actually 
build the image, it could take upwards of 5 GB of disk space. However, the end result should be no more than 530 MB, and most 
most of the folders can be deleted once building is complete.

There are several tools required to build the image. Most of the utilities involved are core Linux/Unix utilities (i.e. tar, 
zcat, mkfs, dd). The tools involved that are not core are GCC, G++, GNU Make, the official FRC GCC Compiler for Linux, QEMU 
(with ARM extention) and (...). Once you have all of those installed, you can proceed to phase 1.

Please note that this is not a fast process, as during this you are going to download roughly 4 GB of git repositories and 
build a large amount of code. The process can take anywhere between 30 minutes and 2 hours depending on internet and computer
speed. With all this in mind, please move on to phase 1.

## Phase 1: Set-Up

Enter your linux environment of choice and open up a terminal. Create a temporary folder for all of the git repositories, 
typically something like `git`. Enter that directory and run the following commands. Note these will both take quite a bit of 
time.

```shell
$ mkdir git
$ mkdir -p vm-package/wpilib vm-package/java vm-package/root
$ cd git
$ git clone https://www.github.com/autodesk/synthesis
$ git clone https://www.github.com/Xilinx/linux-xlnx
$ git clone https://git.buildroot.net/buildroot
$ git clone https://www.github.com/wpilibsuite/allwpilib
```
## Phase 2: Building Linux

After the downloads complete, copy the configuration files from the ~/git/synthesis/emulation/hel/external-configs folder into
their respective folders

```shell
$ cp ~/git/synthesis/emulation/hel/external-configs/.linux-config ~/git/linux-xlnx/.config
$ cp ~/git/synthesis/emulation/hel/external-configs/.buildroot-config ~/git/buildroot/.config
```

After you have copied the files, move into the linux directory. After you are in, run the following make commands (replace 8 in -j8
with 2 times the number of CPU cores you have). The build process can take several minutes.

```shell
$ make -j8 ARCH=arm zImage CROSS_COMPILE=arm-frc-linux-gnueabi- 
$ make -j8 ARCH=arm UIMAGE_LOADADDR=0x8000 dtbs CROSS_COMPILE=arm-frc-linux-gnueabi-
```

After building, copy those files to the location you wish your VM to be built in (~/vm-package in this example). 

```shell
$ mkdir ~/vm-package
$ cp arch/arm/boot/zImage ~/vm-package
$ cp arch/arm/boot/dts/zynq-zed.dtb ~/vm-package
```

After this is complete, move on to building buildroot.

## PHase 3: Building the Initial RAM Disk

Move into the buildroot directory. This process is much simpler as it is one make command, however it is much longer time-wise.

```shell
$ cd ~/git/build && make
$ cp ~/git/buildroot/output/images/rootfs.cpio.gz ~/vm-package
```

This process can take upwards of 1 hours, so its recommened you do something else to pass time. After this is complete, move on to phase 3.

## Phase 4: Building WPILib
TODO: ADD DESCRIPTION
```shell
$ cd ~/allwpilib
$ git checkout v2018.4.1 # Substitute your WPILib release version
$ ./gradlew wpilibc:build
$ ./gradlew wpilibj:build
$ cp wpilibc/build/libs/wpilibc/shared/athena/libwpilibc.so ~/vm-package/wpilib
$ cp hal/build/libs/halAthena/libhal.so ~/vm-package/wpilib
$ cp build/dependencies/wpiutil-cpp/linuxathena/linux/athena/shared/libwpiutil.so ~/vm-package/wpilib
$ cp build/dependencies/ntcore-cpp/linuxathena/linux/athena/shared/libntcore.so ~/vm-package/wpilib
$ cp build/dependencies/cscore-cpp/linuxathena/linux/athena/shared/libcscore.so ~/vm-package/wpilib
$ cp build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxathena.zip_882ce6d6786024fd1378ddee15c75ec3/linux/athena/shared/*.so* ~/vm-package/wpilib
$ find -name wpilibJNI.jar -exec cp {} ~/vm-packages/wpilib \;
$ find -name ntcore.jar -exec cp {} ~/vm-packages/wpilib \;
$ find -name cscore.jar -exec cp {} ~/vm-packages/wpilib \;
$ find -name hal.jar -exec cp {} ~/vm-packages/wpilib \;
$ find -name wpilibj.jar -exec cp {} ~/vm-packages/wpilib \;
```

## Phase 5: Building HEL
TODO: ADD DESCRIPTION
```shell
$ cd ~/synthesis/emulation/hel
$ cmake . -DCMAKE_BUILD_TYPE=RELEASE -DARCH=ARM
$ make -j16 hel
$ cp libs/libhel.so ~/vm-package
```

## Phase 6: Making the Core Image
This is the most terminal intensive part.

```shell
$ wget -O - http://fl.us.mirror.archlinuxarm.org/arm/extra/jdk8-openjdk-8.u172-2-arm.pkg.tar.xz
$ wget -O - http://ca.us.mirror.archlinuxarm.org/arm/extra/jre8-openjdk-headless-8.u172-2-arm.pkg.tar.xz
$ wget -O - http://il.us.mirror.archlinuxarm.org/aarch64/extra/java-runtime-common-3-1-any.pkg.tar.xz
$ wget -O - http://fl.us.mirror.archlinuxarm.org/aarch64/core/ca-certificates-utils-20170307-1-any.pkg.tar.xz
$ find . -name \*.xz -exec tar -xvf {} -C ./java \; 
$ dd if=/dev/zero of=./rootfs.ext4
$ mkfs.ext4 ./rootfs.ext4
$ sudo losetup /dev/loop0 ./rootfs.ext4
$ sudo mount -t ext4 /dev/loop ./root
$ cd root
$ sudo mkdir -p ./home/lvuser
$ sudo zcat ../rootfs.cpio.gz | sudo cpio -imdv
$ sudo cp ../java/* -r .
$ sudo cp ../wpilib/* -r ./home/lvuser
$ sudo cp ../libhel.so ./home/lvuser
$ sudo cp /usr/arm-frc-linux-gnueabi/lib/libstdc++.so.6 ./usr/lib
$ cd ..
$ sudo umount ./root && sudo losetup -d
```

## Phase 7: VM Set-Up
TODO: ADD DESCRIPTION

```shell
qemu-system-arm -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel ../zImage -dtb ../zynq-zed.dtb -display none -serial null -serial mon:stdio -localtime -append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0" -redir tcp:10022::22  -redir tcp:11000::11000 -redir tcp:11001::11001 -sd ../rootfs.ext4 # You will be inside of the VM now
# When it asks for the username, enter the word root
# When it prompts for the password, enter the word synthesis
# You are now **inside** the VM

$ date %Y%m%d -s "20180815" # Todays date in a Year/Month/Day format
$ useradd lvuser # Press enter 3 times to ensure the command executes and has no password
$ sed -i 's/PermitRootLogin\ no/PermitRootLogin\ yes/' /etc/ssh/sshd_config
$ sed -i 's/PasswordAuthentication\ no/PasswordAuthentication\ yes/' /etc/ssh/sshd_config
$ sed -i 's/PermitEmptyPasswords\ no/PermitEmptyPasswords\ yes/' /etc/ssh/sshd_config
$ /etc/init.d/S50sshd restart
$ sed -i 's/root\ ALL=(ALL)\ ALL/&'$'\nlvuser\ ALL=(ALL)\ NOPASSWD:ALL/' /etc/sudoers # NEVER DO THIS UNLESS YOU KNOW EXACTLY WHAT YOU ARE DOING
$ cd ./home/lvuser
$ ln -s libhel.so libNiFpgaLv.so.13
$ ln -s libhel.so libvisa.so
$ ln -s libhel.so libNiRioSrv.so.13
$ ln -s libhel.so libniriodevunum.so.1
$ ln -s libhel.so libniriosession.so.1
$ ln -s libhel.so libRoboRIO_FRC_ChipObject.so.18
$ ln -s libhel.so libNiFpga.so.13
$ ln -s libFRC_NetworCommunication.so.18
$ cat > .vminfo <<EOF
$ 1.0
$ v2018.4.1
$ EOF
$ cat > .profile <<EOF
$ LD_LIBRARY_PATH=~:$LD_LIBRARY_PATH
$ export LD_LIBRARY_PATH
$ chmod +x /usr/lib/jvm/bin/*
$ mv /usr/bin/java /usr/bin/java.bak
$ ln -s /usr/lib/jvm/bin/java /usr/bin/java
# Press control-x then a to close VM
```

VM image creation is now complete. It can now be used as a drop in replacement for any current synthesis installations, or used 
again via the qemu command listed above. 
