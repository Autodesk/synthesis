@echo off

call properties.bat

echo Downloading QEMU binaries...
cscript //nologo download.vbs http://www.omledom.com/pub/qemu/qemu-%QEMU_VERSION%-win64.tar.lzma
"C:\Program Files\7-Zip\7z.exe" x qemu-%QEMU_VERSION%-win64.tar.lzma
"C:\Program Files\7-Zip\7z.exe" x qemu-%QEMU_VERSION%-win64.tar
echo Done.
echo Cleaning up...
del qemu-%QEMU_VERSION%-win64.tar.lzma
del qemu-%QEMU_VERSION%-win64.tar
echo Done.