/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Servo.h"

#include "NetworkCommunication/UsageReporting.h"
#include "LiveWindow/LiveWindow.h"

constexpr float Servo::kMaxServoAngle;
constexpr float Servo::kMinServoAngle;

/**
 * Common initialization code called by all constructors.
 *
 * InitServo() assigns defaults for the period multiplier for the servo PWM control signal, as
 * well as the minimum and maximum PWM values supported by the servo.
 */
void Servo::InitServo()
{
	m_table = NULL;
	SetBounds(2.27, 1.513, 1.507, 1.5, .743);
	SetPeriodMultiplier(kPeriodMultiplier_4X);


	LiveWindow::GetInstance()->AddActuator("Servo", GetModuleNumber(), GetChannel(), this);
	nUsageReporting::report(nUsageReporting::kResourceType_Servo, GetChannel(), GetModuleNumber() - 1);
}

/**
 * Constructor that assumes the default digital module.
 *
 * @param channel The PWM channel on the digital module to which the servo is attached.
 */
Servo::Servo(uint32_t channel) : SafePWM(channel)
{
	InitServo();
}

/**
 * Constructor that specifies the digital module.
 *
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The PWM channel on the digital module to which the servo is attached (1..10).
 */
Servo::Servo(uint8_t moduleNumber, uint32_t channel) : SafePWM(moduleNumber, channel)
{
	InitServo();
}

Servo::~Servo()
{
}

/**
 * Set the servo position.
 *
 * Servo values range from 0.0 to 1.0 corresponding to the range of full left to full right.
 *
 * @param value Position from 0.0 to 1.0.
 */
void Servo::Set(float value)
{
	SetPosition(value);
}

/**
 * Set the servo to offline.
 * 
 * Set the servo raw value to 0 (undriven)
 */
void Servo::SetOffline() {
	SetRaw(0);
}

/**
 * Get the servo position.
 *
 * Servo values range from 0.0 to 1.0 corresponding to the range of full left to full right.
 *
 * @return Position from 0.0 to 1.0.
 */
float Servo::Get()
{
	return GetPosition();
}

/**
 * Set the servo angle.
 *
 * Assume that the servo angle is linear with respect to the PWM value (big assumption, need to test).
 *
 * Servo angles that are out of the supported range of the servo simply "saturate" in that direction
 * In other words, if the servo has a range of (X degrees to Y degrees) than angles of less than X
 * result in an angle of X being set and angles of more than Y degrees result in an angle of Y being set.
 *
 * @param degrees The angle in degrees to set the servo.
 */
void Servo::SetAngle(float degrees)
{
	if (degrees < kMinServoAngle)
	{
		degrees = kMinServoAngle;
	}
	else if (degrees > kMaxServoAngle)
	{
		degrees = kMaxServoAngle;
	}

	SetPosition(((float) (degrees - kMinServoAngle)) / GetServoAngleRange());
}

/**
 * Get the servo angle.
 *
 * Assume that the servo angle is linear with respect to the PWM value (big assumption, need to test).
 * @return The angle in degrees to which the servo is set.
 */
float Servo::GetAngle()
{
	return (float)GetPosition() * GetServoAngleRange() + kMinServoAngle;
}

void Servo::ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew) {
	Set(value.f);
}

void Servo::UpdateTable() {
	if (m_table != NULL) {
		m_table->PutNumber("Value", Get());
	}
}

void Servo::StartLiveWindowMode() {
	if (m_table != NULL) {
		m_table->AddTableListener("Value", this, true);
	}
}

void Servo::StopLiveWindowMode() {
	if (m_table != NULL) {
		m_table->RemoveTableListener(this);
	}
}

std::string Servo::GetSmartDashboardType() {
	return "Servo";
}

void Servo::InitTable(ITable *subTable) {
	m_table = subTable;
	UpdateTable();
}

ITable * Servo::GetTable() {
	return m_table;
}


