var driveType = 1;

window.fusionJavaScriptHandler = {handle: function(action, data) {
    try {
        var parsedConfig = JSON.parse(data);
        if (action === "joints") {
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
    if (driveType === 1) {
        document.getElementById("left-highlight").style.visibility = "visible";
    } else if (driveType === 2) {
        document.getElementById("middle-highlight").style.visibility = "visible";
    } else if (driveType === 3) {
        document.getElementById("right-highlight").style.visibility = "visible";
    } else {
        document.getElementById("left-highlight").style.visibility = "visible";
    }
}

function unhighlightAll() {
    document.getElementById("left-highlight").style.visibility = "hidden";
    document.getElementById("middle-highlight").style.visibility = "hidden";
    document.getElementById("right-highlight").style.visibility = "hidden";
}

function sendInfoToFusion() {
    adsk.fusionSendData("drivetrain_type", JSON.stringify({"drivetrainType": driveType}));
}