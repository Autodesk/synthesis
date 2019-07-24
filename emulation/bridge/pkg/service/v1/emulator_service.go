package v1

import (
	"context"
	"fmt"
	"io"

	v1 "github.com/autodesk/synthesis/emulation/bridge/pkg/api/v1"
	"google.golang.org/grpc"
)

type ServInfo struct {
	Type, Name, Addr string
}

type EmulationBridgeService struct {
	clientInfo      []ServInfo
	defaultHandlers map[string]string
	activeConns     map[string]*grpc.ClientConn
}

func NewEmulationBridgeService() *EmulationBridgeService {
	return &EmulationBridgeService{
		clientInfo:      make([]ServInfo, 0),
		activeConns:     make(map[string]*grpc.ClientConn),
		defaultHandlers: make(map[string]string),
	}
}

func (eb *EmulationBridgeService) RegisterClient(name, serverType, addr string) error {
	if _, ok := eb.activeConns[name]; ok {
		return fmt.Errorf("server '%s' already registered", name)
	}
	for _, elem := range eb.clientInfo {
		if elem.Name == name {
			return fmt.Errorf("server name '%s' already taken", name)
		}
	}
	eb.clientInfo = append(eb.clientInfo, ServInfo{
		Name: name,
		Type: serverType,
		Addr: addr,
	})
	return nil
}

func (eb *EmulationBridgeService) ActivateClient(name string, isDefault bool) error {

	servInfo, err := eb.getServInfoByName(name)
	if err != nil {
		return err
	}
	conn, err := grpc.Dial(servInfo.Addr, grpc.WithInsecure())
	if err != nil {
		return err
	}
	if isDefault {
		for k, v := range eb.defaultHandlers {
			if k == servInfo.Type {
				return fmt.Errorf("failed to register '%s' as default handler for '%s'. '%s' already registered", name, servInfo.Type, v)
			}
		}
	}
	eb.activeConns[name] = conn
	if isDefault {
		eb.defaultHandlers[servInfo.Type] = name
	}
	return nil
}

func (eb EmulationBridgeService) getServInfoByName(name string) (*ServInfo, error) {
	for _, elem := range eb.clientInfo {
		if elem.Name == name {
			return &elem, nil
		}
	}
	return nil, fmt.Errorf("failed to find server info for server named '%s'", name)

}

func (eb *EmulationBridgeService) RobotOutputs(req *v1.RobotOutputsRequest, steam v1.EmulationReader_RobotOutputsServer) error {
	var handlerType string
	var reader v1.EmulationReaderClient
	switch req.TargetPlatform {
	case v1.TargetPlatform_JAVA:
		handlerType = "Java"
	case v1.TargetPlatform_NATIVE:
		handlerType = "Native"
	case v1.TargetPlatform_OTHER:
		handlerType = "Other"
	}
	if conn, ok := eb.activeConns[eb.defaultHandlers[handlerType]]; ok {
		reader = v1.NewEmulationReaderClient(conn)
	} else {
		return fmt.Errorf("handler not registered for 'RobotInputs'")
	}

	clientStream, err := reader.RobotOutputs(context.Background(), req)
	if err != nil {
		return err
	}
	for {
		robotOutputs, err := clientStream.Recv()
		if err != nil {
			return err
		}
		fmt.Printf("%v\n", robotOutputs)
		steam.Send(robotOutputs)
	}
}

func (eb *EmulationBridgeService) RobotInputs(stream v1.EmulationWriter_RobotInputsServer) error {
	var handlerType string
	var writer v1.EmulationWriterClient = nil
	var clientStream v1.EmulationWriter_RobotInputsClient
	for {
		var err error
		inputs, err := stream.Recv()
		if err == io.EOF {
			stream.SendAndClose(&v1.UpdateRobotInputsResponse{
				Api: "v1",
			})
			return nil
		} else if err != nil {
			return err
		}
		if writer == nil {
			switch inputs.TargetPlatform {
			case v1.TargetPlatform_JAVA:
				handlerType = "Java"
			case v1.TargetPlatform_NATIVE:
				handlerType = "Native"
			case v1.TargetPlatform_OTHER:
				handlerType = "Other"
			}
			if conn, ok := eb.activeConns[eb.defaultHandlers[handlerType]]; ok {
				writer = v1.NewEmulationWriterClient(conn)
			} else {
				return fmt.Errorf("handler not registered for 'RobotInputs'")
			}
			if clientStream == nil {
				clientStream, err = writer.RobotInputs(context.Background())
				if err != nil {
					return err
				}
			}
		}
		err = clientStream.Send(inputs)
		if err == io.EOF {
			break
		} else if err != nil {
			return err
		}
	}
	return nil
}
