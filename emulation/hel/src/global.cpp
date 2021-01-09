#include "roborio_manager.hpp"
#include "system_interface.hpp"

#include "robot_input_service.hpp"
#include "robot_output_service.hpp"

#include <grpc++/grpc++.h>
#include <chrono>

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tGlobal.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {

int gRPCPort = 50051;

// constexpr const float TIME_CONSTANT = 1.13;  // This is the offset from local time to real world time

Global::Global()noexcept { 
	fpga_start_time = getCurrentTime();
}

Global::Global(const Global& source) noexcept {
#define COPY(NAME) NAME = source.NAME
	COPY(fpga_start_time);
#undef COPY
}

uint64_t Global::getCurrentTime() noexcept {
	return std::chrono::duration_cast<std::chrono::microseconds>(
			   std::chrono::high_resolution_clock::now().time_since_epoch()).count();  // FIXME system time runs fast
}

uint64_t Global::getFPGAStartTime() const noexcept { return fpga_start_time; }

struct GlobalManager : public tGlobal {
	tSystemInterface* getSystemInterface() { return new SystemInterface(); }

	void writeLEDs(tLEDs /*value*/, tRioStatusCode* /*status*/) {
	}  // unnecessary for emulation

	void writeLEDs_Comm(uint8_t /*value*/, tRioStatusCode* /*status*/) {
	}  // unnecessary for emulation

	void writeLEDs_Mode(uint8_t /*value*/, tRioStatusCode* /*status*/) {
	}  // unnecessary for emulation

	void writeLEDs_RSL(bool /*value*/, tRioStatusCode* /*status*/) {
	}  // unnecessary for emulation

	tLEDs readLEDs(tRioStatusCode* /*status*/) {  // unnecessary for emulation
		return *(new tGlobal::tLEDs);
	}

	uint8_t readLEDs_Comm(
		tRioStatusCode* /*status*/) {  // unnecessary for emulation
		return 0;
	}

	uint8_t readLEDs_Mode(
		tRioStatusCode* /*status*/) {  // unnecessary for emulation
		return 0;
	}

	bool readLEDs_RSL(tRioStatusCode* /*status*/) {  // unnecessary for
													 // emulation
		return false;
	}

	uint32_t readLocalTimeUpper(tRioStatusCode* /*status*/) {
		auto instance = RoboRIOManager::getInstance();
		instance.second.unlock();
		return (Global::getCurrentTime() -
				instance.first->global.getFPGAStartTime()) >>
			   32;
	}

	uint16_t readVersion(tRioStatusCode* /*status*/) {
		return 2018;  // WPILib assumes this is the competition year
	}

	uint32_t readLocalTime(tRioStatusCode* /*status*/) {
		auto instance = RoboRIOManager::getInstance();
		instance.second.unlock();
		return (uint32_t)(Global::getCurrentTime() -
						  instance.first->global.getFPGAStartTime());
	}

	bool readUserButton(tRioStatusCode* /*status*/) {
		auto instance = RoboRIOManager::getInstance();
		instance.second.unlock();
		return instance.first->user_button;
	}

	uint32_t readRevision(
		tRioStatusCode* /*status*/) {  // unnecessary for emulation
		return 0;
	}
	void strobeInterruptForceOnce(tRioStatusCode* /*status*/) {
		return;
	}
	void writeInterruptForceNumber(uint8_t value, tRioStatusCode* /*status*/) {
		auto instance = RoboRIOManager::getInstance();
		instance.first->interrupt_force_number = value;
		instance.second.unlock();
		return;
	}

	uint8_t readInterruptForceNumber(tRioStatusCode* /*status*/) {
		auto instance = RoboRIOManager::getInstance();
		instance.second.unlock();
		return instance.first->interrupt_force_number;
	}

};
}  // namespace hel

std::thread grpc_thread;

namespace nFPGA {
namespace nRoboRIO_FPGANamespace {
tGlobal* tGlobal::create(tRioStatusCode* /*status*/) {
	grpc_thread = std::thread([] {
		std::string server_addr("0.0.0.0:" + std::to_string(hel::gRPCPort));
		RobotInputService inputs;
		RobotOutputService outputs;

		grpc::ServerBuilder builder;
		builder.AddListeningPort(server_addr,
								 grpc::InsecureServerCredentials());
		builder.RegisterService(&inputs);
		builder.RegisterService(&outputs);
		auto server = builder.BuildAndStart();
		printf("gRPC serving on %s.\n", server_addr.c_str());
		server->Wait();
	});
	grpc_thread.detach();
	return new hel::GlobalManager();
}
}  // namespace nRoboRIO_FPGANamespace
}  // namespace nFPGA
