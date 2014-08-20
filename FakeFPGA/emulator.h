#ifndef __EMULATOR_H
#define __EMULATOR_H
#include <stdint.h>
#include <WPILib.h>	 // Ensure we have the START_ROBOT_CLASS Macro once so we can override it.

// Used to specify your team ID
extern const int TEAM_ID;

int StartEmulator();
extern "C" {
	int32_t FRC_UserProgram_StartupLibraryInit();
}

#ifndef START_ROBOT_CLASS
#error "emulator.h needs to be included AFTER WPILib"
#endif

#undef START_ROBOT_CLASS // Squash warnings

// Override the default FRC robot class start macro.
#define START_ROBOT_CLASS(_ClassName_) \
	void *FRC_userClassFactory() \
	{ \
		return new _ClassName_(); \
	} \
	extern "C" { \
		int32_t FRC_UserProgram_StartupLibraryInit() \
		{ \
			RobotBase::startRobotTask(FRC_userClassFactory); \
			return 0; \
		} \
		int main(int argc, char ** argv) { StartEmulator(); } \
	}
#endif