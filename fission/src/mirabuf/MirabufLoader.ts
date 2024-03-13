import { mirabuf } from "../proto/mirabuf";
import Pako from "pako";
import * as fs from 'fs';

export function UnzipMira(buff: Uint8Array): Uint8Array {
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff);
    } else {
        return buff;
    }
}

export async function LoadMirabufRemote(fetchLocation: string): Promise<mirabuf.Assembly | undefined> {
    const miraBuff = await fetch(fetchLocation, {cache: "no-store"}).then(x => x.blob()).then(x => x.arrayBuffer());
    const byteBuffer = UnzipMira(new Uint8Array(miraBuff));
    return mirabuf.Assembly.decode(byteBuffer);
}

export function LoadMirabufLocal(fileLocation: string): mirabuf.Assembly {
    return mirabuf.Assembly.decode(UnzipMira(new Uint8Array(fs.readFileSync(fileLocation))));
}