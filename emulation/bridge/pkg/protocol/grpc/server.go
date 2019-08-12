package grpc

import (
	"context"
	"github.com/autodesk/synthesis/emulation/bridge/pkg/api/v1"
	"google.golang.org/grpc"
	"os"
	"os/signal"
	"log"
	"net"
)

func RunServer(ctx context.Context, readerSrv v1.EmulationReaderServer, writerSrv v1.EmulationWriterServer) error {
	lis, err := net.Listen("tcp", ":50051")
	if err != nil {
		log.Fatalf("failed to bind to port 50051: %v", err)
	}
	server := grpc.NewServer()
	v1.RegisterEmulationReaderServer(server, readerSrv);
	v1.RegisterEmulationWriterServer(server, writerSrv);

	sig := make(chan os.Signal, 1)
	signal.Notify(sig, os.Interrupt)
	go func() {
		for range sig {
			log.Println("shutting down gRPC server...")
			server.GracefulStop()
			<-ctx.Done()
		}
	}()
	log.Printf("starting gRPC server...")
	return server.Serve(lis)
}
