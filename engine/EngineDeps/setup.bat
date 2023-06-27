@echo off

:highlight
    echo ^<ESC^>[32m [32m%~1[0m
EXIT /B 0

CALL :highlight "\nRemoving Existing DLLs...\n"

DEL /S /Q /f "..\Assets\Packages\*.dll"

CALL :highlight "\nBuilding Solution...\n"

dotnet build

CALL :highlight "\nInstalling Dependencies...\n"

COPY  "%HOMEPATH%\.nuget\packages\google.protobuf\3.23.3\lib\netstandard2.0\Google.Protobuf.dll" "..\Assets\Packages\Google.Protobuf.dll"
COPY  "%HOMEPATH%\.nuget\packages\mathnet.numerics\4.15.0\lib\netstandard2.0\MathNet.Numerics.dll" "..\Assets\Packages\MathNet.Numerics.dll"
COPY  "%HOMEPATH%\.nuget\packages\mathnet.spatial\0.6.0\lib\netstandard2.0\MathNet.Spatial.dll" "..\Assets\Packages\MathNet.Spatial.dll"
COPY  "%HOMEPATH%\.nuget\packages\newtonsoft.json\13.0.1\lib\netstandard2.0\Newtonsoft.Json.dll" "..\Assets\Packages\Newtonsoft.Json.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0\System.Buffers.dll" "..\Assets\Packages\System.Buffers.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.memory\4.5.3\lib\netstandard2.0\System.Memory.dll" "..\Assets\Packages\System.Memory.dll"
COPY  "%HOMEPATH%\.nuget\packages\system.runtime.compilerservices.unsafe\6.0.0\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll" "..\Assets\Packages\System.Runtime.CompilerServices.Unsafe.dll"
COPY  "%HOMEPATH%\.nuget\packages\portable.bouncycastle\1.9.0\lib\netstandard2.0\BouncyCastle.Crypto.dll" "..\Assets\Packages\BouncyCastle.Crypto.dll"
COPY  "%HOMEPATH%\.nuget\packages\ionic.zip\1.9.1.8\lib\Ionic.Zip.dll" "..\Assets\Packages\Ionic.Zip.dll"

CALL :highlight "\n!!! Finished Copying Dependencies !!!\n"
