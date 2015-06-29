/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2014. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/
#pragma once

#include "SpeedController.h"

/**
 * Interface for "smart" CAN-based speed controllers.
 * @see CANJaguar
 * @see CANTalon
 */
class CANSpeedController : public SpeedController
{
public:
	enum ControlMode {
		kPercentVbus=0,
		kCurrent=1,
		kSpeed=2,
		kPosition=3,
		kVoltage=4,
    kFollower=5 // Not supported in Jaguar.
	};

	enum Faults {
		kCurrentFault = 1,
		kTemperatureFault = 2,
		kBusVoltageFault = 4,
		kGateDriverFault = 8,
		/* SRX extensions */
		kFwdLimitSwitch = 0x10,
		kRevLimitSwitch = 0x20,
		kFwdSoftLimit = 0x40,
		kRevSoftLimit = 0x80,
	};

	enum Limits {
		kForwardLimit = 1,
		kReverseLimit = 2
	};

	enum NeutralMode {
		/** Use the NeutralMode that is set by the jumper wire on the CAN device */
		kNeutralMode_Jumper = 0,
		/** Stop the motor's rotation by applying a force. */
		kNeutralMode_Brake = 1,
		/** Do not attempt to stop the motor. Instead allow it to coast to a stop without applying resistance. */
		kNeutralMode_Coast = 2
	};

	enum LimitMode {
		/** Only use switches for limits */
		kLimitMode_SwitchInputsOnly = 0,
		/** Use both switches and soft limits */
		kLimitMode_SoftPositionLimits = 1,
		/* SRX extensions */
		/** Disable switches and disable soft limits */
		kLimitMode_SrxDisableSwitchInputs = 2,
	};

	virtual float Get() = 0;
	virtual void Set(float value, uint8_t syncGroup=0) = 0;
	virtual void Disable() = 0;
	virtual void SetP(double p) = 0;
	virtual void SetI(double i) = 0;
	virtual void SetD(double d) = 0;
	virtual void SetPID(double p, double i, double d) = 0;
	virtual double GetP() = 0;
	virtual double GetI() = 0;
	virtual double GetD() = 0;
	virtual float GetBusVoltage() = 0;
	virtual float GetOutputVoltage() = 0;
	virtual float GetOutputCurrent() = 0;
	virtual float GetTemperature() = 0;
	virtual double GetPosition() = 0;
	virtual double GetSpeed() = 0;
	virtual bool GetForwardLimitOK() = 0;
	virtual bool GetReverseLimitOK() = 0;
	virtual uint16_t GetFaults() = 0;
	virtual void SetVoltageRampRate(double rampRate) = 0;
	virtual uint32_t GetFirmwareVersion() = 0;
	virtual void ConfigNeutralMode(NeutralMode mode) = 0;
	virtual void ConfigEncoderCodesPerRev(uint16_t codesPerRev) = 0;
	virtual void ConfigPotentiometerTurns(uint16_t turns) = 0;
	virtual void ConfigSoftPositionLimits(double forwardLimitPosition, double reverseLimitPosition) = 0;
	virtual void DisableSoftPositionLimits() = 0;
	virtual void ConfigLimitMode(LimitMode mode) = 0;
	virtual void ConfigForwardLimit(double forwardLimitPosition) = 0;
	virtual void ConfigReverseLimit(double reverseLimitPosition) = 0;
	virtual void ConfigMaxOutputVoltage(double voltage) = 0;
	virtual void ConfigFaultTime(float faultTime) = 0;
  // Hold off on interface until we figure out ControlMode enums.
//	virtual void SetControlMode(ControlMode mode) = 0;
//	virtual ControlMode GetControlMode() = 0;
};
