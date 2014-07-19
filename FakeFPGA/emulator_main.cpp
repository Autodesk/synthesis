#include "ChipObject/NiFpga.h"
#include "ChipObject/NiFpgaState.h"
#include <string.h>
#include <stdio.h>
#include <WPILib.h>
#include <math.h>
#include <unistd.h>
#include "ChipObject/tDIOImpl.h"

namespace nFPGA {
NiFpgaState *state;
}

int main(int argc, char ** argv) {
	printf("Start now!\n");
	NiFpga_Initialize();
	printf("Init FPGA\n");
	Talon *t = new Talon(1, 1);
	float j = 0;
	while (true) {
		t->Set(sin(j));
		j += 0.1;
		printf("%d\n", state->getDIO(0)->pwmValue[0]);
		sleep(1);
	}
}
