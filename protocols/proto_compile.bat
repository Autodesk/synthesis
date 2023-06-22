@echo off
md ..\api\Api\Gen\
md ..\api\Api\Gen\Proto\
md ..\api\Api\Gen\Mirabuf\
@echo on
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto