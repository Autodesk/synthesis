package com.autodesk.synthesis.ctre;

import com.ctre.phoenix6.hardware.DeviceIdentifier;
import com.ctre.phoenix6.configs.TorqueCurrentConfigs;
import com.ctre.phoenix6.StatusCode;

//import com.ctre.phoenix6.configs.TalonFXConfigurator

public class TalonFXConfigurator extends com.ctre.phoenix6.configs.TalonFXConfigurator {
    private TalonFX devicePtr;

    public TalonFXConfigurator(DeviceIdentifier id, TalonFX device) {
        super(id);
        // awful, jank solution, please help
        // if you know how to get a device from an id, let me know
        this.devicePtr = device;
    }

    @Override
    public StatusCode apply(TorqueCurrentConfigs newTorqueCurrent) {
        StatusCode code = super.apply(newTorqueCurrent);
        double newDeadband = newTorqueCurrent.TorqueNeutralDeadband;
        this.devicePtr.setNeutralDeadband(newDeadband);
        return code;
    }
}
