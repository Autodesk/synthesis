abstract class SceneObject {
    private _id: number | null = null

    public get id() {
        return this._id!
    }

    public set id(sceneId: number) {
        if (this._id) return
        this._id = sceneId
    }

    /**
     * Setup is executed after an ID is assigned and the SceneObject is registered with the SceneRenderer
     */
    public abstract Setup(): void

    /**
     * Update is executed just before rendering of the scene to allow for uniform and rendering parameter updates.
     */
    public abstract Update(): void

    /**
     * Dispose is executed just before the SceneObject is unregistered and removed from the SceneRenderer.
     */
    public abstract Dispose(): void
}

export default SceneObject
