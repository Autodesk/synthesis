#include "robot_input_service.hpp"
#include "receive_data.hpp"

#include <utility>

using namespace grpc;
using namespace EmulationService;

Status RobotInputService::RobotInputs(
    ServerContext* /*context*/, ServerReader<UpdateRobotInputsRequest>* stream,
	UpdateRobotInputsResponse* response) {
	UpdateRobotInputsRequest req;
	while (stream->Read(&req)) {
		auto instance = hel::ReceiveDataManager::getInstance();
		instance.first->sync(req.output_data());
		instance.first->updateShallow();
		instance.second.unlock();
	}

	response->set_api("v1");
	response->set_success(true);
	return Status::OK;
}
