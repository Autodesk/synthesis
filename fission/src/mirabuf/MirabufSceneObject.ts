import SceneObject from "../systems/scene/SceneObject";
import GetSceneRenderer from "../systems/scene/SceneRenderer";
import MirabufInstance from "./MirabufInstance";

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

export default MirabufSceneObject;