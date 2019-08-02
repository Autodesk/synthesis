var settings = { analytics: true, guide: true }; // properties to be expanded later on

function updateView() {
    "use strict";
    document.getElementById("guideCheckbox").checked = settings.guide; // saves the value
    document.getElementById("analyticsCheckbox").checked = settings.analytics; // saves the value
}

window.fusionJavaScriptHandler = {handle: function(action, data) {
    "use strict";
    try {
        if (action === "settings_analytics") {
            settings.analytics = (data === 'true'); // convert string to boolean
            updateView();
        } else if (action === "settings_guide") {
            settings.guide = (data === 'true'); // convert string to boolean
            updateView();
        }
    } catch (e) {
        console.log(e);
        console.log("exception caught with action: " + action + ", data: " + data);
        return "FAILED";
    }
    return "OK";
}};

function saveSettings() {
    "use strict";
    settings.analytics = document.getElementById("analyticsCheckbox").checked;
    settings.guide = document.getElementById("guideCheckbox").checked;
}

function sendInfoToFusion() {
    saveSettings();
    adsk.fusionSendData("settings_analytics", settings.analytics.toString());
    adsk.fusionSendData("settings_guide", settings.guide.toString());
}
