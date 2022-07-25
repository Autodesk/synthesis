@echo on
md ..\api\Api\Gen\Proto\
md ..\api\Api\Gen\Mirabuf\
md ..\server\ServerApi\Gen\Proto\
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --csharp_out=../server/ServerApi/Gen/Proto/ v1/server/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf ../mirabuf/*.proto

pause
