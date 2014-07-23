#include <NetworkCommunication/FRCComm.h>
#include <NetworkCommunication/AICalibration.h>
#include <stdio.h>

#include "FRCNetImpl.h"
#include "FRCFakeNetComm.h"

extern "C" {
	static FRCRobotControl robotControlData;

	void EXPORT_FUNC getFPGAHardwareVersion(uint16_t *fpgaVersion,
		uint32_t *fpgaRevision) {
	}
	/**
	* Get the control data from the driver station. The parameter "data"
	* is only updated when the method returns 0.
	*
	* @param data the object to store the results in (out param)
	* @param wait_ms the maximum time to wait
	* @return 0 if new data, 1 if no new data, 2 if access timed out.
	*/
	int EXPORT_FUNC getCommonControlData(FRCCommonControlData *data, int wait_ms) {
		if (!GetFakeNetComm()->waitForNewPacket(wait_ms)) {
			return 2;
		}
		*data = GetFakeNetComm()->getLastPacket();
		return 0;
	}
	int EXPORT_FUNC getRecentCommonControlData(FRCCommonControlData *commonData,
		int wait_ms) {
			*commonData = GetFakeNetComm()->getLastPacket();
			return 0;
	}
	int EXPORT_FUNC getRecentStatusData(uint8_t *batteryInt, uint8_t *batteryDec,
		uint8_t *dsDigitalOut, int wait_ms) {
			return 0;
	}

	/**
	* Get the dynamic control data from the driver station. The parameter 
	* "dynamicData" is only updated when the method returns 0.
	* @param type The type to get.
	* @param dynamicData The array to hold the result in.
	* @param maxLength The maximum length of the data.
	* @param wait_ms The maximum time to wait.
	* @return 0 if new data, 1 if no new data, 2 if access timed out.
	*/
	int EXPORT_FUNC getDynamicControlData(uint8_t type, char *dynamicData,
		int32_t maxLength, int wait_ms) {
			return 0;
	}


	/**
	* Set the status data to send to the ds
	*
	* @param battery the battery voltage
	* @param dsDigitalOut value to set the digital outputs on the ds to
	* @param updateNumber unique ID for this update (incrementing)
	* @param userDataHigh additional high-priority user data bytes
	* @param userDataHighLength number of high-priority data bytes
	* @param userDataLow additional low-priority user data bytes
	* @param userDataLowLength number of low-priority data bytes
	* @param wait_ms the timeout
	* @return 0 on success, 1 if userData.length is too big, 2 if semaphore could not be taken in wait_ms.
	*/
	int EXPORT_FUNC setStatusData(float battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, int wait_ms) {
			int bInt = (int) battery;
			uint8_t chunkA = ((bInt/10) << 4) | (bInt % 10);
			int bFrac = (float)((battery - (float) bInt) * 100.0f);
			uint8_t chunkB = ((bFrac/10) << 4) | (bFrac % 10);
			return setStatusDataFloatAsInt((chunkA << 8) | chunkB, dsDigitalOut, updateNumber, userDataHigh, userDataHighLength, userDataLow, userDataLowLength, wait_ms);
	}

	int EXPORT_FUNC setStatusDataFloatAsInt(int battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, int wait_ms) {
			robotControlData.packetIndex = updateNumber;
			robotControlData.digitalOutputs = dsDigitalOut;
			robotControlData.batteryVolts = (battery >> 8) & 0xFF;
			robotControlData.batteryMilliVolts = (battery & 0xFF);
			robotControlData.teamID = GetFakeNetComm()->getLastPacket().teamID;
			// I don't want to deal with the rest.
			return 0;
	}

	/**
	* Send data to the driver station's error panel
	* @param bytes the byte array containing the properly formatted information for the display
	* @param length the length of the byte array
	* @param timeOut the maximum time to wait
	*/
	int EXPORT_FUNC setErrorData(const char *errors, int errorsLength,
		int wait_ms) {
			printf("%s", errors);
			return 0;
	}

	/**
	* Send data to the driver station's user panel
	* @param userDsLcdData the byte array containing the properly formatted information for the display
	* @param userDsLcdDataLength the length of the byte array
	* @param wait_ms the maximum time to wait
	*/
	int EXPORT_FUNC setUserDsLcdData(const char *userDsLcdData,
		int userDsLcdDataLength, int wait_ms) {
			return 0;
	}
	int EXPORT_FUNC overrideIOConfig(const char *ioConfig, int wait_ms) {
		return 0;
	}

	void EXPORT_FUNC setNewDataSem(WaitSemaphore* r) {
		GetFakeNetComm()->newDataSem=r;
	}
	void EXPORT_FUNC setResyncSem(ReentrantSemaphore* r) {
		GetFakeNetComm()->resyncSem = r;
	}
	void EXPORT_FUNC signalResyncActionDone(void) {
		if (GetFakeNetComm()->resyncSem != NULL) {
			GetFakeNetComm()->resyncSem->give();
		}
	}

	// this uint32_t is really a LVRefNum
	void EXPORT_FUNC setNewDataOccurRef(uint32_t refnum) {
		printf("FAIL\n");
	}
	void EXPORT_FUNC setResyncOccurRef(uint32_t refnum) {
		printf("FAIL\n");
	}

	void EXPORT_FUNC FRC_NetworkCommunication_getVersionString(char *version) {
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramStarting(void) {
		robotControlData.control = 0;
		robotControlData.notEStop = 1;
		GetFakeNetComm()->sendControl(robotControlData);
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramDisabled(void) {
		robotControlData.control = 0;
		robotControlData.notEStop = 1;
		GetFakeNetComm()->sendControl(robotControlData);
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramAutonomous(void) {
		robotControlData.control = 0;
		robotControlData.notEStop = 1;
		robotControlData.enabled = 1;
		robotControlData.autonomous = 1;
		GetFakeNetComm()->sendControl(robotControlData);
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTeleop(void) {
		robotControlData.control = 0;
		robotControlData.notEStop = 1;
		robotControlData.enabled = 1;
		GetFakeNetComm()->sendControl(robotControlData);
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTest(void) {
		robotControlData.control = 0;
		robotControlData.notEStop = 1;
		robotControlData.enabled = 1;
		robotControlData.test = 1;
		GetFakeNetComm()->sendControl(robotControlData);
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
