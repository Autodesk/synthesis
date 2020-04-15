@echo off
protoc --csharp_out=.\ ProtoRobot.proto
protoc --python_out=..\..\..\exporter\SynthesisExporter\protocols\ ProtoRobot.proto