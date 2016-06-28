set NETDIR="C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727"

%NETDIR%\regasm.exe /u ..\ClientGraphicsDemoAU\bin\Debug\ClientGraphicsDemoAU.dll
%NETDIR%\regasm.exe /u ..\GraphicManagerTest\bin\GraphicManagerTest.dll

set NETDIR=
pause