import * as THREE from "three"

import PhysicsSystem from "./physics/PhysicsSystem"
import SceneRenderer from "./scene/SceneRenderer"
import SimulationSystem from "./simulation/SimulationSystem"
import InputSystem from "./input/InputSystem"
import AnalyticsSystem, { AccumTimes } from "./analytics/AnalyticsSystem"

class World {
    private static _isAlive: boolean = false
    private static _clock: THREE.Clock

    private static _sceneRenderer: SceneRenderer
    private static _physicsSystem: PhysicsSystem
    private static _simulationSystem: SimulationSystem
    private static _inputSystem: InputSystem
    private static _analyticsSystem: AnalyticsSystem | undefined

    private static _accumTimes: AccumTimes = {
        frames: 0,
        sceneTime: 0,
        physicsTime: 0,
        simulationTime: 0,
        inputTime: 0,
        totalTime: 0,
    }

    public static get accumTimes() {
        return World._accumTimes
    }

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

    public static resetAccumTimes() {
        this._accumTimes = {
            frames: 0,
            sceneTime: 0,
            physicsTime: 0,
            simulationTime: 0,
            inputTime: 0,
            totalTime: 0,
        }
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

        this._accumTimes.frames++

        this._accumTimes.totalTime += this.time(() => {
            this._accumTimes.simulationTime += this.time(() => World._simulationSystem.Update(deltaT))
            this._accumTimes.physicsTime += this.time(() => World._physicsSystem.Update(deltaT))
            this._accumTimes.inputTime += this.time(() => World._inputSystem.Update(deltaT))
            this._accumTimes.sceneTime += this.time(() => World._sceneRenderer.Update(deltaT))
        })

        World._analyticsSystem?.Update(deltaT)
    }

    private static time(func: () => void): number {
        const start = Date.now()
        func()
        return Date.now() - start
    }
}

export default World
