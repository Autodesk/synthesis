@echo off

echo Downloading Xilinx Zynq A9 Linux Kernel...
cscript //nologo download.vbs http://www.wiki.xilinx.com/file/view/2017.1-zed-release.tar.xz/611988021/2017.1-zed-release.tar.xz
echo Done.
echo Extracting Kernel...
"C:\Program Files\7-Zip\7z.exe" x 2017.1-zed-release.tar.xz
"C:\Program Files\7-Zip\7z.exe" x -o2017.1-zed-release 2017.1-zed-release.tar
echo Done.
echo Getting kernel files...
mkdir linux
move 2017.1-zed-release\2017.1-zed-release\system.dtb linux/devicetree.dtb
move 2017.1-zed-release\2017.1-zed-release\image.ub linux/uImage
rmdir /q /s 2017.1-zed-release
del 2017.1-zed-release.tar
del 2017.1-zed-release.tar.xz
echo Done.