protoc -I ../api/v1/proto \
    -I$GOPATH/src \
    --go_out=plugins=grpc:pkg/api/v1 \
    ../api/v1/proto/emulator_service.proto