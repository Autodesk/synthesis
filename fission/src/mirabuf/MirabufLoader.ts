import { mirabuf } from "../proto/mirabuf";
import Pako from "pako";
import * as fs from 'fs';
import { time } from "console";

var id = 6;

export function UnzipMira(buff: Uint8Array): Uint8Array {
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff);
    } else {
        return buff;
    }
}
export async function LoadMirabufRemote(fetchLocation: string, useCache: boolean = true): Promise<mirabuf.Assembly | undefined> {

    const target = fetchLocation.substring(fetchLocation.lastIndexOf("\\") + 1).substring(fetchLocation.lastIndexOf("/") + 1)
    console.log(target)

    let cachedMira = JSON.parse(window.localStorage.getItem('Mira') ?? "{}")
    let targetID = cachedMira[target]

    // let targetID = JSON.parse(window.localStorage.getItem(target) ?? "{}") //will later make a nice array cachedMira to grab instead then search through with cachedMira[target]
    console.log(targetID)
    let assembly;
    const root = await navigator.storage.getDirectory();
    const folderHandle = await root.getDirectoryHandle("Mira", { create: true })
    if (!targetID) {


        const sID = id.toString()
        id = id + 1




        // Grab file remote and store local
        const miraBuff = await fetch(encodeURI(fetchLocation), useCache ? undefined : {cache: "no-store"}).then(x => x.blob()).then(x => x.arrayBuffer());
        const byteBuffer = UnzipMira(new Uint8Array(miraBuff));
        assembly = mirabuf.Assembly.decode(byteBuffer);

        const fileHandle = await folderHandle.getFileHandle(sID, { create: true });
        const writable = await fileHandle.createWritable();
        await writable.write(miraBuff)
        await writable.close()






        // Local cache array
        console.log('better hi')
        targetID = sID
        cachedMira[target] = targetID
        window.localStorage.setItem('Mira', JSON.stringify(cachedMira))
    } else {
        // Grab file opfs
        const fileHandle = await folderHandle.getFileHandle(targetID, {create: false})
        const file = await fileHandle.getFile()
        const buff = await file.arrayBuffer()
        console.log(file)
        assembly = mirabuf.Assembly.decode(UnzipMira(new Uint8Array(buff)))
    }

    



    // let bool = window.localStorage.getItem(fetchLocation)
    // console.log(bool);
    // var assembly;
    // if (bool == null) {
    //     console.log("hi");
    //     const miraBuff = await fetch(encodeURI(fetchLocation), useCache ? undefined : {cache: "no-store"}).then(x => x.blob()).then(x => x.arrayBuffer());
    //     const byteBuffer = UnzipMira(new Uint8Array(miraBuff));
    //     // localStorage.setItem(fetchLocation, "A");
    //     assembly = mirabuf.Assembly.decode(byteBuffer);
    //     console.log(assembly);
        
    // } else {
    //     console.log("bye")
    //     assembly = LoadMirabufLocal('./public/Downloadables/Mira/Fields/FRC Field 2018_v13.mira');
    //     console.log(assembly);
    // }
    // console.log(assembly);





    // const targetLocation = 'somewhere'

    // const fetchLocations = JSON.parse(window.localStorage.getItem('fetchLocations') ?? "{}")
    // let existingLocation = fetchLocations[targetLocation]
    // if (!existingLocation) {
    //   // Load mirabuf file, get uuid
    //   existingLocation = 0
    //   fetchLocations[targetLocation] = existingLocation
    //   window.localStorage.setItem('fetchLocation', JSON.stringify(existingLocation))
    // }

    console.log(assembly)
    return assembly;
}

export function LoadMirabufLocal(fileLocation: string): mirabuf.Assembly {
    console.log(fileLocation);
    return mirabuf.Assembly.decode(UnzipMira(new Uint8Array(fs.readFileSync(fileLocation))));
}