package main

import v1 "github.com/autodesk/synthesis/emulation/bridge/pkg/service/v1"
import "context"
import "github.com/autodesk/synthesis/emulation/bridge/pkg/protocol/grpc"

func main() {
	emuSrv := v1.NewEmulationBridgeService()
	emuSrv.RegisterClient("native", "Native", "localhost:50052")
	emuSrv.RegisterClient("java", "Java", "localhost:50053")
	emuSrv.ActivateClient("native", true)
	emuSrv.ActivateClient("java", true)
	grpc.RunServer(context.Background(), emuSrv, emuSrv)
}
