#ifndef _ROBOT_OUTPUT_SERVICE_HPP_
#define _ROBOT_OUTPUT_SERVICE_HPP_

#include <emulator_service.grpc.pb.h>

class RobotOutputService final
	: public EmulationService::EmulationReader::Service {
   public:
	virtual grpc::Status RobotOutputs(
		grpc::ServerContext*, const EmulationService::RobotOutputsRequest*,
		grpc::ServerWriter<EmulationService::RobotOutputsResponse>*) override;
};
#endif
