#include "ChipObject/NiFakeFpga.h"
#include "ChipObject/NiFpga.h"
#include "ChipObject/NiFpgaState.h"
#include <string.h>
#include <stdio.h>
#include <WPILib.h>
#include <math.h>
#include <tDIO.h>
#include "ChipObject/tDIOImpl.h"
#include "OSAL/System.h"
#include "PWMDecoder.h"

int main(int argc, char ** argv) {
	printf("Start now!\n");
	NiFpga_Initialize();
	printf("Init FPGA\n");
	Talon *t = new Talon(1, 1);
	float j = 0;
	while (true) {
		j += 0.1;
		float val = (float) sin(j);
		t->Set(val);
		float curr = PWMDecoder::decodePWM(GetFakeFPGA()->getDIO(0), t->GetChannel()-1);
		printf("VALUE: %f->%f\t\t%d\t(%f)\t\tError: %f\n", val, t->Get(),
				GetFakeFPGA()->getDIO(0)->pwmValue[0], curr, val-curr);
		sleep_ms(1000);
	}
}
