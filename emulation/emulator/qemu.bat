@echo off

echo Downloading QEMU binaries...
cscript //nologo download.vbs http://www.omledom.com/pub/qemu/qemu-2.6.0-win64.tar.lzma
"C:\Program Files\7-Zip\7z.exe" x qemu-2.6.0-win64.tar.lzma
"C:\Program Files\7-Zip\7z.exe" x qemu-2.6.0-win64.tar
echo Done.
echo Cleaning up...
del qemu-2.6.0-win64.tar.lzma
del qemu-2.6.0-win64.tar
echo Done.