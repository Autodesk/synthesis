// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action === 'joints')
                {
                    document.getElementById('finished-button').innerHTML = "Export Robot";
                    console.log("Receiving joint info...");
                    console.log(data);
                    loadConfig(JSON.parse(data));
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

// Populates the form with joints
function loadConfig(configData)
{
    document.getElementById('name').value = configData.name === undefined ? "unnamed" : configData.name;
    document.getElementById('convex').value = configData.convex === undefined ? "BOX" : configData.convex;
}

// Disable submit button if no name entered
function validateForm()
{
    var submitButton = document.getElementById('finished-button');
    var name = document.getElementById('name').value;
    submitButton.disabled = (name.length === 0);
}

// Outputs currently entered data as a JSON object
function saveConfig()
{
    var configData = { 
        'name': document.getElementById('name').value, 
        'convex': parseInt(document.getElementById('convex').value)
    };
    console.log(configData);
    return configData;
}

// Sends the data to the Fusion add-in
function exportRobot()
{
    if (document.getElementById('name').value.length === 0)
    {
        alert("Please enter a name.");
        return;
    }

    // TODO: This is lazy, put this in the JSON
    var openSynthesisCheckbox = document.getElementById('open-synthesis');
    adsk.fusionSendData(openSynthesisCheckbox.checked ? 'export-and-open' : 'export', JSON.stringify(saveConfig()));
}

function cancel() {
    adsk.fusionSendData("close", "export");
}
