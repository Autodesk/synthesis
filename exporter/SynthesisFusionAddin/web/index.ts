declare global {
    interface Window {
        adsk: {
            fusionSendData(action:string, body:string):Promise<string>
        };
        fusionJavaScriptHandler: {
            /* The return value is a string and is passed back to your add-in as the return argument of the sendInfoToHTML method. Returning an empty string is interpreted as an error, so you should always return something in both success and failure cases.*/
            handle(action:string, body:string):string
        }
        initiateSelection(type:SelectionFilter):void
    }
}
export {}

console.log("TEST")

async function sendData(action:string, body:any):Promise<any|undefined> {
    const resp = await window.adsk.fusionSendData(action, JSON.stringify(body));
    try {
        return JSON.parse(resp)
    } catch (error) {
        console.error(error)
        return undefined
    }
}


window.initiateSelection = async function(t){
    const id = await sendData("selectJoint", {msg:`Selecting ${t}`})
    console.log(id)
    alert("Selected")
}


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
        await sendData("export", Object.fromEntries(data));
    });
});