#include<emulator_service.grpc.pb.h>

class RobotInputService final :
    public EmulationService::EmulationWriter::Service {
public:
    virtual grpc::Status RobotInputs(
                             grpc::ServerContext*,
                             grpc::ServerReader<EmulationService::UpdateRobotInputsRequest>*,
                             EmulationService::UpdateRobotInputsResponse*);

    
};
