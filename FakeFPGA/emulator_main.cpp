#include "ChipObject/NiFakeFpga.h"
#include "ChipObject/NiFpga.h"
#include "ChipObject/NiFpgaState.h"
#include <string.h>
#include <stdio.h>
#include <WPILib.h>
#include <math.h>
#include <tDIO.h>
#include "ChipObject/tDIOImpl.h"
#include "ChipObject/tSolenoidImpl.h"
#include "OSAL/System.h"
#include "PWMDecoder.h"
#include "StateNetwork/StatePacket.h"
#include "StateNetwork/StateNetworkServer.h"
#include "FRCNetComm/FRCNetImpl.h"

#ifndef __ROBOTDEMO
#define __ROBOTDEMO
class RobotDemo : public SimpleRobot {
private:
	RobotDrive drive;
	Joystick joy;
	Encoder enc;
public:
	RobotDemo(void): drive(1,2), joy(1),enc(1,2) {
	}
	void Autonomous(void) {
		printf("Entering autonomous!\n");
		drive.SetSafetyEnabled(false);
	}
	void OperatorControl(void) {
		printf("Entering teleop!\n");
		drive.SetSafetyEnabled(true);
		while (IsOperatorControl() && !IsDisabled()) {
			drive.ArcadeDrive(joy);
			Sleep(15);
		}
	}
};
START_ROBOT_CLASS(RobotDemo)
#endif

	int main(int argc, char ** argv) {
		printf("Start now!\n");
		NiFpga_Initialize();
		printf("Init FPGA\n");
		FRC_UserProgram_StartupLibraryInit();
		StatePacket pack = StatePacket();
		StateNetworkServer serv = StateNetworkServer();
		serv.Open();
		tRioStatusCode status;
		while (true) {
			for (int i = 0; i<8; i++){
				float curr = PWMDecoder::decodePWM(GetFakeFPGA()->getDIO(0), i);
				pack.pwmValues[i] = curr;
			}
			pack.solenoidValues = GetFakeFPGA()->getSolenoid()->readDO7_0(0, &status);
			serv.SendStatePacket(pack);
			sleep_ms(50);
		}
}
