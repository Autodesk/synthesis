#include "robot_output_service.hpp"
#include "send_data.hpp"

#include<unistd.h>
#include<utility>

using namespace grpc;
using namespace EmulationService;

Status RobotOutputService::RobotOutputs(
                                      ServerContext* context,
                                      const RobotOutputsRequest* request,
                                      ServerWriter<RobotOutputsResponse>* stream) {

    for (;;) {
        auto instance = hel::SendDataManager::getInstance();
        
        if(instance.first->hasNewData()){
            auto data = instance.first->syncShallow();
            instance.second.unlock();
            auto res = RobotOutputsResponse{};
            res.set_api("v1");
            *res.mutable_output_data() = data;
            res.set_success(true);
            stream->Write(res);
        } else {
            instance.second.unlock();
        }
        usleep(30000);
    }
    return Status::OK;
}

