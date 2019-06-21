#include "robot_output_service.hpp"
#include "send_data.hpp"

#include<utility>

using namespace grpc;
using namespace EmulationService;

Status RobotOutputService::RobotOutputs(
                                      ServerContext* context,
                                      RobotOutputRequest* request,
                                      ServerReader<RobotOutputResponse>* stream) {
    for (;;) {
        
    }
}

