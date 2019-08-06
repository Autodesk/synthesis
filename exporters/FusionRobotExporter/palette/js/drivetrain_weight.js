/*
WEIGHT PACKET:
{
    value: number,
    unit: 0?1 (0 = METRIC, 1 = IMPERAIL)
}

*/
// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action === 'state')
                {
                    document.getElementById('save-button').innerHTML = "OK";
                    console.log("Receiving DT weight info...");
                    console.log(data);
                    applyConfigData(JSON.parse(data));
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

// Populates the form with sensors
function applyConfigData(dt_weight)
{
    let dtWeight = document.getElementById("dt-weight-value");
    let weightUnit = document.getElementById("dt-weight-unit");
    dtWeight.value = 0;
    if (dt_weight.weight !== undefined) {
        dtWeight.value = dt_weight.weight;
        if (weightUnit.value === "kgs") {
            dtWeight.value *= 0.453592;
        }
    }
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    let data = {"weight": {}};
    data.weight = parseFloat(document.getElementById("dt-weight-value").value);

    if (document.getElementById("dt-weight-unit").value === "kgs") {
        data.weight /= 0.453592;
    }
    return data;
}

// Sends the data to the Fusion add-in
function sendInfoToFusion()
{
    let jsonString = JSON.stringify(readConfigData());
    console.log("Sending JSON to fusion: "+jsonString);
    adsk.fusionSendData('dt_weight_save', jsonString);
}

function cancel() {
    adsk.fusionSendData("close", "drivetrain_weight");
}
