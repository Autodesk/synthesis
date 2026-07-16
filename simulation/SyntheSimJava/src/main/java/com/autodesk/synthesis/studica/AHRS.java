package com.autodesk.synthesis.studica;

import edu.wpi.first.wpilibj.SPI;

import com.autodesk.synthesis.Gyro;
import com.autodesk.synthesis.Accel;
import edu.wpi.first.wpilibj.ADXL362;

/**
 * NavX AHRS wrapper to add proper WPILib HALSim support.
 *
 * NOTE: no filtering or sensor fusion is provided.
 */
public class AHRS extends com.studica.frc.AHRS {
    private Gyro m_gyro;
    private Accel m_accel;

    private double adjustment = 0.0;
    private double yawOffset = 0.0;

    private static final float MOVING_THRESHOLD = 0.5F;
    private static final float ROTATION_THRESHOLD = 2.0F;

    public AHRS() {
        this(NavXComType.kMXP_SPI);
    }

    public AHRS(NavXComType comType) {
        super(comType);
        this.m_gyro = new Gyro("SYN AHRS", comType.ordinal());
        this.m_gyro.setConnected(true);
        this.m_accel = new Accel("SYN AHRS", comType.ordinal());
        this.m_accel.setConnected(true);
    }

///region Gyro
    
    @Override
    public double getAngle() {
        return m_gyro.getAngleZ() + this.adjustment;
    }

    /**
     * NOTE: no tilt compensation
     */
    @Override
    public float getCompassHeading() {
        // TODO: tilt compensation?
        return (float) (-m_gyro.getAngleZ() % 360);
    }

    @Override
    public float getYaw() {
        return (float) (-(m_gyro.getAngleZ() - yawOffset) % 360 - 180);
    }

    @Override
    public float getPitch() {
        return (float) (m_gyro.getAngleX() % 360 - 180);
    }

    @Override
    public float getRoll() {
        return (float) (m_gyro.getAngleY() % 360 - 180);
    }

    @Override
    public double getRate() {
        return m_gyro.getRateZ();
    }

    // TODO: quaternions

    @Override
    public float getRawGyroX() {
        return (float) m_gyro.getRateX();
    }

    @Override
    public float getRawGyroY() {
        return (float) m_gyro.getRateY();
    }

    @Override
    public float getRawGyroZ() {
        return (float) m_gyro.getRateZ();
    }

    @Override
    public double getAngleAdjustment() {
        return this.adjustment;
    }

    @Override
    public void setAngleAdjustment(double adjustment) {
        this.adjustment = adjustment;
    }

    @Override
    public void zeroYaw() {
        this.yawOffset = m_gyro.getAngleZ();
    }

    @Override
    public boolean isRotating() {
        return Math.abs(getRate()) > ROTATION_THRESHOLD;
    }

    @Override
    public void reset() {
        zeroYaw(); // is this right?
    }

///endregion

///region Accelerometer

    @Override
    public float getRawAccelX() {
        return (float) m_accel.getX();
    }

    @Override
    public float getRawAccelY() {
        return (float) m_accel.getY();
    }

    @Override
    public float getRawAccelZ() {
        return (float) m_accel.getZ();
    }

    @Override
    public float getWorldLinearAccelX() {
        return (float) m_accel.getX();
    }

    @Override
    public float getWorldLinearAccelY() {
        return (float) m_accel.getY();
    }

    @Override
    public float getWorldLinearAccelZ() {
        return (float) m_accel.getZ();
    }

    @Override
    public float getVelocityX() {
        return (float) m_accel.getVelX();
    }

    @Override
    public float getVelocityY() {
        return (float) m_accel.getVelY();
    }

    @Override
    public float getVelocityZ() {
        return (float) m_accel.getVelZ();
    }

    @Override
    public boolean isMoving() {
        return Math.abs(getVelocityX()) > MOVING_THRESHOLD
        || Math.abs(getVelocityY()) > MOVING_THRESHOLD;
    }

///endregion

    /**
     * WARNING: temperature not simulated, so always returns 25.0C
     */
    @Override
    public float getTempC() {
        return 25.0F;
    }

///region Magnetometer (unsupported)

    /**
     * WARNING: magnetometer not supported, so always returns true
     */
    @Override
    public boolean isMagnetometerCalibrated() {
        return true;
    }

    /**
     * WARNING: magnetometer not supported, so always returns false
     */
    @Override
    public boolean isMagneticDisturbance() {
        return false;
    }

    /**
     * WARNING: magnetometer not supported, so always returns 0.0
     */
    @Override
    public float getRawMagX() {
        return 0.0F;
    }

    /**
     * WARNING: magnetometer not supported, so always returns 0.0
     */
    @Override
    public float getRawMagY() {
        return 0.0F;
    }

    /**
     * WARNING: magnetometer not supported, so always returns 0.0
     */
    @Override
    public float getRawMagZ() {
        return 0.0F;
    }
///endregion
///region Altimeter & Barometer (unsupported)

    /**
     * WARNING: altimeter not supported, so always returns false
     */
    @Override
    public boolean isAltitudeValid() {
        return false;
    }

    /**
     * WARNING: altimeter not supported, so always returns 0.0
     */
    @Override
    public float getAltitude() {
        return 0.0F;
    }

    /**
     * WARNING: barometer not supported, so always returns 1013.25 millibar (1 atm)
     */
    @Override
    public float getBarometricPressure() {
        return 1013.25F;
    }

    /**
     * WARNING: barometer not supported, so always returns 1013.25 millibar (1 atm)
     */
    @Override
    public float getPressure() {
        return 1013.25F;
    }

///endregion
}
