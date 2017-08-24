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