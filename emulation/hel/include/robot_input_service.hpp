#ifndef _ROBOT_INPUT_SERVICE_HPP_
#define _ROBOT_INPUT_SERVICE_HPP_

#include <emulator_service.grpc.pb.h>

/**
 * \brief Supervisor for gRPC robot input receiver
 */

class RobotInputService final
	: public EmulationService::EmulationWriter::Service {
   public:
	virtual grpc::Status RobotInputs(
		grpc::ServerContext*,
		grpc::ServerReader<EmulationService::UpdateRobotInputsRequest>*,
		EmulationService::UpdateRobotInputsResponse*) override;
};
#endif
