@echo off

CALL :highlight "Removing Existing DLLs..."

DEL /S /Q /f "..\Assets\Packages\*.dll"

CALL :highlight "Building Solution..."

dotnet build

CALL :highlight "Installing Dependencies..."

COPY  "%HOMEPATH%\.nuget\packages\google.protobuf\3.23.3\lib\netstandard2.0\Google.Protobuf.dll" "..\Assets\Packages\Google.Protobuf.dll"
COPY  "%HOMEPATH%\.nuget\packages\mathnet.numerics\4.15.0\lib\netstandard2.0\MathNet.Numerics.dll" "..\Assets\Packages\MathNet.Numerics.dll"
COPY  "%HOMEPATH%\.nuget\packages\mathnet.spatial\0.6.0\lib\netstandard2.0\MathNet.Spatial.dll" "..\Assets\Packages\MathNet.Spatial.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0\System.Buffers.dll" "..\Assets\Packages\System.Buffers.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.memory\4.5.3\lib\netstandard2.0\System.Memory.dll" "..\Assets\Packages\System.Memory.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.runtime.compilerservices.unsafe\6.0.0\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll" "..\Assets\Packages\System.Runtime.CompilerServices.Unsafe.dll"
COPY  "%HOMEPATH%\.nuget\packages\portable.bouncycastle\1.9.0\lib\netstandard2.0\BouncyCastle.Crypto.dll" "..\Assets\Packages\BouncyCastle.Crypto.dll"
COPY  "%HOMEPATH%\.nuget\packages\ionic.zip\1.9.1.8\lib\Ionic.Zip.dll" "..\Assets\Packages\Ionic.Zip.dll"

CALL :highlight "!!! Finished Copying Dependencies !!!"

EXIT /B 0

:highlight
    echo 
    echo [32m%~1[0m
