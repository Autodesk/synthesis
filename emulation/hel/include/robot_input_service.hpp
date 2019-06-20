#ifndef _ROBOT_INPUT_SERVICE_HPP_
#define _ROBOT_INPUT_SERVICE_HPP_

#include<emulator_service.grpc.pb.h>

class RobotInputService final :
    public EmulationService::EmulationReader::Service {
public:
    virtual grpc::Status RobotInputs(
                                    grpc::ServerContext*,
                                    grpc::ServerReader<EmulationService::UpdateRobotInputsRequest>*,
                                    EmulationService::UpdateRobotInputsResponse*);
};
#endif
