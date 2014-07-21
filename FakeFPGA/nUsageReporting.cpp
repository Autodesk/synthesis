#include <NetworkCommunication/UsageReporting.h>
#include <stddef.h>
#include <stdint.h>
#include "ChipObject/tDIOImpl.h"
#include "ChipObject/NiFakeFpga.h"

namespace nUsageReporting {
	// type, channel, module
	// this can be used to ID what is on what port
	uint32_t EXPORT_FUNC report(tResourceType resource, uint8_t instanceNumber,
		uint8_t context, const char *feature) {
			switch (resource) {
			case kResourceType_Jaguar:
			case kResourceType_Victor:
			case kResourceType_Servo:
			case kResourceType_Talon: {
				GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber] = resource;
				break;
									  }
			case kResourceType_PWM: {
				uint8_t type = GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber];
				// Don't overwrite the more specific ones
				if (type != kResourceType_Jaguar && type != kResourceType_Victor
					&& type != kResourceType_Talon) {
						GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber] = resource;
				}
				break;
									}
			default:
				// Don't need this
				break;
			}
			return 0;
	}
}

#ifdef __cplusplus
extern "C" {
#endif

	uint32_t EXPORT_FUNC FRC_NetworkCommunication_nUsageReporting_report(
		uint8_t resource, uint8_t instanceNumber, uint8_t context,
		const char *feature) {
			return 0;
	}

#ifdef __cplusplus
}
#endif
