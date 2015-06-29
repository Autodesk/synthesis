#ifndef _ANALOG_POTENTIOMETER_H
#define _ANALOG_POTENTIOMETER_H

#include "Interfaces/Potentiometer.h"
#include "LiveWindow/LiveWindowSendable.h"
#include "AnalogChannel.h"

/**
 * Class for reading analog potentiometers. Analog potentiometers read
 * in an analog voltage that corresponds to a position. Usually the
 * position is either degrees or meters. However, if no conversion is
 * given it remains volts.
 *
 * @author Alex Henning
 */
class AnalogPotentiometer : public Potentiometer, public LiveWindowSendable {
private:
    int m_module, m_channel;
    double m_scale, m_offset;
    AnalogChannel *m_analog_channel;
    void InitPot(int slot, int channel, double scale, double offset);
	ITable *m_table;
public:
    AnalogPotentiometer(int slot, int channel, double scale, double offset);
    AnalogPotentiometer(int channel, double scale, double offset);    
    AnalogPotentiometer(int channel, double scale);
    AnalogPotentiometer(int channel);
    double Get();
    double PIDGet();
    std::string GetSmartDashboardType();
    void InitTable(ITable *subtable);
    void UpdateTable();
    ITable * GetTable();
    void StartLiveWindowMode();
    void StopLiveWindowMode();
};

#endif
