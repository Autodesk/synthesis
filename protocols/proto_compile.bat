@echo off
md ..\api\Gen\Proto\
protoc --csharp_out=../api/Gen/Proto/ v1/ProtoBot.proto

