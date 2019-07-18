#include "robot_output_service.hpp"
#include "robot_outputs.hpp"

#include <unistd.h>
#include <utility>
#include "util.hpp"

using namespace grpc;
using namespace EmulationService;

Status RobotOutputService::RobotOutputs(
	ServerContext* /*context*/, const RobotOutputsRequest* /*request*/,
	ServerWriter<RobotOutputsResponse>* stream) {
	for (;true;) {
		auto instance = hel::RobotOutputsManager::getInstance();

		if (instance.first->hasNewData()) {
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
		usleep(50000);
	}
	return Status::OK;
}
