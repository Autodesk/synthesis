set NETDIR="C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727"

%NETDIR%\regasm.exe /codebase ..\ClientGraphicsDemoAU\bin\Debug\ClientGraphicsDemoAU.dll
%NETDIR%\regasm.exe /codebase ..\GraphicManagerTest\bin\GraphicManagerTest.dll

set NETDIR=
pause