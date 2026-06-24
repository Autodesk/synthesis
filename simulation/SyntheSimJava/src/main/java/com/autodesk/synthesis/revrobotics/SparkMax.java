package com.autodesk.synthesis.revrobotics;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Map;
import java.util.TreeMap;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;
import com.revrobotics.REVLibError;
import com.revrobotics.PersistMode;
import com.revrobotics.ResetMode;
import com.revrobotics.config.BaseConfig;
import com.revrobotics.spark.SparkBase;
import com.revrobotics.spark.SparkLowLevel;
import com.revrobotics.spark.config.SparkBaseConfig.IdleMode;
import com.revrobotics.spark.config.SparkBaseConfig;
import com.revrobotics.spark.config.SparkMaxConfig;

/**
 * SparkMax wrapper to add proper WPILib HALSim support.
 *
 * See original documentation for more information https://codedocs.revrobotics.com/java/com/revrobotics/spark/sparkmax
 */
public class SparkMax extends com.revrobotics.spark.SparkMax {
    private static final Map<Integer, SparkMax> s_instances = new TreeMap<>();

    private static final int kIdleMode = 6;
    private static final int kEncoderInverted = 72;
    private static final int kPositionConversionFactor = 112;
    private static final int kVelocityConversionFactor = 113;
    private static final int kDutyCyclePositionFactor = 139;
    private static final int kDutyCycleVelocityFactor = 140;
    private static final int kDutyCycleInverted = 141;
    private static final int kFollowerModeLeaderId = 194;

    private CANMotor m_motor;
    public CANEncoder m_encoder;
    private ArrayList<SparkMax> m_followers;
    private SparkMax m_leader;

    double m_encoderPositionFactor = 1.0;
    double m_encoderVelocityFactor = 1.0;
    boolean m_encoderInverted = false;

    double m_absEncoderPositionFactor = 1.0;
    double m_absEncoderVelocityFactor = 1.0;
    boolean m_absEncoderInverted = false;

    /**
     * Creates a new SparkMax, wrapped with simulation support.
     *
     * @param deviceId  CAN Device ID.
     * @param motorType Motor type.
     */
    public SparkMax(int deviceId, SparkLowLevel.MotorType motorType) {
        super(deviceId, motorType);

        this.m_motor = new CANMotor("SYN SparkMax", deviceId, 0.0, false, 0.3);
        this.m_encoder = new CANEncoder("SYN SparkMax", deviceId);
        this.m_followers = new ArrayList<SparkMax>();
        s_instances.put(deviceId, this);
    }

    /**
     * Sets the percent output of the real and simulated motors.
     * Propagates to all registered sim followers.
     *
     * @param percent The new percent output [-1.0, 1.0]
     */
    @Override
    public void set(double percent) {
        super.set(percent);
        this.m_motor.setPercentOutput(percent);
        for (SparkMax follower : this.m_followers) {
            follower.set(percent);
        }
    }

    @Override
    public REVLibError configure(
        SparkBaseConfig config,
        com.revrobotics.ResetMode resetMode,
        com.revrobotics.PersistMode persistMode
    ) {
        REVLibError result = super.configure(config, resetMode, persistMode);

        Object idleMode = readConfigParam(config, kIdleMode);
        if (idleMode instanceof Integer) {
            m_motor.setBrakeMode(((Integer) idleMode) == 1);
        }

        Object posFactor = readConfigParam(config, kPositionConversionFactor);
        if (posFactor instanceof Float) {
            m_encoderPositionFactor = (Float) posFactor;
        }

        Object velFactor = readConfigParam(config, kVelocityConversionFactor);
        if (velFactor instanceof Float) {
            m_encoderVelocityFactor = (Float) velFactor;
        }

        Object encInverted = readConfigParam(config, kEncoderInverted);
        if (encInverted instanceof Boolean) {
            m_encoderInverted = (Boolean) encInverted;
        }

        Object absPosFactor = readConfigParam(config, kDutyCyclePositionFactor);
        if (absPosFactor instanceof Float) {
            m_absEncoderPositionFactor = (Float) absPosFactor;
        }

        Object absVelFactor = readConfigParam(config, kDutyCycleVelocityFactor);
        if (absVelFactor instanceof Float) {
            m_absEncoderVelocityFactor = (Float) absVelFactor;
        }

        Object absInverted = readConfigParam(config, kDutyCycleInverted);
        if (absInverted instanceof Boolean) {
            m_absEncoderInverted = (Boolean) absInverted;
        }

        Object leaderIdObj = readConfigParam(config, kFollowerModeLeaderId);
        if (leaderIdObj instanceof Integer) {
            int leaderId = (Integer) leaderIdObj;
            if (leaderId != 0) {
                if (m_leader != null) {
                    m_leader.m_followers.remove(this);
                }
                m_leader = s_instances.get(leaderId);
                if (m_leader != null) {
                    m_leader.m_followers.add(this);
                }
            } else {
                if (m_leader != null) {
                    m_leader.m_followers.remove(this);
                    m_leader = null;
                }
            }
        }

        return result;
    }

    public com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder getAbsoluteEncoderSim() {
        return new com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder(this);
    }

    public com.autodesk.synthesis.revrobotics.RelativeEncoder getEncoderSim() {
        return new RelativeEncoder(this, this.m_encoder);
    }

    void newFollower(SparkMax f) {
        this.m_followers.add(f);
    }

    private static Object readConfigParam(SparkBaseConfig config, int paramId) {
        try {
            Field f = BaseConfig.class.getDeclaredField("parameters");
            f.setAccessible(true);
            @SuppressWarnings("unchecked")
            Map<Integer, Object> params = (Map<Integer, Object>) f.get(config);
            return params.get(paramId);
        } catch (Exception e) {
            return null;
        }
    }
}
