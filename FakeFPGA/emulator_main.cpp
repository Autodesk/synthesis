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
		printf("VALUE: %f->%f\t\t%d\n", val, t->Get(),
				GetFakeFPGA()->getDIO(0)->pwmValue[0]);
		sleep_ms(1000);
	}
}
