package main

import "github.com/autodesk/synthesis/emulation/bridge/pkg/service/v1"
import "context"
import "github.com/autodesk/synthesis/emulation/bridge/pkg/protocol/grpc"

func main() {
	emuSrv := v1.NewEmulationBridgeService()
	emuSrv.RegisterClient("native", "Native", "localhost:50052")
	emuSrv.ActivateClient("native", true)
	grpc.RunServer(context.Background(), emuSrv, emuSrv)
}
