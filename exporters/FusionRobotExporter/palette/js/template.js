// Handles the receiving of data from Fusion
var lastState = {};



window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action == 'state')
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

// Populates the form with sensors
function applyConfigData(state)
{
    // Delete all existing slots
    lastState = state;
    // UI Setting Goes Here
}


// Outputs currently entered data as a JSON object
function readConfigData()
{
    var configData = {};

    // READ UI

    //

    lastState.sensors = configData;
    return lastState;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    adsk.fusionSendData('state_update', JSON.stringify(readConfigData()));
}
