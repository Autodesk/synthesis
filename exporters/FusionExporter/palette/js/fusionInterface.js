// Joint Types
var JOINT_NEITHER = 0;
var JOINT_ANGULAR = 1;
var JOINT_LINEAR = 2;
var JOINT_BOTH = 3;

// Driver Types
var DRIVER_UNKNOWN = 0;
var DRIVER_MOTOR = 1;
var DRIVER_SERVO = 2;
var DRIVER_WORM_SCREW = 3;
var DRIVER_BUMPER_PNEUMATIC = 4;
var DRIVER_RELAY_PNEUMATIC = 5;
var DRIVER_DUAL_MOTOR = 6;
var DRIVER_ELEVATOR = 7;

// Signal Types
var PWM = 1;
var CAN = 2;

// Wheel Types
var WHEEL_NORMAL = 1;
var WHEEL_OMNI = 2;
var WHEEL_MECANUM = 3;

// Friction Levels
var FRICTION_LOW = 1;
var FRICTION_MEDIUM = 2;
var FRICTION_HIGH = 3;

// Pneumatic Widths
var WIDTH_2_5MM = 0;
var WIDTH_5MM = 1;
var WIDTH_10MM = 2;

// Pneumatic Pressures
var PRESSURE_10PSI = 0;
var PRESSURE_20PSI = 1;
var PRESSURE_60PSI = 2;

// Elevator Types
var SINGLE = 0;
var CASCADING_STAGE_1 = 1;
var CASCADING_STAGE_2 = 2;
var CONTINUOUS_STAGE_1 = 3;
var CONTINUOUS_STAGE_2 = 4;

// Sensor Port Types
var DIO = 1;
var ANALOG = 2;

// Joint Sensor Types
var SENSOR_ENCODER = 1;
var CONVERSION_FACTOR_NAMES = { 1: "Ticks per Revolution" };

function createDriver(_type = DRIVER_MOTOR, _signal = PWM, _portOne = 0, _portTwo = -1)
{
    return { type: _type, signal: _signal, portOne: _portOne, portTwo: _portTwo, wheel: null, pneumatic: null, elevator: null };
}

function createWheel(_type = WHEEL_NORMAL, _frictionLevel = FRICTION_MEDIUM, _isDriveWheel = false)
{
    return { type: _type, frictionLevel: _frictionLevel, isDriveWheel: _isDriveWheel };
}

function createPneumatic(_width = 5.0, _pressure = 40.0)
{
    return { width: _width, pressure: _pressure };
}

function createElevator(_type = SINGLE)
{
    return { type: _type };
}

function createSensor(_type = SENSOR_ENCODER, _signal = DIO, _portA = 0, _portB = 1, _conversionFactor = 1)
{
    return { type: _type, signal: _signal, portA: _portA, portB: _portB, conversionFactor: _conversionFactor };
}
