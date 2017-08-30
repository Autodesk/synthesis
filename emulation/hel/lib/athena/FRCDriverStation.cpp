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

#include <thread>
#include <winsock2.h>
#include <sys/types.h>
//#include <netinet/in.h>
//#include <semaphore.h>
//#include <unistd.h>

#include "FRC_NetworkCommunication/FRCComm.h"
#include "HAL/DriverStation.h"
#include "HAL/cpp/priority_condition_variable.h"
#include "HAL/cpp/priority_mutex.h"
#include "DriverStationInternal.h"
#include "crc32.h"

#undef min
#undef max

static_assert(sizeof(int32_t) >= sizeof(int),
              "FRC_NetworkComm status variable is larger than 32 bits");

struct HAL_JoystickAxesInt {
  int16_t count;
  int16_t axes[HAL_kMaxJoystickAxes];
};

struct {
  uint32_t dynamicLen;
  const char *dynamicData;
} embeddedDynamicChunks[kEmbeddedCount];

static hal::priority_mutex msgMutex;
static hal::priority_condition_variable newDSDataAvailableCond;
static hal::priority_mutex newDSDataAvailableMutex;
static int newDSDataAvailableCounter{0};

FRCCommonControlData lastDataPacket;
DynamicControlData lastDynamicControlPacket [32];
WaitSemaphore newDataSemInternal;
WaitSemaphore* newDataSem = NULL;
ReentrantSemaphore readingSem;
ReentrantSemaphore writingSem;
ReentrantSemaphore* resyncSem = NULL;
bool enabled;
FRCRobotControl ctl;

void DriverStationThread() {
  printf("Starting driver station connection thread\n");
  struct sockaddr_in robotAddress;
  struct sockaddr_in dsAddress;
  int robotSocket;
  int dsSocket;

  char* teamIDString = getenv("TEAM_ID");
  int teamID;
  sscanf(teamIDString, "%d", &teamID);
  //generates IP addresses of the form 10.TE.AM.x
  //x is 0 here, but it gets bitwise OR'd with another number for the final
  //digit
  uint32_t network = (10 << 24) |
    (((teamID / 100) & 0xFF) << 16) |
    ((teamID % 100) << 8) | 0;

  //address to bind to (this should be the loopback adapter)
  robotAddress.sin_family = AF_INET;
  robotAddress.sin_addr.s_addr = htonl(network | 2);
  //port 1110
  robotAddress.sin_port = htons(1110);

  //address to send driver station messages to
  dsAddress.sin_family = AF_INET;
  //starts out at 0.0.0.0, but gets updated after
  //the first driver station message
  dsAddress.sin_addr.s_addr = htonl(0);
  //port 1150
  dsAddress.sin_port = htons(1150);

  //initializes the loopback adapter socket
  robotSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
  if (robotSocket < 0) {
    fprintf(stderr, "Could not create socket ROBOT!\n");
    return;
  }

  //binds the loopback adapter socket to the "robot" ip address
  if (bind(robotSocket,
	(const struct sockaddr *)&robotAddress,
	sizeof(robotAddress)) != 0)
  {
    fprintf(stderr, "Could not bind socket ROBOT!\n");
    fprintf(stderr, "Did you configure your loopback adapters?\n");
    perror("bind");
    return;
  }

  //initializes the driver station socket
  dsSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
  if (dsSocket < 0) {
    fprintf(stderr, "Could not create socket DS!\n");
    fprintf(stderr, "Did you configure your loopback adapters?\n");
    return;
  }

  //buffer used for reading
  char buffer[1024];
  bool lockedResync = false;

  // Read from the DS thread
  while (true) {
    if (resyncSem != NULL && !lockedResync) {
      resyncSem->take();
      lockedResync = true;
    }

    //recvfrom is used instead of recv to get the ip address that is sending
    //the most recent message. this is used to handle situations in which the
    //driver station binds to something weird (like the loopback adapter)
    int srcaddrSize = sizeof(sockaddr_in);
    int len = recvfrom(robotSocket,
	(char*) &buffer,
	sizeof(buffer),
	0,
	(sockaddr*) &dsAddress,
	&srcaddrSize);

    if (len < 0) {
      printf("Read failed\n");
    }
    readingSem.take();

    // Convert 2015 packets to 2014 (TODO find a better way)
    {
      FRCCommonControlData p2014;
      memset(&p2014, 0, sizeof(p2014));
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
	  fprintf(stderr, "Unimplemented alliance station state\n");
	  return;
      }

      p2014.dsID_Alliance = alliance;
      p2014.dsID_Position = position;

      int joystick_idx = 0;
      size_t address = (size_t)&buffer[6];

      while(address < (size_t)&buffer + len) {
	uint8_t size = *(uint8_t*)address;
	size_t old_address = address;
	address += 2; //skip the next byte

	uint8_t num_axes = *(uint8_t*)address;
	p2014.joysticks[joystick_idx].num_axes = num_axes;
	address += 1;
	for(int axis = 0; axis < num_axes; axis++) {
	  p2014.joysticks[joystick_idx].axes[axis] = *(int8_t*)address;
	  address += 1;
	}

	uint8_t num_buttons = *(uint8_t*)address;
	p2014.joysticks[joystick_idx].num_buttons = num_buttons;
	address += 1;
	p2014.joysticks[joystick_idx].buttons = ntohs(*(uint16_t*)address);
	address += 2;

	address = old_address + size + 1;

	p2014.num_joysticks++;
	joystick_idx++;
      }

      p2014.enabled = p2015.state & 4 ? true : false;
      p2014.autonomous = p2015.state & 2 ? true : false;
      p2014.test = p2015.state & 1 ? true : false;
      ctl.control.enabled = p2014.enabled;
      ctl.control.autonomous = p2014.autonomous;
      ctl.control.test = p2014.test;

      memcpy(&lastDataPacket, &p2014, sizeof(p2014));
    }

    {
      // Handle endians
      lastDataPacket.packetIndex = ntohs(lastDataPacket.packetIndex);
      lastDataPacket.teamID = ntohs(lastDataPacket.teamID);
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

    // Convert 2014 packets to 2015
    {
      FRCRobotControl2015 c2015;
      c2015.packetIndex = ctl.packetIndex++;
      c2015.voltage_greater = 12;
      int oscillation = (ctl.control.enabled ? 0 : (rand() % 2));
      c2015.voltage_lesser = 0x63 - oscillation;
      c2015.mode = 0;
      // sets the 3rd bit to the value of the 3rd bit in ctl.control
      c2015.mode += (ctl.control.enabled ? 4 : 0);
      c2015.mode += (ctl.control.test ? 1 : 0); // sets 1st bit
      c2015.mode += (ctl.control.autonomous ? 2 : 0); // sets 2nd bit
      c2015.state = 0x30;
      memcpy(&sendBuffer, &c2015, sizeof(c2015));
    }

    uint32_t pos = 0x21;
    for (int i = 0; i<kEmbeddedCount; i++){
      uint32_t slen = htonl(embeddedDynamicChunks[i].dynamicLen *
	  (embeddedDynamicChunks[i].dynamicData != NULL ? 1 : 0));
      memcpy(&sendBuffer[pos], &slen, sizeof(slen));
      slen = ntohl(slen);
      if (slen > 0) {
	memcpy(&sendBuffer[pos + sizeof(slen)],
	    embeddedDynamicChunks[i].dynamicData, slen);
	delete embeddedDynamicChunks[i].dynamicData;
	embeddedDynamicChunks[i].dynamicData = NULL;
	embeddedDynamicChunks[i].dynamicLen = 0;
      }
      pos += sizeof(slen) + slen;
    }
    writingSem.give();
    //calculates the CRC sum
    uint32_t crc = crc32buf(sendBuffer, 0x400);
    crc = htonl(crc);
    memcpy(&sendBuffer[0x3fc], &crc, sizeof(uint32_t));
    dsAddress.sin_port = htons( 1150 );
    sendto(dsSocket,(const char *) &sendBuffer,
	0x07, 0,(const sockaddr*)&dsAddress, srcaddrSize);
  }
}

extern "C" {
int32_t HAL_SetErrorData(const char* errors, int32_t errorsLength,
    int32_t waitMs) {
  return 0;
}

int32_t HAL_SendError(HAL_Bool isError, int32_t errorCode, HAL_Bool isLVCode,
    const char* details, const char* location,
    const char* callStack, HAL_Bool printMsg) {
  return 0;
}

int32_t HAL_GetControlWord(HAL_ControlWord* controlWord) {
  //this should just copy stuff over from the real control word
  //unfortunately, the network representation and the internal representation
  //are not the same, so this is a little ugly
  std::memset(controlWord, 0, sizeof(HAL_ControlWord));
  controlWord->enabled = ctl.control.enabled;
  controlWord->autonomous = ctl.control.autonomous;
  controlWord->test = ctl.control.test;
  controlWord->fmsAttached = ctl.control.fmsAttached;
  controlWord->dsAttached = true;
  controlWord->eStop = !ctl.control.notEStop;
  return 0;
}

HAL_AllianceStationID HAL_GetAllianceStation(int32_t* status) {
  return HAL_AllianceStationID_kRed1;
}

void HAL_FreeJoystickName(char* name) {
  std::free(name);
}

int32_t HAL_GetJoystickAxes(int32_t joystickNum, HAL_JoystickAxes* axes) {
  //copyh over joystic axis info
  HAL_JoystickAxesInt axesInt;

  axesInt.count = lastDataPacket.joysticks[joystickNum].num_axes;
  for(int i = 0; i < axesInt.count; i++) {
    axesInt.axes[i] = lastDataPacket.joysticks[joystickNum].axes[i];
  }

  // copy integer values to double values
  axes->count = axesInt.count;
  //current scaling is -128 to 127, can easily be patched in the future by
  //changing this function.
  for (int32_t i = 0; i < axesInt.count; i++) {
    int8_t value = axesInt.axes[i];
    if (value < 0) {
      axes->axes[i] = value / 128.0;
    } else {
      axes->axes[i] = value / 127.0;
    }
  }
  return 0;
}

int32_t HAL_GetJoystickPOVs(int32_t joystickNum, HAL_JoystickPOVs* povs) {
  //we don't support POV?
  //TODO: figure out a solution to this
  povs->count = 0;
  return 0;
}

int32_t HAL_GetJoystickButtons(int32_t joystickNum,
                               HAL_JoystickButtons* buttons) {
  //copy over button info
  buttons->count = lastDataPacket.joysticks[joystickNum].num_buttons;
  buttons->buttons = lastDataPacket.joysticks[joystickNum].buttons;
  return 0;
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
  //we don't know how to tell if it's xbox
  desc->isXbox = 0;
  //type doesn't matter
  desc->type = std::numeric_limits<uint8_t>::max();
  //name doesn't matter
  desc->name[0] = '\0';
  //this stuff matters
  desc->axisCount = lastDataPacket.joysticks[joystickNum].num_axes;
  desc->buttonCount = lastDataPacket.joysticks[joystickNum].num_buttons;
  desc->povCount = 0;
  return 0;
}

HAL_Bool HAL_GetJoystickIsXbox(int32_t joystickNum) {
  //since we don't know if it's xbox, this will always return 0, but if we
  //fix the isXbox stuff in HAL_GetJoystickDescriptor, this will work correctly
  HAL_JoystickDescriptor joystickDesc;
  if (HAL_GetJoystickDescriptor(joystickNum, &joystickDesc) < 0) {
    return 0;
  } else {
    return joystickDesc.isXbox;
  }
}

int32_t HAL_GetJoystickType(int32_t joystickNum) {
  return 0;
}

char* HAL_GetJoystickName(int32_t joystickNum) {
  return 0;
}

int32_t HAL_GetJoystickAxisType(int32_t joystickNum, int32_t axis) {
  return 0;
}

int32_t HAL_SetJoystickOutputs(int32_t joystickNum, int64_t outputs,
                               int32_t leftRumble, int32_t rightRumble) {
  return 0;
}

double HAL_GetMatchTime(int32_t* status) {
  return 0.0;
}

void HAL_ObserveUserProgramStarting(void) {}

void HAL_ObserveUserProgramDisabled(void) {}

void HAL_ObserveUserProgramAutonomous(void) {}

void HAL_ObserveUserProgramTeleop(void) {}

void HAL_ObserveUserProgramTest(void) {}

bool HAL_IsNewControlData(void) {
  return false;
}

/**
 * Waits for the newest DS packet to arrive. Note that this is a blocking call.
 */
void HAL_WaitForDSData(void) { /*HAL_WaitForDSDataTimeout(0);*/ }

/**
 * Waits for the newest DS packet to arrive. If timeout is <= 0, this will wait
 * forever. Otherwise, it will wait until either a new packet, or the timeout
 * time has passed. Returns true on new data, false on timeout.
 */
HAL_Bool HAL_WaitForDSDataTimeout(double timeout) {
  return true;
}

// Internal NetComm function to set new packet callback
extern int NetCommRPCProxy_SetOccurFuncPointer(
    int32_t (*occurFunc)(uint32_t refNum));

// Constant number to be used for our occur handle
constexpr int32_t refNumber = 42;

static int32_t newDataOccur(uint32_t refNum) {
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

  std::thread(DriverStationThread).detach();

  initialized = true;
}

/*
 * Releases the DS Mutex to allow proper shutdown of any threads that are
 * waiting on it.
 */
void HAL_ReleaseDSMutex(void) { newDataOccur(refNumber); }

}  // extern "C"
