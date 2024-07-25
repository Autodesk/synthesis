import * as THREE from "three"

import PhysicsSystem from "./physics/PhysicsSystem"
import SceneRenderer from "./scene/SceneRenderer"
import SimulationSystem from "./simulation/SimulationSystem"
import InputSystem from "./input/InputSystem"
import AnalyticsSystem from "./analytics/AnalyticsSystem"

class World {
    private static _isAlive: boolean = false
    private static _clock: THREE.Clock

    private static _sceneRenderer: SceneRenderer
    private static _physicsSystem: PhysicsSystem
    private static _simulationSystem: SimulationSystem
    private static _inputSystem: InputSystem
    private static _analyticsSystem: AnalyticsSystem | undefined

    public static get isAlive() {
        return World._isAlive
    }

    public static get SceneRenderer() {
        return World._sceneRenderer
    }
    public static get PhysicsSystem() {
        return World._physicsSystem
    }
    public static get SimulationSystem() {
        return World._simulationSystem
    }
    public static get InputSystem() {
        return World._inputSystem
    }
    public static get AnalyticsSystem() {
        return World._analyticsSystem
    }

    public static InitWorld() {
        if (World._isAlive) return

        World._clock = new THREE.Clock()
        World._isAlive = true

        World._sceneRenderer = new SceneRenderer()
        World._physicsSystem = new PhysicsSystem()
        World._simulationSystem = new SimulationSystem()
        World._inputSystem = new InputSystem()
        try {
            World._analyticsSystem = new AnalyticsSystem()
        } catch (_) {
            World._analyticsSystem = undefined
        }
    }

    public static DestroyWorld() {
        if (!World._isAlive) return

        World._isAlive = false

        World._physicsSystem.Destroy()
        World._sceneRenderer.Destroy()
        World._simulationSystem.Destroy()
        World._inputSystem.Destroy()
        World._analyticsSystem?.Destroy()
    }

    public static UpdateWorld() {
        const deltaT = World._clock.getDelta()
        World._simulationSystem.Update(deltaT)
        World._physicsSystem.Update(deltaT)
        World._inputSystem.Update(deltaT)
        World._sceneRenderer.Update(deltaT)
        World._analyticsSystem?.Update(deltaT)
    }
}

export default World
