
#ifndef __AICalibration_h__
#define __AICalibration_h__



#ifdef __cplusplus
extern "C"
{
#endif

	__declspec(dllexport) uint32_t FRC_NetworkCommunication_nAICalibration_getLSBWeight(const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status);
	__declspec(dllexport) int32_t FRC_NetworkCommunication_nAICalibration_getOffset(const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status);

#ifdef __cplusplus
}
#endif

#endif // __AICalibration_h__
