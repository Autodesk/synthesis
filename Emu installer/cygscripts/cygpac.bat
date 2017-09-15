@echo off

call cygproperties.bat

echo Downloading Cygwin Installer (%ARCH%)...
cscript //nologo download.vbs https://cygwin.com/setup-%ARCH%.exe
echo Done.

REM Having a -P with no packages afterwards throws an error
REM These two lines get around that
set INSTALL_PACKAGES=-P "mingw64-x86_64-gcc-g++,make"

echo Installing Cygwin...
setup-%ARCH%.exe -B -q -D -L -d -g -o -s %SITE% -R %ROOT% -C Base %INSTALL_PACKAGES%
echo Done.