#include <stdint.h>

extern "C" {
	int32_t UserSwitchInput(int32_t nSwitch) {
		return 0;
	}
	int32_t LedInput(int32_t led) {
		return 0;
	}
	int32_t LedOutput(int32_t led, int32_t value) {
		return 0;
	}
}
