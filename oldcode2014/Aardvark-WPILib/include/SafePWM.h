/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __SAFE_PWM__
#define __SAFE_PWM__

#include "MotorSafety.h"
#include "PWM.h"

class MotorSafetyHelper;

/**
 * A safe version of the PWM class.
 * It is safe because it implements the MotorSafety interface that provides timeouts
 * in the event that the motor value is not updated before the expiration time.
 * This delegates the actual work to a MotorSafetyHelper object that is used for all
 * objects that implement MotorSafety.
 */
class SafePWM: public PWM, public MotorSafety
{
public:
	explicit SafePWM(uint32_t channel);
	SafePWM(uint8_t moduleNumber, uint32_t channel);
	~SafePWM();

	void SetExpiration(float timeout);
	float GetExpiration();
	bool IsAlive();
	void StopMotor();
	bool IsSafetyEnabled();
	void SetSafetyEnabled(bool enabled);
	void GetDescription(char *desc);

	virtual void SetSpeed(float speed);
private:
	void InitSafePWM();
	MotorSafetyHelper *m_safetyHelper;
};

#endif
