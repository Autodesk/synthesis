package com.autodesk.synthesis.ctre;

import com.ctre.phoenix6.hardware.DeviceIdentifier;
import com.ctre.phoenix6.configs.TorqueCurrentConfigs;
import com.ctre.phoenix6.StatusCode;

/**
 * TalonFXConfigurator wrapper to add proper WPILib HALSim support.
 */
public class TalonFXConfigurator extends com.ctre.phoenix6.configs.TalonFXConfigurator {
    private TalonFX devicePtr;

    /**
     * Creates a new TalonFXConfigurator, wrapped with simulation support.
     * 
     * @param id Device ID
     * @param device The motor to configure
     */
    public TalonFXConfigurator(DeviceIdentifier id, TalonFX device) {
        super(id);
        // awful, jank solution, please help
        // if you know how to get a device from an id, let me know
        this.devicePtr = device;
    }
    
    /**
     * Applies a torque configuration to a TalonFX motor and passes the new neutral deadband to the simulated motor in fission if applicable
     *
     * @param newTorqueCurrent The new torque configuration for this motor
     */
    @Override
    public StatusCode apply(TorqueCurrentConfigs newTorqueCurrent) {
        StatusCode code = super.apply(newTorqueCurrent);
        double newDeadband = newTorqueCurrent.TorqueNeutralDeadband;
        this.devicePtr.setNeutralDeadband(newDeadband);
        return code;
    }
}
