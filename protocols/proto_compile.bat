@echo on
md ..\api\Api\Gen\Proto\
md ..\api\Api\Gen\Mirabuf\
md ..\server\ServerApi\Gen\Proto\
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto -I=v1/
protoc --proto_path=v1/server --csharp_out=../server/ServerApi/Gen/Proto/ v1/server/*.proto -I=v1/server/
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf ../mirabuf/*.proto

pause
