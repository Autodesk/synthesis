@echo off
dotnet publish SynthesisServer.csproj --runtime win-x64 --framework netcoreapp3.1 --self-contained true --output bin/win
dotnet publish SynthesisServer.csproj --runtime osx-x64 --framework netcoreapp3.1 --self-contained true --output bin/osx
dotnet publish SynthesisServer.csproj --runtime linux-x64 --framework netcoreapp3.1 --self-contained true --output bin/linux/glibc
dotnet publish SynthesisServer.csproj --runtime linux-musl-x64 --framework netcoreapp3.1 --self-contained true --output bin/linux/musl
dotnet publish SynthesisServer.csproj --runtime linux-arm --framework netcoreapp3.1 --self-contained true --output bin/linux/arm

copy appsettings.json bin\win\
copy appsettings.json bin\osx\
copy appsettings.json bin\linux\glibc
copy appsettings.json bin\linux\musl
copy appsettings.json bin\linux\arm