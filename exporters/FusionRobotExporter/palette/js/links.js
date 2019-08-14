window.fusionJavaScriptHandler = {handle: function(action, data) {
    try {
        // any needed implementation
    } catch (e) {
        console.log(e);
        console.log("exception caught with action: " + action + ", data: " + data);
        return "FAILED";
    }
    return "OK";
}};

function openLink(link) {
    adsk.fusionSendData("open_link", link);
}