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
    console.log(state);
    // Delete all existing slots
    lastState = state;
    // UI Setting Goes Here
    var uiRobotName = document.getElementById("robot-name");
    uiRobotName.value = lastState.name;
}


// Outputs currently entered data as a JSON object
function readConfigData()
{
    var uiRobotName = document.getElementById("robot-name");
    // READ UI
    lastState.robotName = uiRobotName.value;
    return lastState;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    adsk.fusionSendData('state_update', JSON.stringify(readConfigData()));
}

function exportRobot(){
    adsk.fusionSendData('export', JSON.stringify(readConfigData()));
}