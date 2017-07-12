/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <atomic>
#include <chrono>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <limits>

#include <pthread.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <semaphore.h>
#include <unistd.h>

#include "FRC_NetworkCommunication/FRCComm.h"
#include "HAL/DriverStation.h"
#include "HAL/cpp/priority_condition_variable.h"
#include "HAL/cpp/priority_mutex.h"
#include "DriverStation.h"
#include "crc32.h"

static_assert(sizeof(int32_t) >= sizeof(int),
              "FRC_NetworkComm status variable is larger than 32 bits");

struct HAL_JoystickAxesInt {
  int16_t count;
  int16_t axes[HAL_kMaxJoystickAxes];
};

static hal::priority_mutex msgMutex;
static hal::priority_condition_variable newDSDataAvailableCond;
static hal::priority_mutex newDSDataAvailableMutex;
static int newDSDataAvailableCounter{0};

pthread_t thread;

FRCCommonControlData lastDataPacket;
DynamicControlData lastDynamicControlPacket [32];
/*WaitSemaphore newDataSemInternal;
ReentrantSemaphore readingSem;
ReentrantSemaphore writingSem;*/
WaitSemaphore newDataSemInternal;
WaitSemaphore* newDataSem = NULL;
ReentrantSemaphore readingSem;
ReentrantSemaphore writingSem;
ReentrantSemaphore* resyncSem = NULL;
bool enabled;
FRCRobotControl ctl;

void* DriverStationThread(void* param) {
  struct sockaddr_in robotAddress;
	struct sockaddr_in dsAddress;
	int robotSocket;
	int dsSocket;

    //get's the IP address in the form 10.xx.xx.0
	uint32_t network = 0; //(10 << 24) | (((teamID / 100) & 0xFF) << 16) | ((teamID % 100) << 8) | 0;
	//uint32_t network = 0xFFFFFFFF; // 127.0.0.1

	robotAddress.sin_family = AF_INET;
    //10.xx.xx.2
	robotAddress.sin_addr.s_addr = htonl(network | 2);
	robotAddress.sin_port = htons( 1110 );

	dsAddress.sin_family = AF_INET;
    //10.xx.xx.5
	dsAddress.sin_addr.s_addr = htonl(network | 5);
	dsAddress.sin_port = htons( 1150 );

	robotSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (robotSocket < 0) {
		fprintf(stderr, "Could not create socket ROBOT!\n");
		return NULL;
	}

	if (bind(robotSocket, (const struct sockaddr *)&robotAddress, sizeof(robotAddress)) != 0) {
		fprintf(stderr, "Could not bind socket ROBOT!  Did you configure your loopback adapters?\n");
		return NULL;
	}

	dsSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (dsSocket < 0) {
		fprintf(stderr, "Could not create socket DS!  Did you configure your loopback adapters?\n");
		return NULL;
	}

	char buffer[1024];
	for (int i = 0; i < 32; i++) {
		memset(&lastDynamicControlPacket [i], 0, sizeof(lastDynamicControlPacket [i]));
	}
	bool lockedResync = false;

	// Read from the DS thread
	while (enabled) {
		if (resyncSem != NULL && !lockedResync) {
			resyncSem->take();
			lockedResync = true;
		}
		int len = recv(robotSocket, (char*) &buffer, sizeof(buffer), 0);
		if (len < 0) {
			printf("Read failed\n");
		}
		readingSem.take();

		// Convert 2015 packets to 2014 (TODO find a better way)
		{
			FRCCommonControlData p2014;
			FRCCommonControlData2015 p2015;
			memcpy(&p2015, &buffer, sizeof(p2015));
			p2014.packetIndex = p2015.packetIndex++;
			char alliance;
			char position;
			switch (p2015.station) {
			case 0:
				alliance = 'R';
				position = '1';
				break;
			case 1:
				alliance = 'R';
				position = '2';
				break;
			case 2:
				alliance = 'R';
				position = '3';
				break;
			case 3:
				alliance = 'B';
				position = '1';
				break;
			case 4:
				alliance = 'B';
				position = '2';
				break;
			case 5:
				alliance = 'B';
				position = '3';
				break;
			default:
				// eek scary an unimplemented state
				// call the developer or tech support
				// TODO actually throw an error
				break;
			}

			p2014.dsID_Alliance = alliance;
			p2014.dsID_Position = position;
			memcpy(&p2014.stick0Axes[0], &p2015.axis0[0], (size_t)6);
			memcpy(&p2014.stick1Axes[0], &p2015.axis1[0], (size_t)6);
			memcpy(&p2014.stick2Axes[0], &p2015.axis2[0], (size_t)6);
			memcpy(&p2014.stick3Axes[0], &p2015.axis3[0], (size_t)6);
			p2014.stick0Buttons = p2015.buttons0;
			p2014.stick1Buttons = p2015.buttons1;
			p2014.stick2Buttons = p2015.buttons2;
			p2014.stick3Buttons = p2015.buttons3;
			p2014.enabled = p2015.state & 4 ? true : false;
			p2014.autonomous = p2015.state & 2 ? true : false;
			p2014.test = p2015.state & 1 ? true : false;
			ctl.control.enabled = p2014.enabled;
			ctl.control.autonomous = p2014.autonomous;
			ctl.control.test = p2014.test;

			memcpy(&lastDataPacket, &p2014, sizeof(p2014));
		}

		// Reading dynamic data
		{
			int head = 115;
			uint8_t size;
			uint8_t id;
			while (head < 1024 && (size = buffer[head]) > 0) {
				uint8_t id = buffer[head+1];
				lastDynamicControlPacket [id].id=id;
				lastDynamicControlPacket [id].size=size;
				if (lastDynamicControlPacket [id].data != NULL) {
					//delete lastDynamicControlPacket [id].data; // getting a segfault??
					lastDynamicControlPacket [id].data = NULL;
				}
				lastDynamicControlPacket [id].data = (uint8_t*) malloc(size +2);
				memcpy(lastDynamicControlPacket [id].data, &buffer[head], size+2);
				head += size;
			}
		}

		{
			// Handle endians
			lastDataPacket.packetIndex = ntohs(lastDataPacket.packetIndex);
			lastDataPacket.teamID = ntohs(lastDataPacket.teamID);
			lastDataPacket.analog1 = ntohs(lastDataPacket.analog1);
			lastDataPacket.analog2 = ntohs(lastDataPacket.analog2);
			lastDataPacket.analog3 = ntohs(lastDataPacket.analog3);
			lastDataPacket.analog4 = ntohs(lastDataPacket.analog4);
			lastDataPacket.stick0Buttons = ntohs(lastDataPacket.stick0Buttons);
			lastDataPacket.stick1Buttons = ntohs(lastDataPacket.stick1Buttons);
			lastDataPacket.stick2Buttons = ntohs(lastDataPacket.stick2Buttons);
			lastDataPacket.stick3Buttons = ntohs(lastDataPacket.stick3Buttons);
		}

		readingSem.give();
		newDataSemInternal.notify();
		if (newDataSem != NULL) {
			newDataSem->notify();
		}
		// Shenanigans with semaphores
		if (lastDataPacket.resync) {
			if (resyncSem != NULL){
				resyncSem->give();
				Sleep(250);
				resyncSem->take();
			}
		}

		char sendBuffer[2048];
		writingSem.take();
		memset(&sendBuffer, 0, sizeof(sendBuffer));
		//memcpy(&sendBuffer, &ctl,  sizeof(FRCRobotControl));

		// Convert 2014 packets to 2015
		{
			FRCRobotControl2015 c2015;
			c2015.packetIndex = ctl.packetIndex++;
			c2015.voltage_greater = /*ctl.batteryVolts*/12; // who cares anyways
			int oscillation = (ctl.control.enabled ? 0 : (rand() % 2)); // don't judge
			c2015.voltage_lesser = /*ctl.batteryMilliVolts*/ 0x63 - oscillation; // who cares
			c2015.mode = 0;
			c2015.mode += (ctl.control.enabled ? 4 : 0); // sets the 3rd bit to the value of the 3rd bit in ctl.control
			c2015.mode += (ctl.control.test ? 1 : 0); // sets 1st bit
			c2015.mode += (ctl.control.autonomous ? 2 : 0); // sets 2nd bit
			c2015.state = 0x30; // TODO change
			memcpy(&sendBuffer, &c2015, sizeof(c2015));

			//printf("%s\n", ctl.control.enabled ? "true" : "false");
		}

		uint32_t pos = 0x21;
		for (int i = 0; i<kEmbeddedCount; i++){
			uint32_t slen = htonl(embeddedDynamicChunks[i].dynamicLen * (embeddedDynamicChunks[i].dynamicData != NULL ? 1 : 0));
			memcpy(&sendBuffer[pos], &slen, sizeof(slen));
			slen = ntohl(slen);
			if (slen > 0) {
				memcpy(&sendBuffer[pos + sizeof(slen)], embeddedDynamicChunks[i].dynamicData, slen);
				delete embeddedDynamicChunks[i].dynamicData;
				embeddedDynamicChunks[i].dynamicData = NULL;
				embeddedDynamicChunks[i].dynamicLen = 0;
			}
			pos += sizeof(slen) + slen;
		}
		writingSem.give();
		uint32_t crc = crc32buf(sendBuffer, 0x400);
		crc = htonl(crc);
		memcpy(&sendBuffer[0x3fc], &crc, sizeof(uint32_t));
		sendto(dsSocket,(const char *) &sendBuffer, 0x07, 0,(const sockaddr*)&dsAddress, sizeof(dsAddress));
	}

  return NULL;
}

extern "C" {
int32_t HAL_SetErrorData(const char* errors, int32_t errorsLength,
                         int32_t waitMs) {
  return setErrorData(errors, errorsLength, waitMs);
}

int32_t HAL_SendError(HAL_Bool isError, int32_t errorCode, HAL_Bool isLVCode,
                      const char* details, const char* location,
                      const char* callStack, HAL_Bool printMsg) {
  // Avoid flooding console by keeping track of previous 5 error
  // messages and only printing again if they're longer than 1 second old.
  static constexpr int KEEP_MSGS = 5;
  std::lock_guard<hal::priority_mutex> lock(msgMutex);
  static std::string prevMsg[KEEP_MSGS];
  static std::chrono::time_point<std::chrono::steady_clock>
      prevMsgTime[KEEP_MSGS];
  static bool initialized = false;
  if (!initialized) {
    for (int i = 0; i < KEEP_MSGS; i++) {
      prevMsgTime[i] =
          std::chrono::steady_clock::now() - std::chrono::seconds(2);
    }
    initialized = true;
  }

  auto curTime = std::chrono::steady_clock::now();
  int i;
  for (i = 0; i < KEEP_MSGS; ++i) {
    if (prevMsg[i] == details) break;
  }
  int retval = 0;
  if (i == KEEP_MSGS || (curTime - prevMsgTime[i]) >= std::chrono::seconds(1)) {
    retval = FRC_NetworkCommunication_sendError(isError, errorCode, isLVCode,
                                                details, location, callStack);
    if (printMsg) {
      if (location && location[0] != '\0') {
        std::fprintf(stderr, "%s at %s: ", isError ? "Error" : "Warning",
                     location);
      }
      std::fprintf(stderr, "%s\n", details);
      if (callStack && callStack[0] != '\0') {
        std::fprintf(stderr, "%s\n", callStack);
      }
    }
    if (i == KEEP_MSGS) {
      // replace the oldest one
      i = 0;
      auto first = prevMsgTime[0];
      for (int j = 1; j < KEEP_MSGS; ++j) {
        if (prevMsgTime[j] < first) {
          first = prevMsgTime[j];
          i = j;
        }
      }
      prevMsg[i] = details;
    }
    prevMsgTime[i] = curTime;
  }
  return retval;
}

int32_t HAL_GetControlWord(HAL_ControlWord* controlWord) {
  std::memset(controlWord, 0, sizeof(HAL_ControlWord));
  return FRC_NetworkCommunication_getControlWord(
      reinterpret_cast<ControlWord_t*>(controlWord));
}

HAL_AllianceStationID HAL_GetAllianceStation(int32_t* status) {
  HAL_AllianceStationID allianceStation;
  *status = FRC_NetworkCommunication_getAllianceStation(
      reinterpret_cast<AllianceStationID_t*>(&allianceStation));
  return allianceStation;
}

int32_t HAL_GetJoystickAxes(int32_t joystickNum, HAL_JoystickAxes* axes) {
  HAL_JoystickAxesInt axesInt;

  int retVal = FRC_NetworkCommunication_getJoystickAxes(
      joystickNum, reinterpret_cast<JoystickAxes_t*>(&axesInt),
      HAL_kMaxJoystickAxes);

  // copy integer values to double values
  axes->count = axesInt.count;
  // current scaling is -128 to 127, can easily be patched in the future by
  // changing this function.
  for (int32_t i = 0; i < axesInt.count; i++) {
    int8_t value = axesInt.axes[i];
    if (value < 0) {
      axes->axes[i] = value / 128.0;
    } else {
      axes->axes[i] = value / 127.0;
    }
  }

  return retVal;
}

int32_t HAL_GetJoystickPOVs(int32_t joystickNum, HAL_JoystickPOVs* povs) {
  return FRC_NetworkCommunication_getJoystickPOVs(
      joystickNum, reinterpret_cast<JoystickPOV_t*>(povs),
      HAL_kMaxJoystickPOVs);
}

int32_t HAL_GetJoystickButtons(int32_t joystickNum,
                               HAL_JoystickButtons* buttons) {
  return FRC_NetworkCommunication_getJoystickButtons(
      joystickNum, &buttons->buttons, &buttons->count);
}
/**
 * Retrieve the Joystick Descriptor for particular slot
 * @param desc [out] descriptor (data transfer object) to fill in.  desc is
 * filled in regardless of success. In other words, if descriptor is not
 * available, desc is filled in with default values matching the init-values in
 * Java and C++ Driverstation for when caller requests a too-large joystick
 * index.
 *
 * @return error code reported from Network Comm back-end.  Zero is good,
 * nonzero is bad.
 */
int32_t HAL_GetJoystickDescriptor(int32_t joystickNum,
                                  HAL_JoystickDescriptor* desc) {
  desc->isXbox = 0;
  desc->type = std::numeric_limits<uint8_t>::max();
  desc->name[0] = '\0';
  desc->axisCount =
      HAL_kMaxJoystickAxes; /* set to the desc->axisTypes's capacity */
  desc->buttonCount = 0;
  desc->povCount = 0;
  int retval = FRC_NetworkCommunication_getJoystickDesc(
      joystickNum, &desc->isXbox, &desc->type,
      reinterpret_cast<char*>(&desc->name), &desc->axisCount,
      reinterpret_cast<uint8_t*>(&desc->axisTypes), &desc->buttonCount,
      &desc->povCount);
  /* check the return, if there is an error and the RIOimage predates FRC2017,
   * then axisCount needs to be cleared */
  if (retval != 0) {
    /* set count to zero so downstream code doesn't decode invalid axisTypes. */
    desc->axisCount = 0;
  }
  return retval;
}

HAL_Bool HAL_GetJoystickIsXbox(int32_t joystickNum) {
  HAL_JoystickDescriptor joystickDesc;
  if (HAL_GetJoystickDescriptor(joystickNum, &joystickDesc) < 0) {
    return 0;
  } else {
    return joystickDesc.isXbox;
  }
}

int32_t HAL_GetJoystickType(int32_t joystickNum) {
  HAL_JoystickDescriptor joystickDesc;
  if (HAL_GetJoystickDescriptor(joystickNum, &joystickDesc) < 0) {
    return -1;
  } else {
    return joystickDesc.type;
  }
}

char* HAL_GetJoystickName(int32_t joystickNum) {
  HAL_JoystickDescriptor joystickDesc;
  if (HAL_GetJoystickDescriptor(joystickNum, &joystickDesc) < 0) {
    char* name = static_cast<char*>(std::malloc(1));
    name[0] = '\0';
    return name;
  } else {
    size_t len = std::strlen(joystickDesc.name);
    char* name = static_cast<char*>(std::malloc(len + 1));
    std::strncpy(name, joystickDesc.name, len);
    name[len] = '\0';
    return name;
  }
}

int32_t HAL_GetJoystickAxisType(int32_t joystickNum, int32_t axis) {
  HAL_JoystickDescriptor joystickDesc;
  if (HAL_GetJoystickDescriptor(joystickNum, &joystickDesc) < 0) {
    return -1;
  } else {
    return joystickDesc.axisTypes[axis];
  }
}

int32_t HAL_SetJoystickOutputs(int32_t joystickNum, int64_t outputs,
                               int32_t leftRumble, int32_t rightRumble) {
  return FRC_NetworkCommunication_setJoystickOutputs(joystickNum, outputs,
                                                     leftRumble, rightRumble);
}

double HAL_GetMatchTime(int32_t* status) {
  float matchTime;
  *status = FRC_NetworkCommunication_getMatchTime(&matchTime);
  return matchTime;
}

void HAL_ObserveUserProgramStarting(void) {
  FRC_NetworkCommunication_observeUserProgramStarting();
}

void HAL_ObserveUserProgramDisabled(void) {
  FRC_NetworkCommunication_observeUserProgramDisabled();
}

void HAL_ObserveUserProgramAutonomous(void) {
  FRC_NetworkCommunication_observeUserProgramAutonomous();
}

void HAL_ObserveUserProgramTeleop(void) {
  FRC_NetworkCommunication_observeUserProgramTeleop();
}

void HAL_ObserveUserProgramTest(void) {
  FRC_NetworkCommunication_observeUserProgramTest();
}

bool HAL_IsNewControlData(void) {
  // There is a rollover error condition here. At Packet# = n * (uintmax), this
  // will return false when instead it should return true. However, this at a
  // 20ms rate occurs once every 2.7 years of DS connected runtime, so not
  // worth the cycles to check.
  thread_local int lastCount{-1};
  int currentCount = 0;
  {
    std::unique_lock<hal::priority_mutex> lock(newDSDataAvailableMutex);
    currentCount = newDSDataAvailableCounter;
  }
  if (lastCount == currentCount) return false;
  lastCount = currentCount;
  return true;
}

/**
 * Waits for the newest DS packet to arrive. Note that this is a blocking call.
 */
void HAL_WaitForDSData(void) { HAL_WaitForDSDataTimeout(0); }

/**
 * Waits for the newest DS packet to arrive. If timeout is <= 0, this will wait
 * forever. Otherwise, it will wait until either a new packet, or the timeout
 * time has passed. Returns true on new data, false on timeout.
 */
HAL_Bool HAL_WaitForDSDataTimeout(double timeout) {
  auto timeoutTime =
      std::chrono::steady_clock::now() + std::chrono::duration<double>(timeout);

  std::unique_lock<hal::priority_mutex> lock(newDSDataAvailableMutex);
  int currentCount = newDSDataAvailableCounter;
  while (newDSDataAvailableCounter == currentCount) {
    if (timeout > 0) {
      auto timedOut = newDSDataAvailableCond.wait_until(lock, timeoutTime);
      if (timedOut == std::cv_status::timeout) {
        return false;
      }
    } else {
      newDSDataAvailableCond.wait(lock);
    }
  }
  return true;
}

// Internal NetComm function to set new packet callback
extern int NetCommRPCProxy_SetOccurFuncPointer(
    int32_t (*occurFunc)(uint32_t refNum));

// Constant number to be used for our occur handle
constexpr int32_t refNumber = 42;

static int32_t newDataOccur(uint32_t refNum) {
  // Since we could get other values, require our specific handle
  // to signal our threads
  if (refNum != refNumber) return 0;
  std::lock_guard<hal::priority_mutex> lock(newDSDataAvailableMutex);
  // Nofify all threads
  newDSDataAvailableCounter++;
  newDSDataAvailableCond.notify_all();
  return 0;
}

/*
 * Call this to initialize the driver station communication. This will properly
 * handle multiple calls. However note that this CANNOT be called from a library
 * that interfaces with LabVIEW.
 */
void HAL_InitializeDriverStation(void) {
  static std::atomic_bool initialized{false};
  static hal::priority_mutex initializeMutex;
  // Initial check, as if it's true initialization has finished
  if (initialized) return;

  std::lock_guard<hal::priority_mutex> lock(initializeMutex);
  // Second check in case another thread was waiting
  if (initialized) return;

  pthread_create(&thread, NULL, DriverStationThread, NULL);

  initialized = true;
}

/*
 * Releases the DS Mutex to allow proper shutdown of any threads that are
 * waiting on it.
 */
void HAL_ReleaseDSMutex(void) { newDataOccur(refNumber); }

}  // extern "C"
