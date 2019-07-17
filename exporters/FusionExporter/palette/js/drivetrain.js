var driveType = "tank";

window.fusionJavaScriptHandler = {handle: function(action, data) {
    try {
        if (action === "drivetrain_type") {
            driveType = data;
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
    if (driveType === "tank") {
        document.getElementById("left-highlight").style.visibility = "visible";
    } else if (driveType === "h-drive") {
        document.getElementById("middle-highlight").style.visibility = "visible";
    } else if (driveType === "other") {
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
    adsk.fusionSendData("drivetrain_type", driveType);
}