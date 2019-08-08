var driveType = 1;

window.fusionJavaScriptHandler = {handle: function(action, data) {
    try {
        var parsedConfig = JSON.parse(data);
        if (action === "joints") {
            document.getElementById('save').innerHTML = "OK";
            if (parsedConfig.drivetrainType !== undefined)
                driveType = parsedConfig.drivetrainType;
            unhighlightAll();
            highlightDriveTrain();
        }
    } catch (e) {
        console.log(e);
        console.log("exception caught with action: " + action + ", data: " + data);
        return "FAILED";
    }
    return "OK";
}};

function setDriveTrain(selected) {
    driveType = selected;
    unhighlightAll();
    highlightDriveTrain();
}

function highlightDriveTrain() {
    document.getElementById("highlight-"+driveType).style.visibility = "visible";
}

function unhighlightAll() {
    Array.from(document.getElementsByClassName("highlight")).forEach(element => {
        element.style.visibility = "hidden";
    });
}

function sendInfoToFusion() {
    adsk.fusionSendData("drivetrain_type", JSON.stringify({"drivetrainType": driveType}));
}

function cancel() {
    adsk.fusionSendData("close", "drivetrain_type");
}