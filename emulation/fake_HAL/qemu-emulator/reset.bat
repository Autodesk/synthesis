@echo off

echo Resetting image...
qemu-2.6.0-win64\qemu-img.exe snapshot -a initial roborio.img
echo Done.