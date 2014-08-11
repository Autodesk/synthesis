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
#include "ChipObject/tAIImpl.h"
#include "ChipObject/tEncoderImpl.h"
#include "ChipObject/tCounterImpl.h"
#include "OSAL/System.h"
#include "PWMDecoder.h"
#include "StateNetwork/StatePacket.h"
#include "StateNetwork/StateNetworkServer.h"
#include "FRCNetComm/FRCNetImpl.h"
#include "emulator.h"

int StartEmulator() {
	printf("Start now!\n");
	NiFpga_Initialize();
	printf("Init FPGA\n");
	FRC_UserProgram_StartupLibraryInit();
	OutputStatePacket pack = OutputStatePacket();
	InputStatePacket sensors = InputStatePacket();
	StateNetworkServer serv = StateNetworkServer();
	serv.Open();
	tRioStatusCode status;
	while (true) {
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
		sleep_ms(50);
	}
}
