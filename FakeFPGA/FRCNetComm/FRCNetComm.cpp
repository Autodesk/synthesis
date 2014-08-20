#include <NetworkCommunication/FRCComm.h>
#include <NetworkCommunication/AICalibration.h>
#include <stdio.h>
#include <math.h>

#include "FRCNetImpl.h"
#include "FRCFakeNetComm.h"
#include <CAN/can_proto.h>

extern "C" {
	static RobotControlByte control;

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
			GetFakeNetComm()->getDynamicData(type,dynamicData, maxLength);
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
			int bFrac = (int) ((battery - (float) bInt) * 1000.0f);
			if ((bFrac % 10) >= 5) {
				bFrac = (bFrac/10) + 1;
			} else {
				bFrac /= 10;
			}
			uint8_t chunkB = ((bFrac/10) << 4) | (bFrac % 10);
			return setStatusDataFloatAsInt((chunkA << 8) | chunkB, dsDigitalOut, updateNumber, userDataHigh, userDataHighLength, userDataLow, userDataLowLength, wait_ms);
	}

	int EXPORT_FUNC setStatusDataFloatAsInt(int battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, int wait_ms) {
			GetFakeNetComm()->setStatus(battery, dsDigitalOut, updateNumber, userDataHigh, userDataHighLength, userDataLow, userDataLowLength, control.control);
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
			GetFakeNetComm()->setEmbeddedDynamicChunk(FRCNetImpl::kEmbeddedErrors, errors, errorsLength);
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
		return 0;	// This is used for enhanced IO ROBOT->DS
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
		control.control = 0;
		control.notEStop = 1;
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramDisabled(void) {
		control.control = 0;
		control.notEStop = 1;
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramAutonomous(void) {
		control.control = 0;
		control.notEStop = 1;
		control.enabled = 1;
		control.autonomous = 1;
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTeleop(void) {
		control.control = 0;
		control.notEStop = 1;
		control.enabled = 1;
	}
	void EXPORT_FUNC FRC_NetworkCommunication_observeUserProgramTest(void) {
		control.control = 0;
		control.notEStop = 1;
		control.enabled = 1;
		control.test = 1;
	}

	const int JAG_COUNT = 1 << 5;
	const int JAG_PORT_MASK = JAG_COUNT - 1;
	float JAG_SPEEDS[32];
	bool ACKS[32];

	float getJagSpeed(int i ) {return JAG_SPEEDS[i];}

	void FRC_NetworkCommunication_JaguarCANDriver_sendMessage(uint32_t messageID, const uint8_t *data, uint8_t dataSize, int32_t *status){
		int devNum = (messageID & JAG_PORT_MASK) -1;
		if ((messageID & LM_API_VOLT_T_SET) == LM_API_VOLT_T_SET) {
			ACKS[devNum] = true;
			int16_t value = *((int16_t*)(data+2));
			value = ntohs(value);
			JAG_SPEEDS[devNum] = (float) value / 32767.0;
		}
	}
	void FRC_NetworkCommunication_JaguarCANDriver_receiveMessage(uint32_t *messageID, uint8_t *data, uint8_t *dataSize, uint32_t timeoutMs, int32_t *status){
		int devNum = (*messageID & JAG_PORT_MASK)-1;
		if (data==NULL){
			*status = 0;
			return;
		}
		if ((*messageID & LM_API_ACK) == LM_API_ACK) {
			if (!ACKS[devNum]) {
				printf("Ack failed\n");
			}
			ACKS[devNum] = false;
			*status = 0;
		}else if ((*messageID & LM_API_VOLT_T_SET) == LM_API_VOLT_T_SET) {
			int16_t intValue = (int16_t)(JAG_SPEEDS[devNum] * 32767.0);
			*((int16_t*)data) = htons(intValue);
			*dataSize = sizeof(int16_t);
			*status = 0;
		}else if (((*messageID) & CAN_MSGID_API_FIRMVER) == CAN_MSGID_API_FIRMVER) {
			int32_t thing = 101;
			*((int32_t*)data) = htonl(thing);
			*dataSize = sizeof(int32_t);
			*status = 0;
		}
		// I don't care
		*status = 0;
	}
}

uint32_t FRC_NetworkCommunication_nAICalibration_getLSBWeight(
	const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status) {
		return (uint32_t) ((float)(21.06 * 1.0E9) / (float)(1 << 12));
}
int32_t FRC_NetworkCommunication_nAICalibration_getOffset(
	const uint32_t aiSystemIndex, const uint32_t channel, int32_t *status) {
		return 0;
}
