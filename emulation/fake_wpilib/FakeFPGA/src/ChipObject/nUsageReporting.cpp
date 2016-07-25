#include <NetworkCommunication/UsageReporting.h>
#include <stddef.h>
#include <stdint.h>
#include "ChipObject/tDIOImpl.h"
#include "ChipObject/NiFakeFpga.h"

namespace nUsageReporting {
	// type, channel, module
	// This can be used to figure out what is one each port.  That allows us to decode the PWM values
	uint32_t EXPORT_FUNC report(tResourceType resource, uint8_t instanceNumber,
		uint8_t context, const char *feature) {
			switch (resource) {
			case kResourceType_Jaguar:
			case kResourceType_Victor:
			case kResourceType_Servo:
			case kResourceType_Talon: {
				GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber-1] = resource;
				break;
									  }
			case kResourceType_PWM: {
				uint8_t type = GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber];
				// Don't overwrite the more specific ones
				if (type != kResourceType_Jaguar && type != kResourceType_Victor
					&& type != kResourceType_Talon && type != kResourceType_Servo) {
						GetFakeFPGA()->getDIO(context)->pwmTypes[instanceNumber-1] = resource;
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
uint32_t FRC_NetworkCommunication_nUsageReporting_report(
	uint8_t resource, uint8_t instanceNumber, uint8_t context,
	const char *feature) {
		return nUsageReporting::report((nUsageReporting::tResourceType) resource, instanceNumber, context, feature);
}
