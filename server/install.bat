@echo off
dotnet publish Synthesis-Server.csproj --runtime win-x64 --framework netcoreapp3.1 --self-contained true --output bin/win
dotnet publish Synthesis-Server.csproj --runtime osx-x64 --framework netcoreapp3.1 --self-contained true --output bin/osx
dotnet publish Synthesis-Server.csproj --runtime linux-x64 --framework netcoreapp3.1 --self-contained true --output bin/linux/glibc
dotnet publish Synthesis-Server.csproj --runtime linux-musl-x64 --framework netcoreapp3.1 --self-contained true --output bin/linux/musl
dotnet publish Synthesis-Server.csproj --runtime linux-arm --framework netcoreapp3.1 --self-contained true --output bin/linux/arm

copy appsettings.json bin\win\
copy appsettings.json bin\osx\
copy appsettings.json bin\linux\glibc
copy appsettings.json bin\linux\musl
copy appsettings.json bin\linux\arm