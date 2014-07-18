/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef PWM_H_
#define PWM_H_

#include "SensorBase.h"
#include "LiveWindow/LiveWindowSendable.h"
#include "tables/ITableListener.h"

class DigitalModule;

/**
 * Class implements the PWM generation in the FPGA.
 * 
 * The values supplied as arguments for PWM outputs range from -1.0 to 1.0. They are mapped
 * to the hardware dependent values, in this case 0-255 for the FPGA.
 * Changes are immediately sent to the FPGA, and the update occurs at the next
 * FPGA cycle. There is no delay.
 * 
 * As of revision 0.1.10 of the FPGA, the FPGA interprets the 0-255 values as follows:
 *   - 255 = full "forward"
 *   - 254 to 129 = linear scaling from "full forward" to "center"
 *   - 128 = center value
 *   - 127 to 2 = linear scaling from "center" to "full reverse"
 *   - 1 = full "reverse"
 *   - 0 = disabled (i.e. PWM output is held low)
 */
class PWM : public SensorBase, public ITableListener, public LiveWindowSendable
{
	friend class DigitalModule;
public:
	typedef enum {kPeriodMultiplier_1X = 1, kPeriodMultiplier_2X = 2, kPeriodMultiplier_4X = 4} PeriodMultiplier;

	explicit PWM(uint32_t channel);
	PWM(uint8_t moduleNumber, uint32_t channel);
	virtual ~PWM();
	virtual void SetRaw(uint8_t value);
	virtual uint8_t GetRaw();
	void SetPeriodMultiplier(PeriodMultiplier mult);
	void EnableDeadbandElimination(bool eliminateDeadband);
	void SetBounds(int32_t max, int32_t deadbandMax, int32_t center, int32_t deadbandMin, int32_t min);
	void SetBounds(double max, double deadbandMax, double center, double deadbandMin, double min);
	uint32_t GetChannel() {return m_channel;}
	uint32_t GetModuleNumber();

protected:
    /**
     * kDefaultPwmPeriod is in ms
     *
     * - 20ms periods (50 Hz) are the "safest" setting in that this works for all devices
     * - 20ms periods seem to be desirable for Vex Motors
     * - 20ms periods are the specified period for HS-322HD servos, but work reliably down
     *      to 10.0 ms; starting at about 8.5ms, the servo sometimes hums and get hot;
     *      by 5.0ms the hum is nearly continuous
     * - 10ms periods work well for Victor 884
     * - 5ms periods allows higher update rates for Luminary Micro Jaguar speed controllers.
     *      Due to the shipping firmware on the Jaguar, we can't run the update period less
     *      than 5.05 ms.
     *
     * kDefaultPwmPeriod is the 1x period (5.05 ms).  In hardware, the period scaling is implemented as an
     * output squelch to get longer periods for old devices.
     */
    static constexpr float kDefaultPwmPeriod = 5.05;
    /**
     * kDefaultPwmCenter is the PWM range center in ms
     */
    static constexpr float kDefaultPwmCenter = 1.5;
    /**
     * kDefaultPWMStepsDown is the number of PWM steps below the centerpoint
     */
    static const int32_t kDefaultPwmStepsDown = 128;
	static const int32_t kPwmDisabled = 0;

	virtual void SetPosition(float pos);
	virtual float GetPosition();
	virtual void SetSpeed(float speed);
	virtual float GetSpeed();

	bool m_eliminateDeadband;
	int32_t m_maxPwm;
	int32_t m_deadbandMaxPwm;
	int32_t m_centerPwm;
	int32_t m_deadbandMinPwm;
	int32_t m_minPwm;
	
	void ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew);
	void UpdateTable();
	void StartLiveWindowMode();
	void StopLiveWindowMode();
	std::string GetSmartDashboardType();
	void InitTable(ITable *subTable);
	ITable * GetTable();
	
	ITable *m_table;

private:
	void InitPWM(uint8_t moduleNumber, uint32_t channel);
	uint32_t m_channel;
	DigitalModule *m_module;
	int32_t GetMaxPositivePwm() { return m_maxPwm; };
	int32_t GetMinPositivePwm() { return m_eliminateDeadband ? m_deadbandMaxPwm : m_centerPwm + 1; };
	int32_t GetCenterPwm() { return m_centerPwm; };
	int32_t GetMaxNegativePwm() { return m_eliminateDeadband ? m_deadbandMinPwm : m_centerPwm - 1; };
	int32_t GetMinNegativePwm() { return m_minPwm; };
	int32_t GetPositiveScaleFactor() {return GetMaxPositivePwm() - GetMinPositivePwm();} ///< The scale for positive speeds.
	int32_t GetNegativeScaleFactor() {return GetMaxNegativePwm() - GetMinNegativePwm();} ///< The scale for negative speeds.
	int32_t GetFullRangeScaleFactor() {return GetMaxPositivePwm() - GetMinNegativePwm();} ///< The scale for positions.
};

#endif
