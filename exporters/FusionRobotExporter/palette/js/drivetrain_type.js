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
    document.getElementById("drivetrain-" + driveType).style.boxShadow = "0 3000px rgba(245, 156, 66, 0.4) inset";
}

function unhighlightAll() {
    document.getElementById("drivetrain-1").style.boxShadow = "none";
    document.getElementById("drivetrain-2").style.boxShadow = "none";
    document.getElementById("drivetrain-3").style.boxShadow = "none";
}

function sendInfoToFusion() {
    adsk.fusionSendData("drivetrain_type", JSON.stringify({"drivetrainType": driveType}));
}

function cancel() {
    adsk.fusionSendData("close", "drivetrain_type");
}