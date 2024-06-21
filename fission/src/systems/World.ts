import * as THREE from "three";

import PhysicsSystem from "./physics/PhysicsSystem";
import SceneRenderer from "./scene/SceneRenderer";
import SimulationSystem from "./simulation/SimulationSystem";

class World {

    private static _isAlive: boolean = false;
    private static _clock: THREE.Clock;
    
    private static _sceneRenderer: SceneRenderer;
    private static _physicsSystem: PhysicsSystem;
    private static _simulationSystem: SimulationSystem;

    public static get isAlive() { return World._isAlive; }
    
    public static get SceneRenderer() { return World._sceneRenderer; }
    public static get PhysicsSystem() { return World._physicsSystem; }
    public static get SimulationSystem() { return World._simulationSystem; }
    
    public static InitWorld() {
        if (World._isAlive)
            return;

        World._clock = new THREE.Clock();
        World._isAlive = true;

        World._sceneRenderer = new SceneRenderer();
        World._physicsSystem = new PhysicsSystem();
        World._simulationSystem = new SimulationSystem();
    }

    public static DestroyWorld() {
        if (!World._isAlive)
            return;

        World._isAlive = false;

        World._physicsSystem.Destroy();
        World._sceneRenderer.Destroy();
        World._simulationSystem.Destroy();
    }

    public static UpdateWorld() {
        const deltaT = World._clock.getDelta();
        World._simulationSystem.Update(deltaT);
        World._physicsSystem.Update(deltaT);
        World._sceneRenderer.Update(deltaT);
    }
}

export default World;