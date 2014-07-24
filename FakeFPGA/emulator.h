#ifndef __EMULATOR_H
#define __EMULATOR_H
#include <stdint.h>
int StartEmulator();
extern "C" {
	int32_t FRC_UserProgram_StartupLibraryInit();
}


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