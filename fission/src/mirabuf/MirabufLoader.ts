import { mirabuf } from "../proto/mirabuf";
import Pako from "pako";
import * as fs from 'fs';

const root = await navigator.storage.getDirectory();
const miraFolderHandle = await root.getDirectoryHandle("Mira", { create: true })
const robotFolderHandle = await miraFolderHandle.getDirectoryHandle("Robots", { create: true })
const fieldFolderHandle = await miraFolderHandle.getDirectoryHandle("Fields", { create: true })

export function UnzipMira(buff: Uint8Array): Uint8Array {
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff);
    } else {
        return buff;
    }
}
export async function LoadMirabufRemote(fetchLocation: string, useCache: boolean = true): Promise<mirabuf.Assembly | undefined> {

    let target;
    let isRobot;
    if (fetchLocation.includes("Robot")) {
        target = fetchLocation.substring(fetchLocation.lastIndexOf("Robot"))
        isRobot = true
    } else {
        target = fetchLocation.substring(fetchLocation.lastIndexOf("Field"))
        isRobot = false
    }

    console.log(target)



    const miraJSON = window.localStorage.getItem("Mira") //returning object?? Parsed json?? Despite the docs saying returning key string
    let cachedMira;
    let targetID;
    let assembly;



    if (!miraJSON?.includes("[object Object]")) { // Goes to else first time
        console.log("miraJSON found")
        const hi = JSON.stringify(window.localStorage.getItem("Mira"))
        console.log(hi)


        cachedMira = JSON.parse(window.localStorage.getItem("Mira") ?? "{}")

        targetID = cachedMira[target]
        console.log(targetID)
        
        if (targetID) {
            console.log("targetID found")
            // Grab file OPFS if targetID exists
            let fileHandle;
            try {
                if (isRobot) {
                    fileHandle = await robotFolderHandle.getFileHandle(targetID, {create: false}) ?? false
                } else {
                    fileHandle = await fieldFolderHandle.getFileHandle(targetID, {create: false}) ?? false
                }
            } catch (e) {
                console.log('exited')
                // delete cachedMira[target]  figure out how to remove from json
                window.localStorage.setItem('Mira', cachedMira)
                LoadMirabufRemote(target)
            }
            if (fileHandle) {
                console.log(fileHandle)
                const file = await fileHandle.getFile()
                const buff = await file.arrayBuffer()
                console.log(file)
                assembly = mirabuf.Assembly.decode(UnzipMira(new Uint8Array(buff)))
            }

            console.log(assembly)
            return assembly
        }
    } else {
        window.localStorage.setItem("Mira", JSON.stringify(""))
        
        cachedMira = JSON.parse(window.localStorage.getItem("Mira") ?? "{}")
    }

    if (cachedMira) {
        //Download and store file if targetID doesn't exist
        const id = Date.now().toString()

        // Grab file remote
        const miraBuff = await fetch(encodeURI(fetchLocation), useCache ? undefined : {cache: "no-store"}).then(x => x.blob()).then(x => x.arrayBuffer());
        const byteBuffer = UnzipMira(new Uint8Array(miraBuff));
        assembly = mirabuf.Assembly.decode(byteBuffer);

        // Store in OPFS
        let fileHandle = await robotFolderHandle.getFileHandle(id, { create: true });
        if (!isRobot) {
            fileHandle = await fieldFolderHandle.getFileHandle(id, { create: true })
        }

        const writable = await fileHandle.createWritable();
        await writable.write(miraBuff)
        await writable.close()

        // Local cache array
        targetID = id
        cachedMira[target] = targetID
        window.localStorage.setItem('Mira', JSON.stringify(cachedMira))
    }

    console.log(assembly)
    return assembly;
}

export function LoadMirabufLocal(fileLocation: string): mirabuf.Assembly {
    console.log(fileLocation);
    return mirabuf.Assembly.decode(UnzipMira(new Uint8Array(fs.readFileSync(fileLocation))));
}

export async function ClearMira() {
    for await(let key of robotFolderHandle.keys()) {
        robotFolderHandle.removeEntry(key)
    }
    for await(let key of robotFolderHandle.keys()) {
        fieldFolderHandle.removeEntry(key)
    }

    window.localStorage.clear()
}