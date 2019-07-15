#include "robot_input_service.hpp"
#include "robot_inputs.hpp"

#include <utility>

using namespace grpc;
using namespace EmulationService;

Status RobotInputService::RobotInputs(
	ServerContext* /*context*/, ServerReader<UpdateRobotInputsRequest>* stream,
	UpdateRobotInputsResponse* response) {
	UpdateRobotInputsRequest req;
	while (stream->Read(&req)) {
		auto instance = hel::RobotInputsManager::getInstance();
		instance.first->sync(req.input_data());
		std::cout << instance.first->toString() << "\n";
		instance.first->updateShallow();
		instance.second.unlock();
	}

	response->set_api("v1");
	response->set_success(true);
	return Status::OK;
}
