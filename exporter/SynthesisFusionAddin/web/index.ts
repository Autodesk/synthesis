declare global {
    interface Window {
        adsk: {
            fusionSendData(action:string, body:string):Promise<string>
        };
        fusionJavaScriptHandler: {
            handle(action:string, body:string):string
        }
        sendInfoToFusion():void
    }
}
export {}

console.log("TEST")

function getDateString() {
    const today = new Date();
    const date = `${today.getDate()}/${today.getMonth() + 1}/${today.getFullYear()}`;
    const time = `${today.getHours()}:${today.getMinutes()}:${today.getSeconds()}`;
    return `Date: ${date}, Time: ${time}`;
}


function sendInfoToFusion() {
    const args = {
        arg1: (document.getElementById("sampleData") as HTMLInputElement).value,
        arg2: getDateString()
    };

    // Send the data to Fusion as a JSON string. The return value is a Promise.
    window.adsk.fusionSendData("messageFromPalette", JSON.stringify(args)).then((result) =>
        document.getElementById("returnValue")!.innerHTML = `${result}`
    );

}

window.sendInfoToFusion = sendInfoToFusion;

function updateMessage(messageString:string) {
    // Message is sent from the add-in as a JSON string.
    const messageData = JSON.parse(messageString);

    // Update a paragraph with the data passed in.
    document.getElementById("fusionMessage")!.innerHTML =
        `<b>Your text</b>: ${messageData.myText} <br/>` +
        `<b>Your expression</b>: ${messageData.myExpression} <br/>` +
        `<b>Your value</b>: ${messageData.myValue}`;
}

window.fusionJavaScriptHandler = {
    handle: function (action, data) {
        try {
            if (action === "updateMessage") {
                updateMessage(data);
            } else if (action === "debugger") {
                debugger;
            } else {
                return `Unexpected command type: ${action}`;
            }
        } catch (e) {
            console.log(e);
            console.log(`Exception caught with command: ${action}, data: ${data}`);
        }
        return "OK";
    },
};

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("settings") as HTMLFormElement
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const data = new FormData(form);
        await window.adsk.fusionSendData("export", JSON.stringify(Object.fromEntries(data)));
    });
});