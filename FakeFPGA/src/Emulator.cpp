#include <string.h>
#include <math.h>
#include <stdio.h>
#include <OSAL/System.h>

// Definitions
#include <ChipObject/NiFpga.h>
#include <ChipObject/tDIO.h>
#include <NetworkCommunication/AICalibration.h>

// Implementations
#include "ChipObject/NiFakeFpga.h"
#include "ChipObject/NiFpgaState.h"
#include "ChipObject/tDIOImpl.h"
#include "ChipObject/tSolenoidImpl.h"
#include "ChipObject/tAIImpl.h"
#include "ChipObject/tEncoderImpl.h"
#include "ChipObject/tCounterImpl.h"
#include "ChipObject/PWMDecoder.h"
#include "StateNetwork/StatePacket.h"
#include "StateNetwork/StateNetworkServer.h"
#include "FRCNetComm/FRCNetImpl.h"
#include "Emulator.h"

#ifdef JAVA_BUILD
	extern int TEAM_ID = -1;

	void SetEmulatedTeam(int team) {
		if (TEAM_ID <= 0) {
			TEAM_ID = team;
		}
	}
#endif

int StartEmulator() {
	printf("Start now!\n");
	NiFpga_Initialize();	// Make sure we have an FPGA instance ready
	printf("Init FPGA\n");
#ifndef JAVA_BUILD
	FRC_UserProgram_StartupLibraryInit();	// Start the FRC program
#endif
	OutputStatePacket pack = OutputStatePacket();
	InputStatePacket sensors = InputStatePacket();
	StateNetworkServer serv = StateNetworkServer();
	serv.Open();
	tRioStatusCode status;
	while (true) {
		{   // Package the output packet
			for (int j = 0; j<2; j++){
				for (int i = 0; i<8; i++){
					pack.dio[j].pwmValues[i] = PWMDecoder::decodePWM(GetFakeFPGA()->getDIO(j), i);
				}
				pack.dio[j].digitalOutput = GetFakeFPGA()->getDIO(j)->readDO(&status);
				pack.dio[j].relayForward = GetFakeFPGA()->getDIO(j)->readSlowValue_RelayFwd(&status);
				pack.dio[j].relayReverse = GetFakeFPGA()->getDIO(j)->readSlowValue_RelayFwd(&status);
			}
			for (int j = 0; j < 1; j++){
				pack.solenoid[j].state = GetFakeFPGA()->getSolenoid()->readDO7_0(j, &status);
			}
			serv.SendStatePacket(pack);
		}

		// Update sensor values
		if (serv.ReceiveStatePacket(&sensors)) {
			for (int j = 0; j<2; j++){
				GetFakeFPGA()->getDIO(j)->writeDigitalPort(sensors.dio[j].digitalInput, ~(GetFakeFPGA()->getDIO(j)->readOutputEnable(&status)));
			}
			for (int j = 0; j<1; j++) {
				GetFakeFPGA()->getAnalog(j)->updateValues(sensors.ai[j].analogValues);
			}
			for (int j = 0; j<4; j++) {
				GetFakeFPGA()->getEncoder(j)->doUpdate(sensors.encoder[j].value);
			}
			// Counters?
		}
		{ // Fake Battery
			//  Volts = ((LSB_Weight * 1e-9) * raw) - (Offset * 1e-9)
			//  ((Volts * 1E9) + Offset) / LSB_Weight = raw
			int32_t status = 0;
			uint32_t lsbWeight = FRC_NetworkCommunication_nAICalibration_getLSBWeight(DriverStation::kBatteryModuleNumber-1, DriverStation::kBatteryChannel - 1, &status);
			uint32_t offset =  FRC_NetworkCommunication_nAICalibration_getOffset(DriverStation::kBatteryModuleNumber-1, DriverStation::kBatteryChannel - 1, &status);
			signed int value = (signed int) ((float) TEAM_ID * 1E7 / lsbWeight / (1680.0 / 1000.0));
			value += offset * lsbWeight;
			GetFakeFPGA()->getAnalog(DriverStation::kBatteryModuleNumber - 1)->values[DriverStation::kBatteryChannel -1] = value;
			//values[DriverStation::kBatteryChannel - 1] /=  (1680.0 / 1000.0);
		}
		// Don't eat the CPU
		sleep_ms(50);
	}
}
