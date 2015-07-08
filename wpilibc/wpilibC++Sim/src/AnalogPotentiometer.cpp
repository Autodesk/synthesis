
#include "AnalogPotentiometer.h"

/**
 * Common initialization code called by all constructors.
 */
void AnalogPotentiometer::initPot(AnalogInput *input, double scale, double offset) {
    m_scale = scale;
    m_offset = offset;
    m_analog_input = input;
}

AnalogPotentiometer::AnalogPotentiometer(int channel, double scale, double offset) {
    m_init_analog_input = true;
    initPot(new AnalogInput(channel), scale, offset);
}

AnalogPotentiometer::AnalogPotentiometer(AnalogInput *input, double scale, double offset) {
    m_init_analog_input = false;
    initPot(input, scale, offset);
}

AnalogPotentiometer::AnalogPotentiometer(AnalogInput &input, double scale, double offset) {
    m_init_analog_input = false;
    initPot(&input, scale, offset);
}

AnalogPotentiometer::~AnalogPotentiometer() {
  if(m_init_analog_input){
    delete m_analog_input;
    m_init_analog_input = false;
  }
}

/**
 * Get the current reading of the potentiomere.
 *
 * @return The current position of the potentiometer.
 */
double AnalogPotentiometer::Get() {
    return m_analog_input->GetVoltage() * m_scale + m_offset;
}

/**
 * Implement the PIDSource interface.
 *
 * @return The current reading.
 */
double AnalogPotentiometer::PIDGet() {
    return Get();
}


/**
 * @return the Smart Dashboard Type
 */
std::string AnalogPotentiometer::GetSmartDashboardType() {
    return "Analog Input";
}

/**
 * Live Window code, only does anything if live window is activated.
 */
void AnalogPotentiometer::InitTable(ITable *subtable) {
    m_table = subtable;
    UpdateTable();
}

void AnalogPotentiometer::UpdateTable() {
    if (m_table != NULL) {
        m_table->PutNumber("Value", Get());
    }
}

ITable* AnalogPotentiometer::GetTable() {
    return m_table;
}
