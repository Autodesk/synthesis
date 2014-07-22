#include <NetworkCommunication/FRCComm.h>
#include <NetworkCommunication/AICalibration.h>
#include <stdio.h>

extern "C" {
	void EXPORT_FUNC getFPGAHardwareVersion(uint16_t *fpgaVersion,
		uint32_t *fpgaRevision) {
	}
	int EXPORT_FUNC getCommonControlData(FRCCommonControlData *data, int wait_ms) {
		return 0;
	}
	int EXPORT_FUNC getRecentCommonControlData(FRCCommonControlData *commonData,
		int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC getRecentStatusData(uint8_t *batteryInt, uint8_t *batteryDec,
		uint8_t *dsDigitalOut, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC getDynamicControlData(uint8_t type, char *dynamicData,
		int32_t maxLength, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC setStatusData(float battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC setStatusDataFloatAsInt(int battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC setErrorData(const char *errors, int errorsLength,
		int wait_ms) {
			printf("%s", errors);
			return 0;
	}
	int EXPORT_FUNC setUserDsLcdData(const char *userDsLcdData,
		int userDsLcdDataLength, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC overrideIOConfig(const char *ioConfig, int wait_ms) {
		return 0;
	}

	void EXPORT_FUNC setNewDataSem(ReentrantSemaphore* r) {
	}
	void EXPORT_FUNC setResyncSem(ReentrantSemaphore* r) {
	}
	void EXPORT_FUNC signalResyncActionDone(void) {
	}

	// this uint32_t is really a LVRefNum
	void EXPORT_FUNC setNewDataOccurRef(uint32_t refnum) {
	}
	void EXPORT_FUNC setResyncOccurRef(uint32_t refnum) {
	}

	void EXPORT_FUNC FRC_NetworkCommunication_getVersionString(char *version) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramStarting(void) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramDisabled(void) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramAutonomous(void) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTeleop(void) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTest(void) {
	}
	void FRC_NetworkCommunication_JaguarCANDriver_sendMessage(uint32_t messageID, const uint8_t *data, uint8_t dataSize, int32_t *status){

	}
	void FRC_NetworkCommunication_JaguarCANDriver_receiveMessage(uint32_t *messageID, uint8_t *data, uint8_t *dataSize, uint32_t timeoutMs, int32_t *status){

	}
}

uint32_t FRC_NetworkCommunication_nAICalibration_getLSBWeight(
	const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status) {
		return 0;
}
int32_t FRC_NetworkCommunication_nAICalibration_getOffset(
	const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status) {
		return 0;
}
