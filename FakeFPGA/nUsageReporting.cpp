#include <NetworkCommunication/UsageReporting.h>
#include <stddef.h>
#include <stdint.h>

namespace nUsageReporting {
uint32_t EXPORT_FUNC report(tResourceType resource, uint8_t instanceNumber,
		uint8_t context, const char *feature) {
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
