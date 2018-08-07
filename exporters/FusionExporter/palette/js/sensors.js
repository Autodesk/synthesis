// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action == 'sensors')
                {
                    console.log("Receiving sensor info...");
                    console.log(data);
                    applyConfigData(JSON.parse(data));
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

function newSensor()
{
    var fieldset = sensorTemplate.cloneNode(true);

    fieldset.id = 'sensor-config-' + String(document.getElementsByClassName('sensor-config').length);

    // Add field to form
    updateFieldOptions(fieldset);
    document.getElementById('sensor-settings').appendChild(fieldset);
}

function deleteSensor(fieldset)
{
    fieldset.parentNode.removeChild(fieldset);
}

// Populates the form with sensors
function applyConfigData(sensors)
{
    // Delete all existing slots
    var existing = document.getElementsByClassName('sensor-config');
    while (existing.length > 0)
        existing[0].parentNode.removeChild(existing[0]);

    // Add slots for given sensors
    var template = sensorTemplate;
    var sensorForm = document.getElementById('sensor-settings');

    for (var i = 0; i < sensors.length; i++)
    {
        var fieldset = template.cloneNode(true);

        getElByClass(fieldset, 'sensor-type').value = sensors[i].type;
        getElByClass(fieldset, 'port-signal').value = sensors[i].signal;
        getElByClass(fieldset, 'port-number-a').value = sensors[i].portA;
        getElByClass(fieldset, 'port-number-b').value = sensors[i].portB;
        getElByClass(fieldset, 'conversion-factor').value = sensors[i].conversionFactor;
        
        // Add field to form
        updateFieldOptions(fieldset);
        sensorForm.appendChild(fieldset);
    }
}

// Change the conversion factor name based on the selected sensor
function updateFieldOptions(fieldset)
{
    var type = parseInt(getElByClass(fieldset, 'sensor-type').value);

    getElByClass(fieldset, 'conversion-factor-label').innerHTML = CONVERSION_FACTOR_NAMES[type];
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    var configData = [];

    var sensorOptions = document.getElementsByClassName('sensor-config');

    for (var i = 0; i < sensorOptions.length; i++)
    {
        var fieldset = sensorOptions[i];

        var type = parseInt(getElByClass(fieldset, 'sensor-type').value);
        var portSignal = parseInt(getElByClass(fieldset, 'port-signal').value);
        var portA = parseInt(getElByClass(fieldset, 'port-number-a').value);
        var portB = parseInt(getElByClass(fieldset, 'port-number-b').value);
        var conversionFactor = parseFloat(getElByClass(fieldset, 'conversion-factor').value);

        var sensor = createSensor(type, portSignal, portA, portB, conversionFactor);

        configData.push(sensor);
    }
    
    return configData;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    adsk.fusionSendData('save_sensors', JSON.stringify(readConfigData()));
}
