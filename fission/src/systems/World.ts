import * as THREE from "three";

import PhysicsSystem from "./physics/PhysicsSystem";
import SceneRenderer from "./scene/SceneRenderer";

class World {

    private static _isAlive: boolean = false;
    private static _clock: THREE.Clock;
    
    private static _sceneRenderer: SceneRenderer;
    private static _physicsSystem: PhysicsSystem;

    public static get SceneRenderer() { return World._sceneRenderer; }
    public static get PhysicsSystem() { return World._physicsSystem; }
    
    public static InitWorld() {
        if (World._isAlive)
            return;

        World._clock = new THREE.Clock();
        World._isAlive = true;

        World._sceneRenderer = new SceneRenderer();
        World._physicsSystem = new PhysicsSystem();
    }

    public static DestroyWorld() {
        if (!World._isAlive)
            return;

        World._isAlive = false;

        World._physicsSystem.Destroy();
        World._sceneRenderer.Destroy();
    }

    public static UpdateWorld() {
        const deltaT = World._clock.getDelta();
        this._physicsSystem.Update(deltaT);
        this._sceneRenderer.Update(deltaT);
    }
}

export default World;