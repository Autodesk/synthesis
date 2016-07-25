@echo off

echo Downloading Xilinx Zynq A9 Linux Kernel...
cscript //nologo download.vbs http://www.wiki.xilinx.com/file/view/2014.3-release.tar.xz/526959742/2014.3-release.tar.xz
echo Done.
echo Extracting Kernel...
"C:\Program Files\7-Zip\7z.exe" x 2014.3-release.tar.xz
"C:\Program Files\7-Zip\7z.exe" x -o2014.3-release 2014.3-release.tar
echo Done.
echo Getting kernel files...
mkdir linux
move 2014.3-release\zed\devicetree.dtb linux/devicetree.dtb
move 2014.3-release\uImage linux/uImage
rmdir /q /s 2014.3-release
del 2014.3-release.tar
del 2014.3-release.tar.xz
echo Done.