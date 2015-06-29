#include "AnalogPotentiometer.h"

/**
 * Class for reading analog potentiometers. Analog potentiometers read
 * in an analog voltage that corresponds to a position. Usually the
 * position is either degrees or meters. However, if no conversion is
 * given it remains volts.
 *
 */
    void AnalogPotentiometer::InitPot(int slot, int channel, double scale, double offset) {
        m_module = slot;
        m_channel = channel;
        m_scale = scale;
        m_offset = offset;
        m_analog_channel = new AnalogChannel(slot, channel);
    }
    
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
     * @param slot The analog module this potentiometer is plugged into.
     * @param channel The analog channel this potentiometer is plugged into.
     * @param scale The scaling to multiply the voltage by to get a meaningful unit.
     * @param offset The offset to add to the scaled value for controlling the zero value
     */
    AnalogPotentiometer::AnalogPotentiometer(int slot, int channel, double scale, double offset) {
        InitPot(slot, channel, scale, offset);
    }
    
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
    AnalogPotentiometer::AnalogPotentiometer(int channel, double scale, double offset) {
        InitPot(1, channel, scale, offset);
    }
    
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
     */
    AnalogPotentiometer::AnalogPotentiometer(int channel, double scale) {
        InitPot(1, channel, scale, 0);
    }
    
    /**
     * AnalogPotentiometer constructor.
     *
     * @param channel The analog channel this potentiometer is plugged into.
     */
    AnalogPotentiometer::AnalogPotentiometer(int channel) {
        InitPot(1, channel, 1, 0);
    }
    
    /**
     * Get the current reading of the potentiomere.
     *
     * @return The current position of the potentiometer.
     */
    double AnalogPotentiometer::Get() {
        return m_analog_channel->GetVoltage() * m_scale + m_offset;
    }
    
    
    /**
     * Implement the PIDSource interface.
     *
     * @return The current reading.
     */
    double AnalogPotentiometer::PIDGet() {
        return Get();
    }
    
    /*
     * Live Window code, only does anything if live window is activated.
     */
    std::string AnalogPotentiometer::GetSmartDashboardType(){
        return "Analog Input";
    }
    
    ITable *m_table;
    
    /**
     * {@inheritDoc}
     */
    void AnalogPotentiometer::InitTable(ITable *subtable) {
        m_table = subtable;
        UpdateTable();
    }
    
    /**
     * {@inheritDoc}
     */
    void AnalogPotentiometer::UpdateTable() {
        if (m_table != NULL) {
            m_table->PutNumber("Value", Get());
        }
    }
    
    /**
     * {@inheritDoc}
     */
    ITable * AnalogPotentiometer::GetTable(){
        return m_table;
    }
    
    /**
     * Analog Channels don't have to do anything special when entering the LiveWindow.
     * {@inheritDoc}
     */
    void AnalogPotentiometer::StartLiveWindowMode() {}
    
    /**
     * Analog Channels don't have to do anything special when exiting the LiveWindow.
     * {@inheritDoc}
     */
    void AnalogPotentiometer::StopLiveWindowMode() {}

