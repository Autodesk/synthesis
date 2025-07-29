export enum SimType {
    PWM = "PWM",
    SIM_DEVICE = "SimDevice",
    CAN_MOTOR = "CANMotor",
    SOLENOID = "Solenoid",
    CAN_ENCODER = "CANEncoder",
    GYRO = "Gyro",
    ACCELEROMETER = "Accel",
    DIO = "DIO",
    AI = "AI",
    AO = "AO",
    DRIVERS_STATION = "DriverStation",
}

export enum FieldType {
    READ = 0,
    WRITE = 1,
    BOTH = 2,
    UNKNOWN = -1,
}

export enum RobotSimMode {
    DISABLED = 0,
    TELEOP = 1,
    AUTO = 2,
}

export type AllianceStation = "red1" | "red2" | "red3" | "blue1" | "blue2" | "blue3"
