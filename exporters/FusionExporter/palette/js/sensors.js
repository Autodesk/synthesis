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
                    //console.log(data);
                    //applyConfigData(JSON.parse(data));
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
    document.getElementById('sensor-settings').appendChild(fieldset);
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

        fieldset.id = 'sensor-config-' + String(i);

        // Add field to form
        sensorForm.appendChild(fieldset);
    }
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    var configData = [];

    var jointOptions = document.getElementsByClassName('joint-config');

    for (var i = 0; i < jointOptions.length; i++)
    {
        var fieldset = jointOptions[i];

        var sensor = {};

        configData.push(sensor);
    }
    
    return configData;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    adsk.fusionSendData('save_sensors', JSON.stringify(readConfigData()));
}
