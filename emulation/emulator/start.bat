@echo off

call properties.bat

qemu-%QEMU_VERSION%-win64\qemu-system-arm.exe^
 -machine xilinx-zynq-a9^
 -cpu cortex-a9^
 -m %RAM_SIZE%^
 -kernel linux/uImage^
 -dtb linux/devicetree.dtb^
 -display none^
 -serial null^
 -serial mon:stdio^
 -localtime^
 -append "console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw"^
 -redir tcp:%LOCAL_SSH_PORT%::22^
 -sd %IMG_FILE%