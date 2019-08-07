var noJoints = false;
let openFieldsetSensors = null;

// Prompts the Fusion add-in for joint data
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
    const jointTypeComboBox = getElByClass(fieldset, 'joint-type');
    if (jointTypeComboBox.value === "none") return;
    const sensors = JSON.parse(fieldset.dataset.sensors);
    sensors.isDrivetrain = jointTypeComboBox.value === "drivetrain";
    adsk.fusionSendData('edit_sensors', JSON.stringify(sensors));
}

// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action === 'joints')
                {
                    document.getElementById('save-button').innerHTML = "OK";
                    const configData = JSON.parse(data);
                    console.log("Input Joint Data Data: ", configData);
                    loadData(configData);
                }
                else if (action === 'sensors')
                {
                    console.log("Receiving sensor info...");
                    console.log(data);
                    if (openFieldsetSensors != null)
                        openFieldsetSensors.dataset.sensors = data;
                }
                else if (action === 'debugger')
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

const delayHover = function (elem, callback) {
    let timeout = null;
    elem.onmouseover = function () {
        timeout = setTimeout(callback, 500);
    };

    elem.onmouseout = function () {
        clearTimeout(timeout);
    }
};

// Populates the form with joints
function loadData(configData)
{
    const joints = configData.joints;

    // Delete all existing slots
    const existing = document.getElementsByClassName('joint-config');
    while (existing.length > 0)
        existing[0].parentNode.removeChild(existing[0]);

    noJoints = joints.length === 0;
    if (noJoints)
        document.getElementById("nodata").style.display = "none";
    setVisible(document.getElementById("nodata"), noJoints);
    setVisible(document.getElementById("save-button"), !noJoints);
    if (noJoints) return;

    // Add slots for given joint
    const template = jointTemplate;
    const exportForm = document.getElementById('export-settings');

    for (let i = 0; i < joints.length; i++)
    {
        const fieldset = template.cloneNode(true);

        fieldset.id = 'joint-config-' + String(i);
        fieldset.dataset.jointId = joints[i].id;
        fieldset.dataset.asBuilt = joints[i].asBuilt ? 'true' : 'false';
        
        // TODO: This is lazy, don't do this
        let gearRatio = 0;
        let signal = 2;
        let portOne = 3;
        
        if (joints[i].driver) {
            gearRatio = joints[i].driver.outputGear === undefined || joints[i].driver.inputGear === undefined ? 0 : joints[i].driver.outputGear / joints[i].driver.inputGear;
            signal = joints[i].driver.signal;
            portOne = joints[i].driver.portOne;
        }
        fieldset.dataset.sensors = JSON.stringify({'sensors': joints[i].sensors, 'gear': gearRatio, 'signal': signal, 'portOne': portOne});

        // Highlight joint if hover for 0.5 seconds
        (function(id){delayHover(fieldset, function() {highlightJoint(id)})}(fieldset.dataset.jointId));

        const jointTitle = getElByClass(fieldset, 'joint-config-legend');
        jointTitle.innerHTML = joints[i].name;
        jointTitle.dataset.jointId = joints[i].id;

        const jointImage = getElByClass(fieldset, 'joint-config-image');
        jointImage.setAttribute("src",configData.tempIconDir+i+".png");

        const jointTypeComboBox = getElByClass(fieldset, 'joint-type');
        const dtSideComboBox = getElByClass(fieldset, 'dt-side');
        const wheelTypeComboBox = getElByClass(fieldset, 'wheel-type');
        const weightInput = getElByClass(fieldset, 'weight');
        const driverTypeComboBox = getElByClass(fieldset, 'driver-type');

        // Joint type
        let isAngular = (joints[i].type & JOINT_ANGULAR) === JOINT_ANGULAR;
        let isLinear = (joints[i].type & JOINT_LINEAR) === JOINT_LINEAR;
        
        // jointTypeComboBox options
        setVisible(jointTypeComboBox.options[1], isAngular);
        
        // driverTypeComboBox options
        Array.from(fieldset.getElementsByClassName('angular-driver')).forEach(object => setVisible(object, isAngular));
        Array.from(fieldset.getElementsByClassName('linear-driver')).forEach(object => setVisible(object, isLinear));
        
        // Defaults
        weightInput.value = 0;
        dtSideComboBox.selectedIndex = 0;
        wheelTypeComboBox.selectedIndex = 0;
        driverTypeComboBox.selectedIndex = isAngular ? 0 : 1; // TODO: Figure out how to select first enabled option
        
        const jointDriver = joints[i].driver;
        if (jointDriver === null) {
            jointTypeComboBox.value = 'none';
        } else if (jointDriver.portOne <= 2) {
            jointTypeComboBox.value = 'drivetrain';
            dtSideComboBox.value = jointDriver.portOne;
            if (jointDriver.wheel != null)
                getElByClass(fieldset, 'wheel-type').value = jointDriver.wheel.type;
        } else {
            jointTypeComboBox.value = 'mechanism';
            weightInput.value = 0; // TODO: Per joint weight loading
            driverTypeComboBox.value = jointDriver.type;
        }
        // Set joint type
        fieldset.dataset.joint_type = joints[i].type;

        doLayout(fieldset);
        exportForm.appendChild(fieldset);
    }
}

// Hides or shows fields based on the values of other fields
function doLayout(fieldset)
{
    // Get the parent node that is the fieldset
    while (fieldset != null && !fieldset.classList.contains('joint-config'))
        fieldset = fieldset.parentNode;

    if (fieldset == null)
        return;

    const jointTypeDiv = getElByClass(fieldset, 'joint-type');
    const jointType = jointTypeDiv.value;
    const drivetrainDiv = getElByClass(fieldset, 'drivetrain-options');
    const mechanismDiv = getElByClass(fieldset, 'mechanism-options');
    const advancedButton = getElByClass(fieldset, 'edit-sensors-button');

    jointTypeDiv.style.background = jointType === 'none' ? 'rgb(255, 153, 0)' : 'white';

    if (jointType === "none") {
        setVisible(drivetrainDiv, false);
        setVisible(mechanismDiv, false);
        setVisible(advancedButton, false);
    } else {
        setVisible(advancedButton, true);
        if (jointType === "drivetrain") {
            setVisible(drivetrainDiv, true);
            setVisible(mechanismDiv, false);
        } else {
            setVisible(drivetrainDiv, false);
            setVisible(mechanismDiv, true);
        }
    }
}

// Outputs currently entered data as a JSON object
function saveValues()
{
    const configData = {};
    const joints = [];

    const jointOptions = document.getElementsByClassName('joint-config');

    for (let i = 0; i < jointOptions.length; i++)
    {
        const fieldset = jointOptions[i];

        let advanced = JSON.parse(fieldset.dataset.sensors);
        const joint = {
            'driver': null,
            'id': fieldset.dataset.jointId,
            'asBuilt': fieldset.dataset.asBuilt === 'true',
            'name': getElByClass(fieldset, 'joint-config-legend').innerHTML,
            'type': parseInt(fieldset.dataset.joint_type),
            'sensors': advanced.sensors
        };

        const jointTypeComboBox = getElByClass(fieldset, 'joint-type');
        const dtSideComboBox = getElByClass(fieldset, 'dt-side');
        const wheelTypeComboBox = getElByClass(fieldset, 'wheel-type');
        const weightInput = getElByClass(fieldset, 'weight');
        const driverTypeComboBox = getElByClass(fieldset, 'driver-type');

        if (jointTypeComboBox.value === 'none') {
            joint.driver = null;
            // joint.weight = 0; // TODO: per joint weight
        } else {
            const selectedDriver = jointTypeComboBox.value === 'drivetrain' ? DRIVER_MOTOR : parseInt(driverTypeComboBox.value);
            joint.driver = createDriver(selectedDriver, CAN, 3, 3);
            // joint.driver.hasBrake = true;
            // joint.driver.motor = MotorType.GENERIC;
            // joint.driver.InputGear = 1;
            // joint.driver.OutputGear = advancedSettingsForm.GearRatio;
            joint.driver.inputGear = 1;
            joint.driver.outputGear = advanced.gear;
            // joint.driver.lowerLimit = 0;
            // joint.driver.upperLimit = 0;
            
            if (jointTypeComboBox.value === 'drivetrain') {
                
                // Port/wheel side
                joint.driver.portOne = parseInt(dtSideComboBox.value);
                joint.driver.portTwo = parseInt(dtSideComboBox.value);
                joint.driver.signal = CAN;

                // Wheel type
                const selectedWheel = parseInt(wheelTypeComboBox.value);
                joint.driver.wheel = createWheel(selectedWheel, FRICTION_MEDIUM, true);
                
                // Weight
                // joint.weight = 0; // TODO: Per joint weight
                
            } else if (jointTypeComboBox.value === 'mechanism') {
                // Port/wheel side
                joint.driver.portOne = advanced.portOne;
                joint.driver.portTwo = 0;
                if (joint.driver.portOne <= 2)
                    joint.driver.portOne = 2;
                joint.driver.signal = advanced.signal;

                // Wheel driver
                joint.driver.wheel = null;
                
                if (selectedDriver === DRIVER_ELEVATOR) {
                    joint.driver.elevator = createElevator(SINGLE)
                }

                // Weight
                // joint.weight = (double) weightInput.Value; // TODO: Per joint weight
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
    const result = JSON.stringify(saveValues());
    console.log("Saving Data: ", result);
    adsk.fusionSendData('save', result);
}

// Sends the data to the Fusion add-in
function cancel() {
    adsk.fusionSendData("close", "joint_editor");
}
