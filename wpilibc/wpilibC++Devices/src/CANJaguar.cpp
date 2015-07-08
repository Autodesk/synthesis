/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2009. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "CANJaguar.h"
#include "Timer.h"
#define tNIRIO_i32 int
#include "NetworkCommunication/CANSessionMux.h"
#include "CAN/can_proto.h"
//#include "NetworkCommunication/UsageReporting.h"
#include "WPIErrors.h"
#include <cstdio>
#include <cassert>
#include "LiveWindow/LiveWindow.h"

/* we are on ARM-LE now, not Freescale so no need to swap */
#define swap16(x)	(x)
#define swap32(x)	(x)

/* Compare floats for equality as fixed point numbers */
#define FXP8_EQ(a,b) ((int16_t)((a)*256.0)==(int16_t)((b)*256.0))
#define FXP16_EQ(a,b) ((int32_t)((a)*65536.0)==(int32_t)((b)*65536.0))

const int32_t CANJaguar::kControllerRate;
constexpr double CANJaguar::kApproxBusVoltage;

static const int32_t kSendMessagePeriod = 20;
static const uint32_t kFullMessageIDMask = (CAN_MSGID_API_M | CAN_MSGID_MFR_M | CAN_MSGID_DTYPE_M);

static const int32_t kReceiveStatusAttempts = 50;

static Resource *allocated = NULL;

static int32_t sendMessageHelper(uint32_t messageID, const uint8_t *data, uint8_t dataSize, int32_t period)
{
	static const uint32_t kTrustedMessages[] = {
			LM_API_VOLT_T_EN, LM_API_VOLT_T_SET, LM_API_SPD_T_EN, LM_API_SPD_T_SET,
			LM_API_VCOMP_T_EN, LM_API_VCOMP_T_SET, LM_API_POS_T_EN, LM_API_POS_T_SET,
			LM_API_ICTRL_T_EN, LM_API_ICTRL_T_SET};

	int32_t status=0;

	for (uint8_t i=0; i<(sizeof(kTrustedMessages)/sizeof(kTrustedMessages[0])); i++)
	{
		if ((kFullMessageIDMask & messageID) == kTrustedMessages[i])
		{
			uint8_t dataBuffer[8];
			dataBuffer[0] = 0;
			dataBuffer[1] = 0;

			// Make sure the data will still fit after adjusting for the token.
			assert(dataSize <= 6);

			for (uint8_t j=0; j < dataSize; j++)
			{
				dataBuffer[j + 2] = data[j];
			}

			FRC_NetworkCommunication_CANSessionMux_sendMessage(messageID, dataBuffer, dataSize + 2, period, &status);

			return status;
		}
	}

	FRC_NetworkCommunication_CANSessionMux_sendMessage(messageID, data, dataSize, period, &status);

	return status;
}

/**
 * Common initialization code called by all constructors.
 */
void CANJaguar::InitCANJaguar()
{
	m_table = NULL;
	m_safetyHelper = new MotorSafetyHelper(this);

	m_value = 0.0f;
	m_speedReference = LM_REF_NONE;
	m_positionReference = LM_REF_NONE;
	m_p = 0.0;
	m_i = 0.0;
	m_d = 0.0;
	m_busVoltage = 0.0f;
	m_outputVoltage = 0.0f;
	m_outputCurrent = 0.0f;
	m_temperature = 0.0f;
	m_position = 0.0;
	m_speed = 0.0;
	m_limits = 0x00;
	m_faults = 0x0000;
	m_firmwareVersion = 0;
	m_hardwareVersion = 0;
	m_neutralMode = kNeutralMode_Jumper;
	m_encoderCodesPerRev = 0;
	m_potentiometerTurns = 0;
	m_limitMode = kLimitMode_SwitchInputsOnly;
	m_forwardLimit = 0.0;
	m_reverseLimit = 0.0;
	m_maxOutputVoltage = 30.0;
	m_voltageRampRate = 0.0;
	m_faultTime = 0.0f;

	// Parameters only need to be verified if they are set
	m_controlModeVerified = false; // Needs to be verified because it's set in the constructor
	m_speedRefVerified = true;
	m_posRefVerified = true;
	m_pVerified = true;
	m_iVerified = true;
	m_dVerified = true;
	m_neutralModeVerified = true;
	m_encoderCodesPerRevVerified = true;
	m_potentiometerTurnsVerified = true;
	m_limitModeVerified = true;
	m_forwardLimitVerified = true;
	m_reverseLimitVerified = true;
	m_maxOutputVoltageVerified = true;
	m_voltageRampRateVerified = true;
	m_faultTimeVerified = true;

	m_receivedStatusMessage0 = false;
	m_receivedStatusMessage1 = false;
	m_receivedStatusMessage2 = false;

	bool receivedFirmwareVersion = false;
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	// Request firmware and hardware version only once
	requestMessage(CAN_IS_FRAME_REMOTE | CAN_MSGID_API_FIRMVER);
	requestMessage(LM_API_HWVER);

	// Wait until we've gotten all of the status data at least once.
	for(int i = 0; i < kReceiveStatusAttempts; i++)
	{
		Wait(0.001);

		setupPeriodicStatus();
		updatePeriodicStatus();

		if(!receivedFirmwareVersion &&
		   getMessage(CAN_MSGID_API_FIRMVER, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			m_firmwareVersion = unpackint32_t(dataBuffer);
			receivedFirmwareVersion = true;
		}

		if(m_receivedStatusMessage0 &&
		   m_receivedStatusMessage1 &&
		   m_receivedStatusMessage2 &&
		   receivedFirmwareVersion)
		{
			break;
		}
	}

	if(!m_receivedStatusMessage0 ||
	   !m_receivedStatusMessage1 ||
	   !m_receivedStatusMessage2 ||
	   !receivedFirmwareVersion)
	{
		wpi_setWPIErrorWithContext(JaguarMessageNotFound, "Status data not found");
	}

	if(getMessage(LM_API_HWVER, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		m_hardwareVersion = dataBuffer[0];

	if (m_deviceNumber < 1 || m_deviceNumber > 63)
	{
		char buf[256];
		snprintf(buf, 256, "device number \"%d\" must be between 1 and 63", m_deviceNumber);
		wpi_setWPIErrorWithContext(ParameterOutOfRange, buf);
		return;
	}

	if (StatusIsFatal())
		return;

	// 3330 was the first shipping RDK firmware version for the Jaguar
	if (m_firmwareVersion >= 3330 || m_firmwareVersion < 108)
	{
		char buf[256];
		if (m_firmwareVersion < 3330)
		{
			snprintf(buf, 256, "Jag #%d firmware (%d) is too old (must be at least version 108 of the FIRST approved firmware)", m_deviceNumber, m_firmwareVersion);
		}
		else
		{
			snprintf(buf, 256, "Jag #%d firmware (%d) is not FIRST approved (must be at least version 108 of the FIRST approved firmware)", m_deviceNumber, m_firmwareVersion);
		}
		wpi_setWPIErrorWithContext(JaguarVersionError, buf);
		return;
	}

	switch (m_controlMode)
	{
	case kPercentVbus:
	case kVoltage:
		// No additional configuration required... start enabled.
		EnableControl();
		break;
	default:
		break;
	}

	HALReport(HALUsageReporting::kResourceType_CANJaguar, m_deviceNumber, m_controlMode);
	LiveWindow::GetInstance()->AddActuator("CANJaguar", m_deviceNumber, this);
}

/**
 * Constructor for the CANJaguar device.<br>
 * By default the device is configured in Percent mode.
 * The control mode can be changed by calling one of the control modes listed below.
 *
 * @param deviceNumber The address of the Jaguar on the CAN bus.
 * @see CANJaguar#SetCurrentMode(double, double, double)
 * @see CANJaguar#SetCurrentMode(PotentiometerTag, double, double, double)
 * @see CANJaguar#SetCurrentMode(EncoderTag, int, double, double, double)
 * @see CANJaguar#SetCurrentMode(QuadEncoderTag, int, double, double, double)
 * @see CANJaguar#SetPercentMode()
 * @see CANJaguar#SetPercentMode(PotentiometerTag)
 * @see CANJaguar#SetPercentMode(EncoderTag, int)
 * @see CANJaguar#SetPercentMode(QuadEncoderTag, int)
 * @see CANJaguar#SetPositionMode(PotentiometerTag, double, double, double)
 * @see CANJaguar#SetPositionMode(QuadEncoderTag, int, double, double, double)
 * @see CANJaguar#SetSpeedMode(EncoderTag, int, double, double, double)
 * @see CANJaguar#SetSpeedMode(QuadEncoderTag, int, double, double, double)
 * @see CANJaguar#SetVoltageMode()
 * @see CANJaguar#SetVoltageMode(PotentiometerTag)
 * @see CANJaguar#SetVoltageMode(EncoderTag, int)
 * @see CANJaguar#SetVoltageMode(QuadEncoderTag, int)
 */
CANJaguar::CANJaguar(uint8_t deviceNumber)
	: m_deviceNumber (deviceNumber)
	, m_maxOutputVoltage (kApproxBusVoltage)
	, m_safetyHelper (NULL)
{
	char buf[64];
	snprintf(buf, 64, "CANJaguar device number %d", m_deviceNumber);
	Resource::CreateResourceObject(&allocated, 63);

	if (allocated->Allocate(m_deviceNumber-1, buf) == ~0ul)
	{
		CloneError(allocated);
		return;
	}

	SetPercentMode();
	InitCANJaguar();
	ConfigMaxOutputVoltage(kApproxBusVoltage);
}

CANJaguar::~CANJaguar()
{
	allocated->Free(m_deviceNumber-1);

	int32_t status;

	// Disable periodic setpoints
	if(m_controlMode == kPercentVbus)
		FRC_NetworkCommunication_CANSessionMux_sendMessage(m_deviceNumber | LM_API_VOLT_T_SET, NULL, 0, CAN_SEND_PERIOD_STOP_REPEATING, &status);
	else if(m_controlMode == kSpeed)
		FRC_NetworkCommunication_CANSessionMux_sendMessage(m_deviceNumber | LM_API_SPD_T_SET, NULL, 0, CAN_SEND_PERIOD_STOP_REPEATING, &status);
	else if(m_controlMode == kPosition)
		FRC_NetworkCommunication_CANSessionMux_sendMessage(m_deviceNumber | LM_API_POS_T_SET, NULL, 0, CAN_SEND_PERIOD_STOP_REPEATING, &status);
	else if(m_controlMode == kCurrent)
		FRC_NetworkCommunication_CANSessionMux_sendMessage(m_deviceNumber | LM_API_ICTRL_T_SET, NULL, 0, CAN_SEND_PERIOD_STOP_REPEATING, &status);
	else if(m_controlMode == kVoltage)
		FRC_NetworkCommunication_CANSessionMux_sendMessage(m_deviceNumber | LM_API_VCOMP_T_SET, NULL, 0, CAN_SEND_PERIOD_STOP_REPEATING, &status);

	delete m_safetyHelper;
	m_safetyHelper = NULL;
}

/**
 * @return The CAN ID passed in the constructor
 */
uint8_t CANJaguar::getDeviceNumber() const
{
	return m_deviceNumber;
}

/**
 * Sets the output set-point value.
 *
 * The scale and the units depend on the mode the Jaguar is in.<br>
 * In percentVbus Mode, the outputValue is from -1.0 to 1.0 (same as PWM Jaguar).<br>
 * In voltage Mode, the outputValue is in volts. <br>
 * In current Mode, the outputValue is in amps. <br>
 * In speed Mode, the outputValue is in rotations/minute.<br>
 * In position Mode, the outputValue is in rotations.
 *
 * @param outputValue The set-point to sent to the motor controller.
 * @param syncGroup The update group to add this Set() to, pending UpdateSyncGroup().  If 0, update immediately.
 */
void CANJaguar::Set(float outputValue, uint8_t syncGroup)
{
	uint32_t messageID;
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	if(m_safetyHelper && !m_safetyHelper->IsAlive() && m_controlEnabled)
	{
		EnableControl();
	}

	if(m_controlEnabled)
	{
		switch(m_controlMode)
		{
		case kPercentVbus:
			{
				messageID = LM_API_VOLT_T_SET;
				if (outputValue > 1.0) outputValue = 1.0;
				if (outputValue < -1.0) outputValue = -1.0;
				dataSize = packPercentage(dataBuffer, outputValue);
			}
			break;
		case kSpeed:
			{
				messageID = LM_API_SPD_T_SET;
				dataSize = packFXP16_16(dataBuffer, outputValue);
			}
			break;
		case kPosition:
			{
				messageID = LM_API_POS_T_SET;
				dataSize = packFXP16_16(dataBuffer, outputValue);
			}
			break;
		case kCurrent:
			{
				messageID = LM_API_ICTRL_T_SET;
				dataSize = packFXP8_8(dataBuffer, outputValue);
			}
			break;
		case kVoltage:
			{
				messageID = LM_API_VCOMP_T_SET;
				dataSize = packFXP8_8(dataBuffer, outputValue);
			}
			break;
    default:
      wpi_setWPIErrorWithContext(IncompatibleMode, "The Jaguar only supports Current, Voltage, Position, Speed, and Percent (Throttle) modes.");
      return;
		}
		if (syncGroup != 0)
		{
			dataBuffer[dataSize] = syncGroup;
			dataSize++;
		}

		sendMessage(messageID, dataBuffer, dataSize, kSendMessagePeriod);

		if (m_safetyHelper) m_safetyHelper->Feed();
	}

	m_value = outputValue;

	verify();
}

/**
 * Get the recently set outputValue setpoint.
 *
 * The scale and the units depend on the mode the Jaguar is in.<br>
 * In percentVbus Mode, the outputValue is from -1.0 to 1.0 (same as PWM Jaguar).<br>
 * In voltage Mode, the outputValue is in volts.<br>
 * In current Mode, the outputValue is in amps.<br>
 * In speed Mode, the outputValue is in rotations/minute.<br>
 * In position Mode, the outputValue is in rotations.<br>
 *
 * @return The most recently set outputValue setpoint.
 */
float CANJaguar::Get()
{
	return m_value;
}

/**
* Common interface for disabling a motor.
*
* @deprecated Call {@link #DisableControl()} instead.
*/
void CANJaguar::Disable()
{
	DisableControl();
}

/**
 * Write out the PID value as seen in the PIDOutput base object.
 *
 * @deprecated Call Set instead.
 *
 * @param output Write out the PercentVbus value as was computed by the PIDController
 */
void CANJaguar::PIDWrite(float output)
{
	if (m_controlMode == kPercentVbus)
	{
		Set(output);
	}
	else
	{
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID only supported in PercentVbus mode");
	}
}

uint8_t CANJaguar::packPercentage(uint8_t *buffer, double value)
{
	int16_t intValue = (int16_t)(value * 32767.0);
	*((int16_t*)buffer) = swap16(intValue);
	return sizeof(int16_t);
}

uint8_t CANJaguar::packFXP8_8(uint8_t *buffer, double value)
{
	int16_t intValue = (int16_t)(value * 256.0);
	*((int16_t*)buffer) = swap16(intValue);
	return sizeof(int16_t);
}

uint8_t CANJaguar::packFXP16_16(uint8_t *buffer, double value)
{
	int32_t intValue = (int32_t)(value * 65536.0);
	*((int32_t*)buffer) = swap32(intValue);
	return sizeof(int32_t);
}

uint8_t CANJaguar::packint16_t(uint8_t *buffer, int16_t value)
{
	*((int16_t*)buffer) = swap16(value);
	return sizeof(int16_t);
}

uint8_t CANJaguar::packint32_t(uint8_t *buffer, int32_t value)
{
	*((int32_t*)buffer) = swap32(value);
	return sizeof(int32_t);
}

double CANJaguar::unpackPercentage(uint8_t *buffer)
{
	int16_t value = *((int16_t*)buffer);
	value = swap16(value);
	return value / 32767.0;
}

double CANJaguar::unpackFXP8_8(uint8_t *buffer)
{
	int16_t value = *((int16_t*)buffer);
	value = swap16(value);
	return value / 256.0;
}

double CANJaguar::unpackFXP16_16(uint8_t *buffer)
{
	int32_t value = *((int32_t*)buffer);
	value = swap32(value);
	return value / 65536.0;
}

int16_t CANJaguar::unpackint16_t(uint8_t *buffer)
{
	int16_t value = *((int16_t*)buffer);
	return swap16(value);
}

int32_t CANJaguar::unpackint32_t(uint8_t *buffer)
{
	int32_t value = *((int32_t*)buffer);
	return swap32(value);
}

/**
 * Send a message to the Jaguar.
 *
 * @param messageID The messageID to be used on the CAN bus (device number is added internally)
 * @param data The up to 8 bytes of data to be sent with the message
 * @param dataSize Specify how much of the data in "data" to send
 * @param periodic If positive, tell Network Communications to send the message
 * 	every "period" milliseconds.
 */
void CANJaguar::sendMessage(uint32_t messageID, const uint8_t *data, uint8_t dataSize, int32_t period)
{
	int32_t localStatus = sendMessageHelper(messageID | m_deviceNumber, data, dataSize, period);

	if(localStatus < 0)
	{
		wpi_setErrorWithContext(localStatus, "sendMessage");
	}
}

/**
 * Request a message from the Jaguar, but don't wait for it to arrive.
 *
 * @param messageID The message to request
 * @param periodic If positive, tell Network Communications to send the message
 * 	every "period" milliseconds.
 */
void CANJaguar::requestMessage(uint32_t messageID, int32_t period)
{
	sendMessageHelper(messageID | m_deviceNumber, NULL, 0, period);
}

/**
 * Get a previously requested message.
 *
 * Jaguar always generates a message with the same message ID when replying.
 *
 * @param messageID The messageID to read from the CAN bus (device number is added internally)
 * @param data The up to 8 bytes of data that was received with the message
 * @param dataSize Indicates how much data was received
 *
 * @return true if the message was found.  Otherwise, no new message is available.
 */
bool CANJaguar::getMessage(uint32_t messageID, uint32_t messageMask, uint8_t *data, uint8_t *dataSize)
{
	uint32_t targetedMessageID = messageID | m_deviceNumber;
	int32_t status = 0;
	uint32_t timeStamp;

	// Caller may have set bit31 for remote frame transmission so clear invalid bits[31-29]
	targetedMessageID &= CAN_MSGID_FULL_M;

	// Get the data.
	FRC_NetworkCommunication_CANSessionMux_receiveMessage(&targetedMessageID, messageMask, data, dataSize, &timeStamp, &status);

	// Do we already have the most recent value?
	if(status == ERR_CANSessionMux_MessageNotFound)
		return false;
	else
		wpi_setErrorWithContext(status, "receiveMessage");

	return true;
}

/**
 * Enables periodic status updates from the Jaguar.
 */
void CANJaguar::setupPeriodicStatus() {
	uint8_t data[8];
	uint8_t dataSize;

	// Message 0 returns bus voltage, output voltage, output current, and
	// temperature.
	static const uint8_t kMessage0Data[] = {
		LM_PSTAT_VOLTBUS_B0, LM_PSTAT_VOLTBUS_B1,
		LM_PSTAT_VOLTOUT_B0, LM_PSTAT_VOLTOUT_B1,
		LM_PSTAT_CURRENT_B0, LM_PSTAT_CURRENT_B1,
		LM_PSTAT_TEMP_B0, LM_PSTAT_TEMP_B1
	};

	// Message 1 returns position and speed
	static const uint8_t kMessage1Data[] = {
		LM_PSTAT_POS_B0, LM_PSTAT_POS_B1, LM_PSTAT_POS_B2, LM_PSTAT_POS_B3,
		LM_PSTAT_SPD_B0, LM_PSTAT_SPD_B1, LM_PSTAT_SPD_B2, LM_PSTAT_SPD_B3
	};

	// Message 2 returns limits and faults
	static const uint8_t kMessage2Data[] = {
		LM_PSTAT_LIMIT_CLR,
		LM_PSTAT_FAULT,
		LM_PSTAT_END
	};

	dataSize = packint16_t(data, kSendMessagePeriod);
	sendMessage(LM_API_PSTAT_PER_EN_S0, data, dataSize);
	sendMessage(LM_API_PSTAT_PER_EN_S1, data, dataSize);
	sendMessage(LM_API_PSTAT_PER_EN_S2, data, dataSize);

	dataSize = 8;
	sendMessage(LM_API_PSTAT_CFG_S0, kMessage0Data, dataSize);
	sendMessage(LM_API_PSTAT_CFG_S1, kMessage1Data, dataSize);
	sendMessage(LM_API_PSTAT_CFG_S2, kMessage2Data, dataSize);
}

/**
 * Check for new periodic status updates and unpack them into local variables
 */
void CANJaguar::updatePeriodicStatus() {
	uint8_t data[8];
	uint8_t dataSize;

	// Check if a new bus voltage/output voltage/current/temperature message
	// has arrived and unpack the values into the cached member variables
	if(getMessage(LM_API_PSTAT_DATA_S0, CAN_MSGID_FULL_M, data, &dataSize)) {
		m_busVoltage    = unpackFXP8_8(data);
		m_outputVoltage = unpackPercentage(data + 2) * m_busVoltage;
		m_outputCurrent = unpackFXP8_8(data + 4);
		m_temperature   = unpackFXP8_8(data + 6);

		m_receivedStatusMessage0 = true;
	}

	// Check if a new position/speed message has arrived and do the same
	if(getMessage(LM_API_PSTAT_DATA_S1, CAN_MSGID_FULL_M, data, &dataSize)) {
		m_position = unpackFXP16_16(data);
		m_speed    = unpackFXP16_16(data + 4);

		m_receivedStatusMessage1 = true;
	}

	// Check if a new limits/faults message has arrived and do the same
	if(getMessage(LM_API_PSTAT_DATA_S2, CAN_MSGID_FULL_M, data, &dataSize)) {
		m_limits = data[0];
		m_faults = data[1];

		m_receivedStatusMessage2 = true;
	}
}

/**
 * Check all unverified params and make sure they're equal to their local
 * cached versions. If a value isn't available, it gets requested.  If a value
 * doesn't match up, it gets set again.
 */
void CANJaguar::verify()
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	// If the Jaguar lost power, everything should be considered unverified.
	if(getMessage(LM_API_STATUS_POWER, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
	{
		bool powerCycled = (bool)dataBuffer[0];

		if(powerCycled)
		{
			// Clear the power cycled bit
			dataBuffer[0] = 1;
			sendMessage(LM_API_STATUS_POWER, dataBuffer, sizeof(uint8_t));

			// Mark everything as unverified
			m_controlModeVerified = false;
			m_speedRefVerified = false;
			m_posRefVerified = false;
			m_neutralModeVerified = false;
			m_encoderCodesPerRevVerified = false;
			m_potentiometerTurnsVerified = false;
			m_forwardLimitVerified = false;
			m_reverseLimitVerified = false;
			m_limitModeVerified = false;
			m_maxOutputVoltageVerified = false;
			m_faultTimeVerified = false;

			if(m_controlMode == kPercentVbus || m_controlMode == kVoltage)
			{
				m_voltageRampRateVerified = false;
			}
			else
			{
				m_pVerified = false;
				m_iVerified = false;
				m_dVerified = false;
			}

			// Verify periodic status messages again
			m_receivedStatusMessage0 = false;
			m_receivedStatusMessage1 = false;
			m_receivedStatusMessage2 = false;

			// Remove any old values from netcomms. Otherwise, parameters are
			// incorrectly marked as verified based on stale messages.
			getMessage(LM_API_SPD_REF, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_POS_REF, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_SPD_PC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_POS_PC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_ICTRL_PC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_SPD_IC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_POS_IC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_ICTRL_IC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_SPD_DC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_POS_DC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_ICTRL_DC, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_BRAKE_COAST, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_ENC_LINES, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_POT_TURNS, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_LIMIT_MODE, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_LIMIT_FWD, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_LIMIT_REV, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_MAX_VOUT, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_VOLT_SET_RAMP, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_VCOMP_COMP_RAMP, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
			getMessage(LM_API_CFG_FAULT_TIME, CAN_MSGID_FULL_M, dataBuffer, &dataSize);
		}
	}
	else
	{
		requestMessage(LM_API_STATUS_POWER);
	}

	// Verify that any recently set parameters are correct
	if(!m_controlModeVerified && m_controlEnabled)
	{
		if(getMessage(LM_API_STATUS_CMODE, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			ControlMode mode = (ControlMode)dataBuffer[0];

			if(m_controlMode == mode)
				m_controlModeVerified = true;
			else
				// Enable control again to resend the control mode
				EnableControl();
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_STATUS_CMODE);
		}
	}

	if(!m_speedRefVerified)
	{
		if(getMessage(LM_API_SPD_REF, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			uint8_t speedRef = dataBuffer[0];

			if(m_speedReference == speedRef)
				m_speedRefVerified = true;
			else
				// It's wrong - set it again
				SetSpeedReference(m_speedReference);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_SPD_REF);
		}
	}

	if(!m_posRefVerified)
	{
		if(getMessage(LM_API_POS_REF, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			uint8_t posRef = dataBuffer[0];

			if(m_positionReference == posRef)
				m_posRefVerified = true;
			else
				// It's wrong - set it again
				SetPositionReference(m_positionReference);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_POS_REF);
		}
	}

	if(!m_pVerified)
	{
		uint32_t message;

		if(m_controlMode == kSpeed)
			message = LM_API_SPD_PC;
		else if(m_controlMode == kPosition)
			message = LM_API_POS_PC;
		else if(m_controlMode == kCurrent)
			message = LM_API_ICTRL_PC;
		else {
			wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
			return;
		}

		if(getMessage(message, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double p = unpackFXP16_16(dataBuffer);

			if(FXP16_EQ(m_p, p))
				m_pVerified = true;
			else
				// It's wrong - set it again
				SetP(m_p);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(message);
		}
	}

	if(!m_iVerified)
	{
		uint32_t message;

		if(m_controlMode == kSpeed)
			message = LM_API_SPD_IC;
		else if(m_controlMode == kPosition)
			message = LM_API_POS_IC;
		else if(m_controlMode == kCurrent)
			message = LM_API_ICTRL_IC;
		else {
			wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
			return;
		}

		if(getMessage(message, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double i = unpackFXP16_16(dataBuffer);

			if(FXP16_EQ(m_i, i))
				m_iVerified = true;
			else
				// It's wrong - set it again
				SetI(m_i);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(message);
		}
	}

	if(!m_dVerified)
	{
		uint32_t message;

		if(m_controlMode == kSpeed)
			message = LM_API_SPD_DC;
		else if(m_controlMode == kPosition)
			message = LM_API_POS_DC;
		else if(m_controlMode == kCurrent)
			message = LM_API_ICTRL_DC;
    else {
		  wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
      return;
    }

		if(getMessage(message, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double d = unpackFXP16_16(dataBuffer);

			if(FXP16_EQ(m_d, d))
				m_dVerified = true;
			else
				// It's wrong - set it again
				SetD(m_d);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(message);
		}
	}

	if(!m_neutralModeVerified)
	{
		if(getMessage(LM_API_CFG_BRAKE_COAST, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			NeutralMode mode = (NeutralMode)dataBuffer[0];

			if(mode == m_neutralMode)
				m_neutralModeVerified = true;
			else
				// It's wrong - set it again
				ConfigNeutralMode(m_neutralMode);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_BRAKE_COAST);
		}
	}

	if(!m_encoderCodesPerRevVerified)
	{
		if(getMessage(LM_API_CFG_ENC_LINES, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			uint16_t codes = unpackint16_t(dataBuffer);

			if(codes == m_encoderCodesPerRev)
				m_encoderCodesPerRevVerified = true;
			else
				// It's wrong - set it again
				ConfigEncoderCodesPerRev(m_encoderCodesPerRev);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_ENC_LINES);
		}
	}

	if(!m_potentiometerTurnsVerified)
	{
		if(getMessage(LM_API_CFG_POT_TURNS, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			uint16_t turns = unpackint16_t(dataBuffer);

			if(turns == m_potentiometerTurns)
				m_potentiometerTurnsVerified = true;
			else
				// It's wrong - set it again
				ConfigPotentiometerTurns(m_potentiometerTurns);
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_POT_TURNS);
		}
	}

	if(!m_limitModeVerified)
	{
		if(getMessage(LM_API_CFG_LIMIT_MODE, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			LimitMode mode = (LimitMode)dataBuffer[0];

			if(mode == m_limitMode)
				m_limitModeVerified = true;
			else
			{
				// It's wrong - set it again
				ConfigLimitMode(m_limitMode);
			}
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_LIMIT_MODE);
		}
	}

	if(!m_forwardLimitVerified)
	{
		if(getMessage(LM_API_CFG_LIMIT_FWD, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double limit = unpackFXP16_16(dataBuffer);

			if(FXP16_EQ(limit, m_forwardLimit))
				m_forwardLimitVerified = true;
			else
			{
				// It's wrong - set it again
				ConfigForwardLimit(m_forwardLimit);
			}
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_LIMIT_FWD);
		}
	}

	if(!m_reverseLimitVerified)
	{
		if(getMessage(LM_API_CFG_LIMIT_REV, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double limit = unpackFXP16_16(dataBuffer);

			if(FXP16_EQ(limit, m_reverseLimit))
				m_reverseLimitVerified = true;
			else
			{
				// It's wrong - set it again
				ConfigReverseLimit(m_reverseLimit);
			}
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_LIMIT_REV);
		}
	}

	if(!m_maxOutputVoltageVerified)
	{
		if(getMessage(LM_API_CFG_MAX_VOUT, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			double voltage = unpackFXP8_8(dataBuffer);

			// The returned max output voltage is sometimes slightly higher or
			// lower than what was sent.  This should not trigger resending
			// the message.
			if(std::abs(voltage - m_maxOutputVoltage) < 0.1)
				m_maxOutputVoltageVerified = true;
			else
			{
				// It's wrong - set it again
				ConfigMaxOutputVoltage(m_maxOutputVoltage);
			}
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_MAX_VOUT);
		}
	}

	if(!m_voltageRampRateVerified)
	{
		if(m_controlMode == kPercentVbus)
		{
			if(getMessage(LM_API_VOLT_SET_RAMP, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
			{
				double rate = unpackPercentage(dataBuffer);

				if(FXP16_EQ(rate, m_voltageRampRate))
					m_voltageRampRateVerified = true;
				else
				{
					// It's wrong - set it again
					SetVoltageRampRate(m_voltageRampRate);
				}
			}
			else
			{
				// Verification is needed but not available - request it again.
				requestMessage(LM_API_VOLT_SET_RAMP);
			}
		}
		else if(m_controlMode == kVoltage)
		{
			if(getMessage(LM_API_VCOMP_COMP_RAMP, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
			{
				double rate = unpackFXP8_8(dataBuffer);

				if(FXP8_EQ(rate, m_voltageRampRate))
					m_voltageRampRateVerified = true;
				else
				{
					// It's wrong - set it again
					SetVoltageRampRate(m_voltageRampRate);
				}
			}
			else
			{
				// Verification is needed but not available - request it again.
				requestMessage(LM_API_VCOMP_COMP_RAMP);
			}
		}
	}

	if(!m_faultTimeVerified)
	{
		if(getMessage(LM_API_CFG_FAULT_TIME, CAN_MSGID_FULL_M, dataBuffer, &dataSize))
		{
			uint16_t faultTime = unpackint16_t(dataBuffer);

			if((uint16_t)(m_faultTime * 1000.0) == faultTime)
				m_faultTimeVerified = true;
			else
			{
				// It's wrong - set it again
				ConfigFaultTime(m_faultTime);
			}
		}
		else
		{
			// Verification is needed but not available - request it again.
			requestMessage(LM_API_CFG_FAULT_TIME);
		}
	}

	if(!m_receivedStatusMessage0 ||
			!m_receivedStatusMessage1 ||
			!m_receivedStatusMessage2)
	{
		// If the periodic status messages haven't been verified as received,
		// request periodic status messages again and attempt to unpack any
		// available ones.
		setupPeriodicStatus();
		GetTemperature();
		GetPosition();
		GetFaults();
	}
}

/**
 * Set the reference source device for speed controller mode.
 *
 * Choose encoder as the source of speed feedback when in speed control mode.
 *
 * @param reference Specify a speed reference.
 */
void CANJaguar::SetSpeedReference(uint8_t reference)
{
	uint8_t dataBuffer[8];

	// Send the speed reference parameter
	dataBuffer[0] = reference;
	sendMessage(LM_API_SPD_REF, dataBuffer, sizeof(uint8_t));

	m_speedReference = reference;
	m_speedRefVerified = false;
}

/**
 * Get the reference source device for speed controller mode.
 *
 * @return A speed reference indicating the currently selected reference device
 * for speed controller mode.
 */
uint8_t CANJaguar::GetSpeedReference()
{
	return m_speedReference;
}

/**
 * Set the reference source device for position controller mode.
 *
 * Choose between using and encoder and using a potentiometer
 * as the source of position feedback when in position control mode.
 *
 * @param reference Specify a PositionReference.
 */
void CANJaguar::SetPositionReference(uint8_t reference)
{
	uint8_t dataBuffer[8];

	// Send the position reference parameter
	dataBuffer[0] = reference;
	sendMessage(LM_API_POS_REF, dataBuffer, sizeof(uint8_t));

	m_positionReference = reference;
	m_posRefVerified = false;
}

/**
 * Get the reference source device for position controller mode.
 *
 * @return A PositionReference indicating the currently selected reference device for position controller mode.
 */
uint8_t CANJaguar::GetPositionReference()
{
	return m_positionReference;
}

/**
 * Set the P, I, and D constants for the closed loop modes.
 *
 * @param p The proportional gain of the Jaguar's PID controller.
 * @param i The integral gain of the Jaguar's PID controller.
 * @param d The differential gain of the Jaguar's PID controller.
 */
void CANJaguar::SetPID(double p, double i, double d)
{
	SetP(p);
	SetI(i);
	SetD(d);
}

/**
 * Set the P constant for the closed loop modes.
 *
 * @param p The proportional gain of the Jaguar's PID controller.
 */
void CANJaguar::SetP(double p)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	switch(m_controlMode)
	{
	case kPercentVbus:
	case kVoltage:
  case kFollower:
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		break;
	case kSpeed:
		dataSize = packFXP16_16(dataBuffer, p);
		sendMessage(LM_API_SPD_PC, dataBuffer, dataSize);
		break;
	case kPosition:
		dataSize = packFXP16_16(dataBuffer, p);
		sendMessage(LM_API_POS_PC, dataBuffer, dataSize);
		break;
	case kCurrent:
		dataSize = packFXP16_16(dataBuffer, p);
		sendMessage(LM_API_ICTRL_PC, dataBuffer, dataSize);
		break;
	}

	m_p = p;
	m_pVerified = false;
}

/**
 * Set the I constant for the closed loop modes.
 *
 * @param i The integral gain of the Jaguar's PID controller.
 */
void CANJaguar::SetI(double i)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	switch(m_controlMode)
	{
	case kPercentVbus:
	case kVoltage:
  case kFollower:
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		break;
	case kSpeed:
		dataSize = packFXP16_16(dataBuffer, i);
		sendMessage(LM_API_SPD_IC, dataBuffer, dataSize);
		break;
	case kPosition:
		dataSize = packFXP16_16(dataBuffer, i);
		sendMessage(LM_API_POS_IC, dataBuffer, dataSize);
		break;
	case kCurrent:
		dataSize = packFXP16_16(dataBuffer, i);
		sendMessage(LM_API_ICTRL_IC, dataBuffer, dataSize);
		break;
	}

	m_i = i;
	m_iVerified = false;
}

/**
 * Set the D constant for the closed loop modes.
 *
 * @param d The derivative gain of the Jaguar's PID controller.
 */
void CANJaguar::SetD(double d)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	switch(m_controlMode)
	{
	case kPercentVbus:
	case kVoltage:
  case kFollower:
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		break;
	case kSpeed:
		dataSize = packFXP16_16(dataBuffer, d);
		sendMessage(LM_API_SPD_DC, dataBuffer, dataSize);
		break;
	case kPosition:
		dataSize = packFXP16_16(dataBuffer, d);
		sendMessage(LM_API_POS_DC, dataBuffer, dataSize);
		break;
	case kCurrent:
		dataSize = packFXP16_16(dataBuffer, d);
		sendMessage(LM_API_ICTRL_DC, dataBuffer, dataSize);
		break;
	}

	m_d = d;
	m_dVerified = false;
}

/**
 * Get the Proportional gain of the controller.
 *
 * @return The proportional gain.
 */
double CANJaguar::GetP()
{
	if(m_controlMode == kPercentVbus || m_controlMode == kVoltage)
	{
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		return 0.0;
	}

	return m_p;
}

/**
 * Get the Intregral gain of the controller.
 *
 * @return The integral gain.
 */
double CANJaguar::GetI()
{
	if(m_controlMode == kPercentVbus || m_controlMode == kVoltage)
	{
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		return 0.0;
	}

	return m_i;
}

/**
 * Get the Differential gain of the controller.
 *
 * @return The differential gain.
 */
double CANJaguar::GetD()
{
	if(m_controlMode == kPercentVbus || m_controlMode == kVoltage)
	{
		wpi_setWPIErrorWithContext(IncompatibleMode, "PID constants only apply in Speed, Position, and Current mode");
		return 0.0;
	}

	return m_d;
}

/**
 * Enable the closed loop controller.
 *
 * Start actually controlling the output based on the feedback.
 * If starting a position controller with an encoder reference,
 * use the encoderInitialPosition parameter to initialize the
 * encoder state.
 *
 * @param encoderInitialPosition Encoder position to set if position with encoder reference.  Ignored otherwise.
 */
void CANJaguar::EnableControl(double encoderInitialPosition)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize = 0;

	switch(m_controlMode)
	{
	case kPercentVbus:
		sendMessage(LM_API_VOLT_T_EN, dataBuffer, dataSize);
		break;
	case kSpeed:
		sendMessage(LM_API_SPD_T_EN, dataBuffer, dataSize);
		break;
	case kPosition:
		dataSize = packFXP16_16(dataBuffer, encoderInitialPosition);
		sendMessage(LM_API_POS_T_EN, dataBuffer, dataSize);
		break;
	case kCurrent:
		sendMessage(LM_API_ICTRL_T_EN, dataBuffer, dataSize);
		break;
	case kVoltage:
		sendMessage(LM_API_VCOMP_T_EN, dataBuffer, dataSize);
		break;
  default:
    wpi_setWPIErrorWithContext(IncompatibleMode, "The Jaguar only supports Current, Voltage, Position, Speed, and Percent (Throttle) modes.");
    return;
	}

	m_controlEnabled = true;
	m_controlModeVerified = false;
}

/**
 * Disable the closed loop controller.
 *
 * Stop driving the output based on the feedback.
 */
void CANJaguar::DisableControl()
{
	uint8_t dataBuffer[8];
	uint8_t dataSize = 0;

	// Disable all control
	sendMessage(LM_API_VOLT_DIS, dataBuffer, dataSize);
	sendMessage(LM_API_SPD_DIS, dataBuffer, dataSize);
	sendMessage(LM_API_POS_DIS, dataBuffer, dataSize);
	sendMessage(LM_API_ICTRL_DIS, dataBuffer, dataSize);
	sendMessage(LM_API_VCOMP_DIS, dataBuffer, dataSize);

	// Stop all periodic setpoints
	sendMessage(LM_API_VOLT_T_SET, dataBuffer, dataSize, CAN_SEND_PERIOD_STOP_REPEATING);
	sendMessage(LM_API_SPD_T_SET, dataBuffer, dataSize, CAN_SEND_PERIOD_STOP_REPEATING);
	sendMessage(LM_API_POS_T_SET, dataBuffer, dataSize, CAN_SEND_PERIOD_STOP_REPEATING);
	sendMessage(LM_API_ICTRL_T_SET, dataBuffer, dataSize, CAN_SEND_PERIOD_STOP_REPEATING);
	sendMessage(LM_API_VCOMP_T_SET, dataBuffer, dataSize, CAN_SEND_PERIOD_STOP_REPEATING);

	m_controlEnabled = false;
}

/**
 * Enable controlling the motor voltage as a percentage of the bus voltage
 * without any position or speed feedback.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 */
void CANJaguar::SetPercentMode()
{
	SetControlMode(kPercentVbus);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_NONE);
}

/**
 * Enable controlling the motor voltage as a percentage of the bus voltage,
 * and enable speed sensing from a non-quadrature encoder.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 *
 * @param tag The constant CANJaguar::Encoder
 * @param codesPerRev The counts per revolution on the encoder
 */
void CANJaguar::SetPercentMode(CANJaguar::EncoderStruct, uint16_t codesPerRev)
{
	SetControlMode(kPercentVbus);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
}

/**
 * Enable controlling the motor voltage as a percentage of the bus voltage,
 * and enable speed sensing from a non-quadrature encoder.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 *
 * @param tag The constant CANJaguar::QuadEncoder
 * @param codesPerRev The counts per revolution on the encoder
 */
void CANJaguar::SetPercentMode(CANJaguar::QuadEncoderStruct, uint16_t codesPerRev)
{
	SetControlMode(kPercentVbus);
	SetPositionReference(LM_REF_ENCODER);
	SetSpeedReference(LM_REF_QUAD_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
}

/**
* Enable controlling the motor voltage as a percentage of the bus voltage,
* and enable position sensing from a potentiometer and no speed feedback.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param potentiometer The constant CANJaguar::Potentiometer
*/
void CANJaguar::SetPercentMode(CANJaguar::PotentiometerStruct)
{
	SetControlMode(kPercentVbus);
	SetPositionReference(LM_REF_POT);
	SetSpeedReference(LM_REF_NONE);
	ConfigPotentiometerTurns(1);
}

/**
 * Enable controlling the motor current with a PID loop.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 *
 * @param p The proportional gain of the Jaguar's PID controller.
 * @param i The integral gain of the Jaguar's PID controller.
 * @param d The differential gain of the Jaguar's PID controller.
 */
void CANJaguar::SetCurrentMode(double p, double i, double d)
{
	SetControlMode(kCurrent);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_NONE);
	SetPID(p, i, d);
}

/**
* Enable controlling the motor current with a PID loop, and enable speed
* sensing from a non-quadrature encoder.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param encoder The constant CANJaguar::Encoder
* @param p The proportional gain of the Jaguar's PID controller.
* @param i The integral gain of the Jaguar's PID controller.
* @param d The differential gain of the Jaguar's PID controller.
*/
void CANJaguar::SetCurrentMode(CANJaguar::EncoderStruct, uint16_t codesPerRev, double p, double i, double d)
{
	SetControlMode(kCurrent);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_NONE);
	ConfigEncoderCodesPerRev(codesPerRev);
	SetPID(p, i, d);
}

/**
* Enable controlling the motor current with a PID loop, and enable speed and
* position sensing from a quadrature encoder.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param endoer The constant CANJaguar::QuadEncoder
* @param p The proportional gain of the Jaguar's PID controller.
* @param i The integral gain of the Jaguar's PID controller.
* @param d The differential gain of the Jaguar's PID controller.
*/
void CANJaguar::SetCurrentMode(CANJaguar::QuadEncoderStruct, uint16_t codesPerRev, double p, double i, double d)
{
	SetControlMode(kCurrent);
	SetPositionReference(LM_REF_ENCODER);
	SetSpeedReference(LM_REF_QUAD_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
	SetPID(p, i, d);
}

/**
* Enable controlling the motor current with a PID loop, and enable position
* sensing from a potentiometer.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param potentiometer The constant CANJaguar::Potentiometer
* @param p The proportional gain of the Jaguar's PID controller.
* @param i The integral gain of the Jaguar's PID controller.
* @param d The differential gain of the Jaguar's PID controller.
*/
void CANJaguar::SetCurrentMode(CANJaguar::PotentiometerStruct, double p, double i, double d)
{
	SetControlMode(kCurrent);
	SetPositionReference(LM_REF_POT);
	SetSpeedReference(LM_REF_NONE);
	ConfigPotentiometerTurns(1);
	SetPID(p, i, d);
}

/**
 * Enable controlling the speed with a feedback loop from a non-quadrature
 * encoder.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 *
 * @param encoder The constant CANJaguar::Encoder
 * @param codesPerRev The counts per revolution on the encoder.
 * @param p The proportional gain of the Jaguar's PID controller.
 * @param i The integral gain of the Jaguar's PID controller.
 * @param d The differential gain of the Jaguar's PID controller.
 */
void CANJaguar::SetSpeedMode(CANJaguar::EncoderStruct, uint16_t codesPerRev, double p, double i, double d)
{
	SetControlMode(kSpeed);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
	SetPID(p, i, d);
}

/**
* Enable controlling the speed with a feedback loop from a quadrature encoder.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param encoder The constant CANJaguar::QuadEncoder
* @param codesPerRev The counts per revolution on the encoder.
* @param p The proportional gain of the Jaguar's PID controller.
* @param i The integral gain of the Jaguar's PID controller.
* @param d The differential gain of the Jaguar's PID controller.
*/
void CANJaguar::SetSpeedMode(CANJaguar::QuadEncoderStruct, uint16_t codesPerRev, double p, double i, double d)
{
	SetControlMode(kSpeed);
	SetPositionReference(LM_REF_ENCODER);
	SetSpeedReference(LM_REF_QUAD_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
	SetPID(p, i, d);
}

/**
 * Enable controlling the position with a feedback loop using an encoder.<br>
 * After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
 *
 * @param encoder The constant CANJaguar::QuadEncoder
 * @param codesPerRev The counts per revolution on the encoder.
 * @param p The proportional gain of the Jaguar's PID controller.
 * @param i The integral gain of the Jaguar's PID controller.
 * @param d The differential gain of the Jaguar's PID controller.
 *
 */
void CANJaguar::SetPositionMode(CANJaguar::QuadEncoderStruct, uint16_t codesPerRev, double p, double i, double d)
{
	SetControlMode(kPosition);
	SetPositionReference(LM_REF_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
	SetPID(p, i, d);
}

/**
* Enable controlling the position with a feedback loop using a potentiometer.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
* @param p The proportional gain of the Jaguar's PID controller.
* @param i The integral gain of the Jaguar's PID controller.
* @param d The differential gain of the Jaguar's PID controller.
*/
void CANJaguar::SetPositionMode(CANJaguar::PotentiometerStruct, double p, double i, double d)
{
	SetControlMode(kPosition);
	SetPositionReference(LM_REF_POT);
	ConfigPotentiometerTurns(1);
	SetPID(p, i, d);
}

/**
* Enable controlling the motor voltage without any position or speed feedback.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*/
void CANJaguar::SetVoltageMode()
{
	SetControlMode(kVoltage);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_NONE);
}

/**
* Enable controlling the motor voltage with speed feedback from a
* non-quadrature encoder and no position feedback.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param encoder The constant CANJaguar::Encoder
* @param codesPerRev The counts per revolution on the encoder
*/
void CANJaguar::SetVoltageMode(CANJaguar::EncoderStruct, uint16_t codesPerRev)
{
	SetControlMode(kVoltage);
	SetPositionReference(LM_REF_NONE);
	SetSpeedReference(LM_REF_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
}

/**
* Enable controlling the motor voltage with position and speed feedback from a
* quadrature encoder.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param encoder The constant CANJaguar::QuadEncoder
* @param codesPerRev The counts per revolution on the encoder
*/
void CANJaguar::SetVoltageMode(CANJaguar::QuadEncoderStruct, uint16_t codesPerRev)
{
	SetControlMode(kVoltage);
	SetPositionReference(LM_REF_ENCODER);
	SetSpeedReference(LM_REF_QUAD_ENCODER);
	ConfigEncoderCodesPerRev(codesPerRev);
}

/**
* Enable controlling the motor voltage with position feedback from a
* potentiometer and no speed feedback.<br>
* After calling this you must call {@link CANJaguar#EnableControl()} or {@link CANJaguar#EnableControl(double)} to enable the device.
*
* @param potentiometer The constant CANJaguar::Potentiometer
*/
void CANJaguar::SetVoltageMode(CANJaguar::PotentiometerStruct)
{
	SetControlMode(kVoltage);
	SetPositionReference(LM_REF_POT);
	SetSpeedReference(LM_REF_NONE);
	ConfigPotentiometerTurns(1);
}

/**
 * Used internally. In order to set the control mode see the methods listed below.
 * Change the control mode of this Jaguar object.
 *
 * After changing modes, configure any PID constants or other settings needed
 * and then EnableControl() to actually change the mode on the Jaguar.
 *
 * @param controlMode The new mode.
 */
void CANJaguar::SetControlMode(ControlMode controlMode)
{
	// Disable the previous mode
	DisableControl();

  if (controlMode == kFollower)
    wpi_setWPIErrorWithContext(IncompatibleMode, "The Jaguar only supports Current, Voltage, Position, Speed, and Percent (Throttle) modes.");

	// Update the local mode
	m_controlMode = controlMode;
	m_controlModeVerified = false;

	HALReport(HALUsageReporting::kResourceType_CANJaguar, m_deviceNumber, m_controlMode);
}

/**
 * Get the active control mode from the Jaguar.
 *
 * Ask the Jag what mode it is in.
 *
 * @return ControlMode that the Jag is in.
 */
CANJaguar::ControlMode CANJaguar::GetControlMode()
{
	return m_controlMode;
}

/**
 * Get the voltage at the battery input terminals of the Jaguar.
 *
 * @return The bus voltage in volts.
 */
float CANJaguar::GetBusVoltage()
{
	updatePeriodicStatus();

	return m_busVoltage;
}

/**
 * Get the voltage being output from the motor terminals of the Jaguar.
 *
 * @return The output voltage in volts.
 */
float CANJaguar::GetOutputVoltage()
{
	updatePeriodicStatus();

	return m_outputVoltage;
}

/**
 * Get the current through the motor terminals of the Jaguar.
 *
 * @return The output current in amps.
 */
float CANJaguar::GetOutputCurrent()
{
	updatePeriodicStatus();

	return m_outputCurrent;
}

/**
 * Get the internal temperature of the Jaguar.
 *
 * @return The temperature of the Jaguar in degrees Celsius.
 */
float CANJaguar::GetTemperature()
{
	updatePeriodicStatus();

	return m_temperature;
}

/**
 * Get the position of the encoder or potentiometer.
 *
 * @return The position of the motor in rotations based on the configured feedback.
 * @see CANJaguar#ConfigPotentiometerTurns(int)
 * @see CANJaguar#ConfigEncoderCodesPerRev(int)
 */
double CANJaguar::GetPosition()
{
	updatePeriodicStatus();

	return m_position;
}

/**
 * Get the speed of the encoder.
 *
 * @return The speed of the motor in RPM based on the configured feedback.
 */
double CANJaguar::GetSpeed()
{
	updatePeriodicStatus();

	return m_speed;
}

/**
 * Get the status of the forward limit switch.
 *
 * @return The motor is allowed to turn in the forward direction when true.
 */
bool CANJaguar::GetForwardLimitOK()
{
	updatePeriodicStatus();

	return m_limits & kForwardLimit;
}

/**
 * Get the status of the reverse limit switch.
 *
 * @return The motor is allowed to turn in the reverse direction when true.
 */
bool CANJaguar::GetReverseLimitOK()
{
	updatePeriodicStatus();

	return m_limits & kReverseLimit;
}

/**
 * Get the status of any faults the Jaguar has detected.
 *
 * @return A bit-mask of faults defined by the "Faults" enum.
 * @see #kCurrentFault
 * @see #kBusVoltageFault
 * @see #kTemperatureFault
 * @see #kGateDriverFault
 */
uint16_t CANJaguar::GetFaults()
{
	updatePeriodicStatus();

	return m_faults;
}

/**
 * Set the maximum voltage change rate.
 *
 * When in PercentVbus or Voltage output mode, the rate at which the voltage changes can
 * be limited to reduce current spikes.  Set this to 0.0 to disable rate limiting.
 *
 * @param rampRate The maximum rate of voltage change in Percent Voltage mode in V/s.
 */
void CANJaguar::SetVoltageRampRate(double rampRate)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;
	uint32_t message;

	switch(m_controlMode)
	{
	case kPercentVbus:
		dataSize = packPercentage(dataBuffer, rampRate / (m_maxOutputVoltage * kControllerRate));
		message = LM_API_VOLT_SET_RAMP;
		break;
	case kVoltage:
		dataSize = packFXP8_8(dataBuffer, rampRate / kControllerRate);
		message = LM_API_VCOMP_COMP_RAMP;
		break;
	default:
		wpi_setWPIErrorWithContext(IncompatibleMode, "SetVoltageRampRate only applies in Voltage and Percent mode");
		return;
	}

	sendMessage(message, dataBuffer, dataSize);

	m_voltageRampRate = rampRate;
	m_voltageRampRateVerified = false;
}

/**
 * Get the version of the firmware running on the Jaguar.
 *
 * @return The firmware version.  0 if the device did not respond.
 */
uint32_t CANJaguar::GetFirmwareVersion()
{
	return m_firmwareVersion;
}

/**
 * Get the version of the Jaguar hardware.
 *
 * @return The hardware version. 1: Jaguar,  2: Black Jaguar
 */
uint8_t CANJaguar::GetHardwareVersion()
{
	return m_hardwareVersion;
}

/**
 * Configure what the controller does to the H-Bridge when neutral (not driving the output).
 *
 * This allows you to override the jumper configuration for brake or coast.
 *
 * @param mode Select to use the jumper setting or to override it to coast or brake.
 */
void CANJaguar::ConfigNeutralMode(NeutralMode mode)
{
	uint8_t dataBuffer[8];

	// Set the neutral mode
	sendMessage(LM_API_CFG_BRAKE_COAST, dataBuffer, sizeof(uint8_t));

	m_neutralMode = mode;
	m_neutralModeVerified = false;
}

/**
 * Configure how many codes per revolution are generated by your encoder.
 *
 * @param codesPerRev The number of counts per revolution in 1X mode.
 */
void CANJaguar::ConfigEncoderCodesPerRev(uint16_t codesPerRev)
{
	uint8_t dataBuffer[8];

	// Set the codes per revolution mode
	packint16_t(dataBuffer, codesPerRev);
	sendMessage(LM_API_CFG_ENC_LINES, dataBuffer, sizeof(uint16_t));

	m_encoderCodesPerRev = codesPerRev;
	m_encoderCodesPerRevVerified = false;
}

/**
 * Configure the number of turns on the potentiometer.
 *
 * There is no special support for continuous turn potentiometers.
 * Only integer numbers of turns are supported.
 *
 * @param turns The number of turns of the potentiometer.
 */
void CANJaguar::ConfigPotentiometerTurns(uint16_t turns)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	// Set the pot turns
	dataSize = packint16_t(dataBuffer, turns);
	sendMessage(LM_API_CFG_POT_TURNS, dataBuffer, dataSize);

	m_potentiometerTurns = turns;
	m_potentiometerTurnsVerified = false;
}

/**
 * Configure Soft Position Limits when in Position Controller mode.
 *
 * When controlling position, you can add additional limits on top of the limit switch inputs
 * that are based on the position feedback.  If the position limit is reached or the
 * switch is opened, that direction will be disabled.
 *

 * @param forwardLimitPosition The position that if exceeded will disable the forward direction.
 * @param reverseLimitPosition The position that if exceeded will disable the reverse direction.
 */
void CANJaguar::ConfigSoftPositionLimits(double forwardLimitPosition, double reverseLimitPosition)
{
	ConfigLimitMode(kLimitMode_SoftPositionLimits);
	ConfigForwardLimit(forwardLimitPosition);
	ConfigReverseLimit(reverseLimitPosition);
}

/**
 * Disable Soft Position Limits if previously enabled.
 *
 * Soft Position Limits are disabled by default.
 */
void CANJaguar::DisableSoftPositionLimits()
{
	ConfigLimitMode(kLimitMode_SwitchInputsOnly);
}

/**
 * Set the limit mode for position control mode.
 *
 * Use ConfigSoftPositionLimits or DisableSoftPositionLimits to set this
 * automatically.
 */
void CANJaguar::ConfigLimitMode(LimitMode mode)
{
	uint8_t dataBuffer[8];

	dataBuffer[0] = mode;
	sendMessage(LM_API_CFG_LIMIT_MODE, dataBuffer, sizeof(uint8_t));

	m_limitMode = mode;
	m_limitModeVerified = false;
}

/**
* Set the position that if exceeded will disable the forward direction.
*
* Use ConfigSoftPositionLimits to set this and the limit mode automatically.
*/
void CANJaguar::ConfigForwardLimit(double forwardLimitPosition)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	dataSize = packFXP16_16(dataBuffer, forwardLimitPosition);
	dataBuffer[dataSize++] = 1;
	sendMessage(LM_API_CFG_LIMIT_FWD, dataBuffer, dataSize);

	m_forwardLimit = forwardLimitPosition;
	m_forwardLimitVerified = false;
}

/**
* Set the position that if exceeded will disable the reverse direction.
*
* Use ConfigSoftPositionLimits to set this and the limit mode automatically.
*/
void CANJaguar::ConfigReverseLimit(double reverseLimitPosition)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	dataSize = packFXP16_16(dataBuffer, reverseLimitPosition);
	dataBuffer[dataSize++] = 0;
	sendMessage(LM_API_CFG_LIMIT_REV, dataBuffer, dataSize);

	m_reverseLimit = reverseLimitPosition;
	m_reverseLimitVerified = false;
}

/**
 * Configure the maximum voltage that the Jaguar will ever output.
 *
 * This can be used to limit the maximum output voltage in all modes so that
 * motors which cannot withstand full bus voltage can be used safely.
 *
 * @param voltage The maximum voltage output by the Jaguar.
 */
void CANJaguar::ConfigMaxOutputVoltage(double voltage)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	dataSize = packFXP8_8(dataBuffer, voltage);
	sendMessage(LM_API_CFG_MAX_VOUT, dataBuffer, dataSize);

	m_maxOutputVoltage = voltage;
	m_maxOutputVoltageVerified = false;
}

/**
 * Configure how long the Jaguar waits in the case of a fault before resuming operation.
 *
 * Faults include over temerature, over current, and bus under voltage.
 * The default is 3.0 seconds, but can be reduced to as low as 0.5 seconds.
 *
 * @param faultTime The time to wait before resuming operation, in seconds.
 */
void CANJaguar::ConfigFaultTime(float faultTime)
{
	uint8_t dataBuffer[8];
	uint8_t dataSize;

	if(faultTime < 0.5) faultTime = 0.5;
	else if(faultTime > 3.0) faultTime = 3.0;

	// Message takes ms
	dataSize = packint16_t(dataBuffer, (int16_t)(faultTime * 1000.0));
	sendMessage(LM_API_CFG_FAULT_TIME, dataBuffer, dataSize);

	m_faultTime = faultTime;
	m_faultTimeVerified = false;
}

/**
 * Update all the motors that have pending sets in the syncGroup.
 *
 * @param syncGroup A bitmask of groups to generate synchronous output.
 */
void CANJaguar::UpdateSyncGroup(uint8_t syncGroup)
{
	sendMessageHelper(CAN_MSGID_API_SYNC, &syncGroup, sizeof(syncGroup), CAN_SEND_PERIOD_NO_REPEAT);
}


void CANJaguar::SetExpiration(float timeout)
{
	if (m_safetyHelper) m_safetyHelper->SetExpiration(timeout);
}

float CANJaguar::GetExpiration()
{
	if (!m_safetyHelper) return 0.0;
	return m_safetyHelper->GetExpiration();
}

bool CANJaguar::IsAlive()
{
	if (!m_safetyHelper) return false;
	return m_safetyHelper->IsAlive();
}

bool CANJaguar::IsSafetyEnabled()
{
	if (!m_safetyHelper) return false;
	return m_safetyHelper->IsSafetyEnabled();
}

void CANJaguar::SetSafetyEnabled(bool enabled)
{
	if (m_safetyHelper) m_safetyHelper->SetSafetyEnabled(enabled);
}

void CANJaguar::GetDescription(char *desc)
{
	sprintf(desc, "CANJaguar ID %d", m_deviceNumber);
}

uint8_t CANJaguar::GetDeviceID()
{
	return m_deviceNumber;
}

/**
 * Common interface for stopping the motor
 * Part of the MotorSafety interface
 *
 * @deprecated Call DisableControl instead.
 */
void CANJaguar::StopMotor()
{
	DisableControl();
}

void CANJaguar::ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew)
{
	Set(value.f);
}

void CANJaguar::UpdateTable()
{
	if (m_table != NULL)
	{
		m_table->PutNumber("Value", Get());
	}
}

void CANJaguar::StartLiveWindowMode()
{
	if (m_table != NULL)
	{
		m_table->AddTableListener("Value", this, true);
	}
}

void CANJaguar::StopLiveWindowMode()
{
	if (m_table != NULL)
	{
		m_table->RemoveTableListener(this);
	}
}

std::string CANJaguar::GetSmartDashboardType()
{
	return "Speed Controller";
}

void CANJaguar::InitTable(ITable *subTable)
{
	m_table = subTable;
	UpdateTable();
}

ITable * CANJaguar::GetTable()
{
	return m_table;
}
