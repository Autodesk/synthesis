/*
WEIGHT PACKET:
{
    value: number,
    unit: 0?1 (0 = METRIC, 1 = IMPERAIL)
}

*/
var lastPacket = {}
// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action == 'dt_weight_load')
                {
                    console.log("Receiving DT weight info...");
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
function applyConfigData(dt_weight)
{
    lastPacket = dt_weight;
    document.getElementById("dt-weight-value").value = dt_weight.weight.value;
    document.getElementById("dt-weight-unit").value = dt_weight.weight.unit;
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    lastPacket.value = parseFloat(document.getElementById("dt-weight-value").value);
    lastPacket.unit = parseFloat(document.getElementById("dt-weight-unit").value);
    return lastPacket;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    adsk.fusionSendData('dt_weight_save', JSON.stringify(readConfigData()));
}

function cancel() {
    adsk.fusionSendData("close", "drivetrain_weight");
}
