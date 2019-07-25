var openFieldsetSensors = null;

// Used for hiding/showing elements in the following function
function setPortView(fieldset, portView)
{
    var motorPorts = Array.from(fieldset.getElementsByClassName('motor-port'));
    var pneumaticPorts = Array.from(fieldset.getElementsByClassName('pneumatic-port'));
    var relayPorts = Array.from(fieldset.getElementsByClassName('relay-port'));

    var toHide = [];
    var toShow = [];

    // Hide ports based on view
    if (portView == "pneumatic")
    {
        toHide = toHide.concat(motorPorts);
        toHide = toHide.concat(relayPorts);
        toShow = toShow.concat(pneumaticPorts);
    }
    else if (portView == "relay")
    {
        toHide = toHide.concat(motorPorts);
        toHide = toHide.concat(pneumaticPorts);
        toShow = toShow.concat(relayPorts);
    }
    else
    {
        toHide = toHide.concat(pneumaticPorts);
        toHide = toHide.concat(relayPorts);
        toShow = toShow.concat(motorPorts);
    }

    for (var i = 0; i < toHide.length; i++)
        toHide[i].style.display = 'none';
    for (var i = 0; i < toShow.length; i++)
        toShow[i].style.display = '';

    getElByClass(fieldset, 'port-signal').value = toShow[0].value;
}

// Prompts the Fusion add-in for joint data
function requestInfoFromFusion()
{
    console.log('Requesting joint info...');
    adsk.fusionSendData('send_joints', '');
}

// Highlight a joint in Fusion
function highlightJoint(jointID)
{
    console.log('Highlighting ' + jointID);
    adsk.fusionSendData('highlight', jointID);
}

// Open a menu for editing joint sensors
function editSensors(fieldset)
{
    openFieldsetSensors = fieldset;
    adsk.fusionSendData('edit_sensors', fieldset.dataset.sensors);
}

// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action == 'joints')
                {
                    console.log("Receiving joint info...");
                    console.log(data);
                    applyConfigData(JSON.parse(data));
                }
                else if (action == 'sensors')
                {
                    console.log("Receiving sensor info...");
                    console.log(data);
                    if (openFieldsetSensors != null)
                        openFieldsetSensors.dataset.sensors = data;
                }
                else if (action == 'fieldNames')
                {
                    console.log("Receiving field names...");
                    console.log(data);
                    applyFieldList(JSON.parse(data))
                }
                else if (action == 'debugger')
                {
                    debugger;
                }
                else
                {
                    return 'Unexpected command type: ' + action;
                }
            }
            catch (e)
            {
                console.log('Exception while excecuting \"' + action + '\":');
                console.log(e);
            }
            return 'OK';
        }
    };

var delayHover = function (elem, callback, hoverTime) {
    var timeout = null;
    elem.onmouseover = function() {
        timeout = setTimeout(callback, hoverTime);
    };

    elem.onmouseout = function() {
        clearTimeout(timeout);
    }
};

// Populates the form with joints
function applyFieldList(configData) {
    var fieldOptions = document.getElementById("frc-field-options");
    fieldOptions.options.length = 0;
    for(var thing in configData) {
        fieldOptions.options[fieldOptions.options.length] = new Option(configData[thing], configData[thing]);
    }
}
// Populates the form with joints
function applyConfigData(configData)
{
    document.getElementById('name').value = configData.name;

    var joints = configData.joints;

    // Delete all existing slots
    var existing = document.getElementsByClassName('joint-config');
    while (existing.length > 0)
        existing[0].parentNode.removeChild(existing[0]);

    // Add slots for given joinst
    var template = jointTemplate;
    var exportForm = document.getElementById('export-settings');

    for (var i = 0; i < joints.length; i++)
    {
        var fieldset = template.cloneNode(true);

        fieldset.id = 'joint-config-' + String(i);
        fieldset.dataset.jointId = joints[i].id;
        fieldset.dataset.asBuilt = joints[i].asBuilt ? 'true' : 'false';
        fieldset.dataset.sensors = JSON.stringify(joints[i].sensors);

        // Highlight joint if hover for 0.5 seconds
        (function(id){delayHover(fieldset, function () {
            highlightJoint(id)
        }, 200)}(fieldset.dataset.jointId));

        var jointTitle = getElByClass(fieldset, 'joint-config-legend');
        jointTitle.innerHTML = joints[i].name;
        jointTitle.dataset.jointId = joints[i].id;

        var jointImage = getElByClass(fieldset, 'joint-config-image');
        jointImage.setAttribute("src",configData.tempIconDir+i+".png");
        

        // Filter for angular or linear joints
        var angularJointDiv = getElByClass(fieldset, 'angular-joint-div');
        var linearJointDiv = getElByClass(fieldset, 'linear-joint-div');
        var elsToHide = [];

        if ((joints[i].type & JOINT_ANGULAR) != JOINT_ANGULAR)
        {
            elsToHide = elsToHide.concat(Array.from(fieldset.getElementsByClassName('angular-driver')));
            elsToHide.push(angularJointDiv);
        }

        if ((joints[i].type & JOINT_LINEAR) != JOINT_LINEAR)
        {
            elsToHide = elsToHide.concat(Array.from(fieldset.getElementsByClassName('linear-driver')));
            elsToHide.push(linearJointDiv);
        }

        for (var j = 0; j < elsToHide.length; j++)
            elsToHide[j].style.display = 'none';

        // Hide sensors button if joint is not angular
        if ((joints[i].type & JOINT_LINEAR) == JOINT_LINEAR)
            setVisible(getElByClass(fieldset, 'edit-sensors-button'), false);

        // Set joint type
        fieldset.dataset.joint_type = joints[i].type;

        // Apply any existing configuration
        applyDriverData(joints[i].driver, fieldset);

        // Show or hide other elements
        updateFieldOptions(fieldset);

        // Add field to form
        exportForm.appendChild(fieldset);
    }
}

// Applies existing driver configuration to a field
function applyDriverData(driver, fieldset)
{
    if (driver == null)
        return;

    getElByClass(fieldset, 'driver-type').value = driver.type;
    getElByClass(fieldset, 'port-signal').value = driver.signal;
    getElByClass(fieldset, 'port-number-one').value = driver.portOne;
    getElByClass(fieldset, 'port-number-two').value = driver.portTwo;

    if (driver.wheel != null)
    {
        getElByClass(fieldset, 'wheel-type').value = driver.wheel.type;
        getElByClass(fieldset, 'is-drive-wheel').checked = driver.wheel.isDriveWheel;

        if (driver.wheel.isDriveWheel)
            getElByClass(fieldset, 'wheel-side').value = driver.portOne;
    }

    if (driver.pneumatic != null)
    {
        getElByClass(fieldset, 'pneumatic-width').value = driver.pneumatic.width;
        getElByClass(fieldset, 'pneumatic-pressure').value = driver.pneumatic.pressure;
    }

    if (driver.type == DRIVER_ELEVATOR && driver.elevator != null)
    {
        getElByClass(fieldset, 'elevator-type').value = driver.elevator.type;
    }
}

// Disable submit button if no name entered
function updateSubmitButton()
{
    var submitButton = document.getElementById('finished-button');
    var name = document.getElementById('name').value;

    document.getElementById('finished-button').disabled = (name.length == 0);
}

// Hides or shows fields based on the values of other fields
function updateFieldOptions(fieldset)
{
    // Get the parent node that is the fieldset
    while (fieldset != null && !fieldset.classList.contains('joint-config'))
        fieldset = fieldset.parentNode;

    if (fieldset == null)
        return;

    // Update view of fieldset
    var driverType = parseInt(getElByClass(fieldset, 'driver-type').value);
    var hasDriverDiv = getElByClass(fieldset, 'has-driver-div');

    if (driverType == 0)
        setVisible(hasDriverDiv, false);
    else
    {
        setVisible(hasDriverDiv, true);

        var jointType = parseInt(fieldset.dataset.joint_type);

        var angularJointDiv = getElByClass(fieldset, 'angular-joint-div');
        var linearJointDiv = getElByClass(fieldset, 'linear-joint-div');

        var genericPortsDiv = getElByClass(fieldset, 'generic-ports-div');
        var portTwoSelector = getElByClass(fieldset, 'port-number-two');
        setVisible(genericPortsDiv, true);
        setVisible(portTwoSelector, false);
        
        setPortView(fieldset, 'motor');

        // Angular Joint Info
        if ((jointType & JOINT_ANGULAR) == JOINT_ANGULAR)
        {
            if (driverType == DRIVER_DUAL_MOTOR)
                setVisible(portTwoSelector, true);

            // Wheel Info
            var selectedWheel = parseInt(getElByClass(fieldset, 'wheel-type').value);
            var hasWheelDiv = getElByClass(fieldset, 'has-wheel-div');

            if (selectedWheel == 0)
            {
                setVisible(hasWheelDiv, false);
                setVisible(linearJointDiv, true);
            }
            else
            {
                setVisible(hasWheelDiv, true);
                setVisible(linearJointDiv, false);

                // Drive wheel
                var isDriveWheel = getElByClass(fieldset, 'is-drive-wheel').checked;
                var driveWheelPortsDiv = getElByClass(fieldset, 'wheel-side');
                
                if (!isDriveWheel)
                    setVisible(driveWheelPortsDiv, false);
                else
                {
                    setVisible(driveWheelPortsDiv, true);
                    setVisible(genericPortsDiv, false);
                }
            }
        }

        // Linear Joint Info
        if ((jointType & JOINT_LINEAR) == JOINT_LINEAR)
        {
            var pneumaticDiv = getElByClass(fieldset, 'pneumatic-div');
            var elevatorDiv = getElByClass(fieldset, 'has-elevator-div');

            if (driverType != DRIVER_BUMPER_PNEUMATIC &&
                driverType != DRIVER_RELAY_PNEUMATIC)
            {
                setVisible(pneumaticDiv, false);

                if (driverType == DRIVER_ELEVATOR)
                    setVisible(elevatorDiv, true);
                else
                    setVisible(elevatorDiv, false);
            }
            else
            {
                if (driverType == DRIVER_BUMPER_PNEUMATIC)
                {
                    setVisible(portTwoSelector, true);
                    setPortView(fieldset, 'pneumatic');
                }
                else
                    setPortView(fieldset, 'relay');

                setVisible(pneumaticDiv, true);
                setVisible(elevatorDiv, false);
            }
        }
    }
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    var configData = { 'name': document.getElementById('name').value };
    var joints = [];

    var jointOptions = document.getElementsByClassName('joint-config');

    for (var i = 0; i < jointOptions.length; i++)
    {
        var fieldset = jointOptions[i];

        var joint = {
            'driver': null,
            'id': fieldset.dataset.jointId,
            'asBuilt': fieldset.dataset.asBuilt == 'true',
            'name': getElByClass(fieldset, 'joint-config-legend').innerHTML,
            'type': parseInt(fieldset.dataset.joint_type),
            'sensors': JSON.parse(fieldset.dataset.sensors)
        };

        var selectedDriver = parseInt(fieldset.getElementsByClassName('driver-type')[0].value);

        if (selectedDriver > 0)
        {
            var signal = parseInt(getElByClass(fieldset, 'port-signal').querySelector('option:checked').dataset.portValue);
            var portOne = parseInt(getElByClass(fieldset, 'port-number-one').value);
            var portTwo = parseInt(getElByClass(fieldset, 'port-number-two').value);

            joint.driver = createDriver(selectedDriver, signal, portOne, portTwo);
            
            if ((joint.type & JOINT_ANGULAR) == JOINT_ANGULAR)
            {
                var selectedWheel = parseInt(getElByClass(fieldset, 'wheel-type').value);

                if (selectedWheel > 0)
                {
                    var isDriveWheel = getElByClass(fieldset, 'is-drive-wheel').checked;
                    joint.driver.wheel = createWheel(selectedWheel, FRICTION_MEDIUM, isDriveWheel);

                    if (isDriveWheel)
                    {
                        joint.driver.signal = PWM;
                        joint.driver.portOne = parseInt(getElByClass(fieldset, 'wheel-side').value);
                        joint.driver.portTwo = parseInt(getElByClass(fieldset, 'wheel-side').value);
                    }
                }
            }

            if ((joint.type & JOINT_LINEAR) == JOINT_LINEAR)
            {
                if (selectedDriver == DRIVER_BUMPER_PNEUMATIC ||
                    selectedDriver == DRIVER_RELAY_PNEUMATIC)
                {
                    var width = parseInt(getElByClass(fieldset, 'pneumatic-width').value);
                    var pressure = parseInt(getElByClass(fieldset, 'pneumatic-pressure').value);

                    joint.driver.pneumatic = createPneumatic(width, pressure);
                }
                else if (selectedDriver == DRIVER_ELEVATOR)
                {
                    var type = parseInt(getElByClass(fieldset, 'elevator-type').value);

                    joint.driver.elevator = createElevator(type);
                }
            }
        }

        joints.push(joint);
    }

    configData.joints = joints;
    console.log(configData);
    return configData;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    if (document.getElementById('name').value.length == 0)
    {
        alert("Please enter a name.");
        return;
    }
    
    adsk.fusionSendData('save', JSON.stringify(readConfigData()));
}

// Sends the data to the Fusion add-in
function exportRobot()
{
    if (document.getElementById('name').value.length == 0)
    {
        alert("Please enter a name.");
        return;
    }

    adsk.fusionSendData('export', JSON.stringify(readConfigData()));
}
