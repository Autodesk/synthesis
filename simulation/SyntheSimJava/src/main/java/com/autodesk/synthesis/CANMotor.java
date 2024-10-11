package com.autodesk.synthesis;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimDouble;

/**
 * CANMotor class for easy implementation of documentation-compliant simulation
 * data.
 * 
 * See
 * https://github.com/wpilibsuite/allwpilib/blob/6478ba6e3fa317ee041b8a41e562d925602b6ea4/simulation/halsim_ws_core/doc/hardware_ws_api.md
 * for documentation on the WebSocket API Specification.
 */
public class CANMotor {

    private SimDevice m_device;

    private SimBoolean m_init;

    private SimDouble m_percentOutput;
    private SimBoolean m_brakeMode;
    private SimDouble m_neutralDeadband;

    private SimDouble m_supplyCurrent;
    private SimDouble m_motorCurrent;
    private SimDouble m_busVoltage;

    /**
     * Creates a CANMotor sim device in accordance with the WebSocket API
     * Specification.
     * 
     * @param name                   Name of the CAN Motor. This is generally the
     *                               class name of the originating motor, prefixed
     *                               with something (ie. "SYN CANSparkMax").
     * @param deviceId               CAN Device ID.
     * @param defaultPercentOutput   Default PercentOutput value. [-1.0, 1.0]
     * @param defaultBrakeMode       Default BrakeMode value. (true/false)
     * @param defaultNeutralDeadband Default Neutral Deadband value. This is used to
     *                               determine when braking should be enabled. [0.0,
     *                               1.0]
     */
    public CANMotor(String name, int deviceId, double defaultPercentOutput, boolean defaultBrakeMode,
            double defaultNeutralDeadband) {
        m_device = SimDevice.create(String.format("%s:%s", "CANMotor", name), deviceId);

        m_init = m_device.createBoolean("init", Direction.kOutput, true);

        m_percentOutput = m_device.createDouble("percentOutput", Direction.kOutput, 0.0);
        m_brakeMode = m_device.createBoolean("brakeMode", Direction.kOutput, false);
        m_neutralDeadband = m_device.createDouble("neutralDeadband", Direction.kOutput, 0.5);

        m_supplyCurrent = m_device.createDouble("supplyCurrent", Direction.kInput, 120.0);
        m_motorCurrent = m_device.createDouble("motorCurrent", Direction.kInput, 120.0);
        m_busVoltage = m_device.createDouble("busVoltage", Direction.kInput, 12.0);
        m_busVoltage.set(0.0); // disable CANMotor inputs
        m_init.set(true);
    }

    /**
     * Set the PercentOutput of the motor.
     * 
     * @param percent [-1.0, 1.0]
     */
    public void setPercentOutput(double percent) {
        if (Double.isNaN(percent) || Double.isInfinite(percent)) {
            percent = 0.0;
        }

        m_percentOutput.set(Math.min(1.0, Math.max(-1.0, percent)));
    }

    /**
     * Set the BrakeMode of the motor.
     * 
     * @param brake True to enable braking. False to not.
     */
    public void setBrakeMode(boolean brake) {
        m_brakeMode.set(brake);
    }

    /**
     * Set the neutral deadband of the motor. Essentially when to enable brake mode.
     * 
     * @param deadband [0.0, 1.0]
     */
    public void setNeutralDeadband(double deadband) {
        if (Double.isNaN(deadband) || Double.isInfinite(deadband)) {
            deadband = 0.0;
        }

        m_neutralDeadband.set(Math.min(1.0, Math.max(0.0, deadband)));
    }

    
    /**
     * Get the supply current, simulated.
     * 
     * @return Supply current in amps.
     */
    public double getSupplyCurrent() {
        return m_supplyCurrent.get();
    }

    /**
     * Get the motor current, simulated.
     * 
     * @return Motor current in amps.
     */
    public double getMotorCurrent() {
        return m_motorCurrent.get();
    }

    /**
     * Get the Bus Voltage, simulated.
     * 
     * @return Bus voltage
     */
    public double getBusVoltage() {
        return m_busVoltage.get();
    }
}
