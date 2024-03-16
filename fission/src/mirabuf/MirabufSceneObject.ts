import { mirabuf } from "@/proto/mirabuf";
import SceneObject from "../systems/scene/SceneObject";
import GetSceneRenderer from "../systems/scene/SceneRenderer";
import MirabufInstance from "./MirabufInstance";
import { LoadMirabufRemote } from "./MirabufLoader";
import MirabufParser, { ParseErrorSeverity } from "./MirabufParser";

class MirabufSceneObject extends SceneObject {

    private _mirabufInstance: MirabufInstance;

    public constructor(mirabufInstance: MirabufInstance) {
        super();

        this._mirabufInstance = mirabufInstance;
    }

    public Setup(): void {
        this._mirabufInstance.AddToScene(GetSceneRenderer().scene);
    }

    public Update(): void { }

    public Dispose(): void {
        this._mirabufInstance.Dispose(GetSceneRenderer().scene);
    }
}

export async function CreateMirabufFromUrl(path: string): Promise<MirabufSceneObject | null | undefined> {
    const miraAssembly = await LoadMirabufRemote(path)
        .catch(console.error);

    if (!miraAssembly || !(miraAssembly instanceof mirabuf.Assembly)) {
        return;
    }

    const parser = new MirabufParser(miraAssembly);
    if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
        console.error(`Assembly Parser produced significant errors for '${miraAssembly.info!.name!}'`);
        return;
    }
    
    return new MirabufSceneObject(new MirabufInstance(parser));
}

export default MirabufSceneObject;