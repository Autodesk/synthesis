/// <summary>
/// Types of joint drivers.
/// </summary>
public enum JointDriverType : byte
{
    MOTOR = 1,
    SERVO = 2,
    WORM_SCREW = 3,
    BUMPER_PNEUMATIC = 4,
    RELAY_PNEUMATIC = 5,
    DUAL_MOTOR = 6,
    ELEVATOR = 7
}

/// <summary>
/// Types of joint motors
/// </summary>
public enum MotorType : byte
{
    GENERIC = 0,
    CIM = 1,
    MINI_CIM = 2,
    BAG_MOTOR = 3,
    REDLINE_775_PRO = 4,
    _9015 = 5,
    BANEBOTS_775_18v = 6,
    BANEBOTS_775_12v = 7,
    BANEBOTS_550_12v = 8,
    ANDYMARK_775_125 = 9,
    SNOW_BLOWER = 10,
    NIDEN_BLDC = 11,
    THROTTLE_MOTOR = 12,
    WINDOW_MOTOR = 13,
    NEVEREST = 14,
    TETRIX_MOTOR = 15,
    MODERN_ROBOTICS_MATRIX_12V = 16,
    REV_ROBOTICS_HD_HEX_12V = 17,
    REV_ROBOTICS_CORE_HEX_12V = 18,
    VEX_V5_Smart_Motor = 19,
    VEX_269 = 20,
    VEX_393 = 21
}

public static class JointDriverTypeExtensions
{
    /// <summary>
    /// Checks if the given driver type requires two ports.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>True is the given type requires two ports</returns>
    public static bool HasTwoPorts(this JointDriverType type)
    {
        return type == JointDriverType.BUMPER_PNEUMATIC || type == JointDriverType.DUAL_MOTOR;
    }

    /// <summary>
    /// Gets the string representation of the port for the given driver type.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Name of port type</returns>
    public static string GetPortType(this JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.WORM_SCREW:
            case JointDriverType.ELEVATOR:
                return "PWM";
            case JointDriverType.BUMPER_PNEUMATIC:
                return "Solenoid";
            case JointDriverType.RELAY_PNEUMATIC:
                return "Relay";
            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// Gets the string representation of the port for the given driver type.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Name of port type</returns>
    public static string GetPortType(this JointDriverType type, bool isCan)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.WORM_SCREW:
            case JointDriverType.ELEVATOR:
                return isCan ? "CAN" : "PWM";
            case JointDriverType.BUMPER_PNEUMATIC:
                return "Solenoid";
            case JointDriverType.RELAY_PNEUMATIC:
                return "Relay";
            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// Checks if the given driver type is a motor.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsMotor(this JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.DUAL_MOTOR:
                return true;
            default:
                return false;
        }
    }

    public static bool IsElevator(this JointDriverType type)
    {
        return type == JointDriverType.ELEVATOR;
    }
    /// <summary>
    /// Checks if the given driver type is pneumatic.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsPneumatic(this JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.BUMPER_PNEUMATIC:
            case JointDriverType.RELAY_PNEUMATIC:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Checks if the given driver type is a worm screw.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsWormScrew(this JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.WORM_SCREW:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the maximum port number for the given driver type.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Max port number</returns>
    public static int GetPortMax(this JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.WORM_SCREW:
            case JointDriverType.ELEVATOR:
                return 8; // PWM
            case JointDriverType.BUMPER_PNEUMATIC:
                return 8; // Bumper
            case JointDriverType.RELAY_PNEUMATIC:
                return 8; // Relay
            default:
                return -1;
        }
    }
}