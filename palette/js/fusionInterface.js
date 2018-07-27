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

function stringifyConfigData(name, joints)
{
    var ASCII_OFFSET = 32;

    var args = String.fromCharCode(name.length) + String(name);

    for (var i = 0; i < joints.length; i++)
    {
        var driver = joints[i].driver;

        if (driver == null)
            args += String.fromCharCode(ASCII_OFFSET + 0);
        else
        {
            args += String.fromCharCode(ASCII_OFFSET + 1);
            args += String.fromCharCode(ASCII_OFFSET + driver.type);
            args += String.fromCharCode(ASCII_OFFSET + driver.signal);
            args += String.fromCharCode(ASCII_OFFSET + driver.portA + 1);
            args += String.fromCharCode(ASCII_OFFSET + driver.portB + 1);

            var wheel = driver.wheel;

            if (wheel == null)
                args += String.fromCharCode(ASCII_OFFSET + 0);
            else
            {
                args += String.fromCharCode(ASCII_OFFSET + 1);
                args += String.fromCharCode(ASCII_OFFSET + wheel.type);
                args += String.fromCharCode(ASCII_OFFSET + wheel.frictionLevel);
                args += String.fromCharCode(ASCII_OFFSET + (wheel.isDriveWheel ? 1 : 0));
            }
        }
    }

    return args;
}

function processJointDataString(jointData)
{
    var joints = [];

    for (var c = 0; c < jointData.length; c++)
    {
        var nameLength = '';
        while (jointData[c] != ' ' && c < jointData.length)
            nameLength += jointData[c++];
        c++;

        var nameStr = '';
        for (var i = 0; i < parseInt(nameLength) && c < jointData.length; i++)
            nameStr += jointData[c++];

        joints.push({ name: nameStr, driver: null });
    }

    return joints;
}

function createDriver(_type = DRIVER_MOTOR, _signal = PWM, _portA = 0, _portB = -1)
{
    return {type: _type, signal: _signal, portA: _portA, portB: _portB, wheel: null};
}

function createWheel(_type = WHEEL_NORMAL, _frictionLevel = FRICTION_MEDIUM, _isDriveWheel = false)
{
    return { type: _type, frictionLevel: _frictionLevel, isDriveWheel: _isDriveWheel };
}
