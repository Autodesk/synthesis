var jointOptions = [];

// Used for hiding/showing elements in the following function
function setVisible(element, visible)
{
    element.style.visibility = visible ? 'visible' : 'hidden';
}

// Gets an a single child element that has the class specified
function getElByClass(fieldset, class)
{
    return fieldset.getElementsByClassName(class)[0]
}

// Prompts the Fusion add-in for joint data
function requestInfoFromFusion()
{
    console.log("Requesting joint info...");
    adsk.fusionSendData('send_joints', '');
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
                    jointOptions = processJointDataString(data);
                    displayJointOptions(jointOptions);
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

// Populates the form with joints
function displayJointOptions(joints)
{
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
        fieldset.getElementsByClassName('joint-config-legend')[0].innerHTML = joints[i].name;

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

        // Set joint type
        fieldset.dataset.joint_type = joints[i].type;

        // Show or hide other elements
        updateFieldOptions(fieldset);

        // Add field to form
        exportForm.appendChild(fieldset);
    }
}

// Hides or shows fields based on the values of other fields
function updateFieldOptions(fieldset)
{
    var selectedDriver = parseInt(getElByClass(fieldset, 'driver-type').value);
    var hasDriverDiv = getElByClass(fieldset, 'has-driver-div');

    if (selectedDriver == 0)
        setVisible(hasDriverDiv, false);
    else
    {
        setVisible(hasDriverDiv, true);

        var jointType = parseInt(fieldset.dataset.joint_type);

        var genericPortsDiv = getElByClass(fieldset, 'generic-ports-div');
        setVisible(genericPortsDiv, true);

        // Angular Joint Info
        if ((jointType & JOINT_ANGULAR) == JOINT_ANGULAR)
        {
            // Wheel Info
            var selectedWheel = parseInt(getElByClass(fieldset, 'wheel-type').value);
            var hasWheelDiv = getElByClass(fieldset, 'has-wheel-div');

            if (selectedWheel == 0)
                setVisible(hasWheelDiv, false);
            else
            {
                setVisible(hasWheelDiv, true);

                // Drive wheel
                var isDriveWheel = getElByClass(fieldset, 'is-drive-wheel').checked;
                var driveWheelPortsDiv = getElByClass(fieldset, 'drive-wheel-ports-div');

                if (!isDriveWheel)
                    setVisible(driveWheelPortsDiv, false);
                {
                    setVisible(genericPortsDiv, false);
                    setVisible(driveWheelPortsDiv, true);
                }
            }
        }

        // Linear Joint Info
        if ((jointType & JOINT_LINEAR) == JOINT_LINEAR)
        {
            // TODO: Implement pneumatic info
        }
    }
}

// Updates jointOptions with the currently entered data
function readFormData()
{
    for (var i = 0; i < jointOptions.length; i++)
    {
        var fieldset = document.getElementById('joint-config-' + String(i));

        var selectedDriver = fieldset.getElementsByClassName('driver-type')[0].selectedIndex;

        if (selectedDriver > 0)
        {
            var signal = parseInt(fieldset.getElementsByClassName('port-signal')[0].value);
            var portA = fieldset.getElementsByClassName('port-number-a')[0].selectedIndex;
            var portB = fieldset.getElementsByClassName('port-number-b')[0].selectedIndex;

            jointOptions[i].driver = createDriver(selectedDriver, signal, portA, portB);
            jointOptions[i].driver.signal = signal;
            jointOptions[i].driver.portA = portA;
            jointOptions[i].driver.portB = portB;

            var selectedWheel = fieldset.getElementsByClassName('wheel-type')[0].selectedIndex;

            if (selectedWheel > 0)
            {
                var isDriveWheel = fieldset.getElementsByClassName('is-drive-wheel')[0].checked;
                jointOptions[i].driver.wheel = createWheel(selectedWheel, FRICTION_MEDIUM, isDriveWheel);

                if (isDriveWheel)
                {
                    jointOptions[i].driver.signal = PWM;
                    jointOptions[i].driver.portA = fieldset.getElementsByClassName('wheel-side')[0].selectedIndex;
                    jointOptions[i].driver.portB = fieldset.getElementsByClassName('wheel-side')[0].selectedIndex;
                }
            }
        }
    }
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    var name = document.getElementById('name').value;

    if (name.length == 0)
        return;

    readFormData();
    adsk.fusionSendData('export', stringifyConfigData(name, jointOptions));
}
