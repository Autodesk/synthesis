
#include "AnalogInput.h"
#include "interfaces/Potentiometer.h"
#include "LiveWindow/LiveWindowSendable.h"

/**
 * Class for reading analog potentiometers. Analog potentiometers read
 * in an analog voltage that corresponds to a position. Usually the
 * position is either degrees or meters. However, if no conversion is
 * given it remains volts.
 *
 * @author Alex Henning
 */
class AnalogPotentiometer : public Potentiometer, public LiveWindowSendable {
public:
    /**
     * AnalogPotentiometer constructor.
     *
     * Use the scaling and offset values so that the output produces
     * meaningful values. I.E: you have a 270 degree potentiometer and
     * you want the output to be degrees with the halfway point as 0
     * degrees. The scale value is 270.0(degrees)/5.0(volts) and the
     * offset is -135.0 since the halfway point after scaling is 135
     * degrees.
     *
     * @param channel The analog channel this potentiometer is plugged into.
     * @param scale The scaling to multiply the voltage by to get a meaningful unit.
     * @param offset The offset to add to the scaled value for controlling the zero value
     */
    AnalogPotentiometer(int channel, double scale = 1.0, double offset = 0.0);

    AnalogPotentiometer(AnalogInput *input, double scale = 1.0, double offset = 0.0);

    AnalogPotentiometer(AnalogInput &input, double scale = 1.0, double offset = 0.0);

    virtual ~AnalogPotentiometer();

    /**
     * Get the current reading of the potentiomere.
     *
     * @return The current position of the potentiometer.
     */
    virtual double Get();


    /**
     * Implement the PIDSource interface.
     *
     * @return The current reading.
     */
    virtual double PIDGet();


    /*
     * Live Window code, only does anything if live window is activated.
     */
    virtual std::string GetSmartDashboardType();
    virtual void InitTable(ITable *subtable);
    virtual void UpdateTable();
    virtual ITable* GetTable();

    /**
     * AnalogPotentiometers don't have to do anything special when entering the LiveWindow.
     */
    virtual void StartLiveWindowMode() {}

    /**
     * AnalogPotentiometers don't have to do anything special when exiting the LiveWindow.
     */
    virtual void StopLiveWindowMode() {}

private:
    double m_scale, m_offset;
    AnalogInput* m_analog_input;
    ITable* m_table;
    bool m_init_analog_input;

    /**
     * Common initialization code called by all constructors.
     */
    void initPot(AnalogInput *input, double scale, double offset);
};
