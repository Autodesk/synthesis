#!/bin/bash
qemu-system-arm -M vexpress-a9 -dtb synthesis_image/vexpress-v2p-ca9.dtb -kernel synthesis_image/zImage -append "root=/dev/mmcblk0 rw roottype=ext4 console=ttyAMA0" --nographic -serial null -drive if=sd,driver=raw,cache=writeback,file=synthesis_image/rootfs.ext4 -redir tcp:10022::22 < /dev/null &> /dev/null &
sleep 20 && expect -c 'spawn ssh synthesis@localhost -p 10022; expect "assword:"; send "synthesis\r"; interact'
