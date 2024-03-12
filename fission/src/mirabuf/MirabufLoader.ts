import { mirabuf } from "../proto/mirabuf";

async function LoadMirabufRemote(fetchLocation: string): Promise<mirabuf.Assembly | undefined> {
    const miraBuff = await fetch(fetchLocation, {cache: "no-store"}).then(x => x.blob()).then(x => x.arrayBuffer());
    return mirabuf.Assembly.decode(new Uint8Array(miraBuff));
}

export default LoadMirabufRemote;