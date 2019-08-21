// weight unit: 0 = METRIC, 1 = IMPERIAL
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

function applyConfigData(dt_weight)
{
    let dtWeight = document.getElementById("weight-value");
    let weightUnit = document.getElementById("weight-unit");
    dtWeight.value = 0;
    if (dt_weight.weight !== undefined) {
        dtWeight.value = dt_weight.weight;
        if (weightUnit.value === "kgs") {
            dtWeight.value *= 0.453592;
        }
    }
    dtWeight.value = Math.round(dtWeight.value * 100) / 100
}

// Outputs currently entered data as a JSON object
function readConfigData()
{
    let data = {"weight": {}};
    data.weight = parseFloat(document.getElementById("weight-value").value);

    if (document.getElementById("weight-unit").value === "kgs") {
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
