git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..\exporters\SynthesisFusionAddin\proto
call build.bat
cd ..\..\..
