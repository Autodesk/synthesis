#include<emulator_service.grpc.pb.h>
#include<grpc++/grpc++.h>

class RobotOuputService final :
    public EmulationService::EmulationWriter::Service {
public:
    virtual grpc::Status RobotOuputs(
                                     grpc::ServerContext*,
                                     const EmulationService::RobotOutputsRequest*,
                                     grpc::ServerWriter<EmulationService::RobotOutputsResponse>*);

};
