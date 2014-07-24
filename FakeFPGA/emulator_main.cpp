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
#include "emulator.h"

int StartEmulator() {
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
			printf("%.03f ", pack.pwmValues[i]);
		}
		pack.solenoidValues = GetFakeFPGA()->getSolenoid()->readDO7_0(0, &status);
		printf("\t");
		for (int i =0; i<8; i++) {
			printf("%d ", (pack.solenoidValues >> i) & 1);
		}
		printf("\n");
		serv.SendStatePacket(pack);
		sleep_ms(50);
	}
}
