abstract class SceneObject {
    private _id: number | null = null;

    public get id() {
        return this._id!;
    }

    public set id(sceneId: number) {
        if (this._id)
            return;
        this._id = sceneId;
    }

    public abstract PreRenderUpdate(): void;
}

export default SceneObject;