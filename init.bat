git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..\exporter\SynthesisFusionAddin\proto
call build.bat
cd ..\..\..
