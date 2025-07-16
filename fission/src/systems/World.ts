import * as THREE from "three"

import PhysicsSystem from "./physics/PhysicsSystem"
import SceneRenderer from "./scene/SceneRenderer"
import SimulationSystem from "./simulation/SimulationSystem"
import InputSystem from "./input/InputSystem"
import AnalyticsSystem, { AccumTimes } from "./analytics/AnalyticsSystem"
import DragModeSystem from "./scene/DragModeSystem"
import { PerformanceMonitoringSystem } from "@/systems/PerformanceMonitor.ts"
import RobotDimensionTracker from "./RobotDimensionTracker"

class World {
    private static _isAlive: boolean = false
    private static _clock: THREE.Clock
    private static _currentDeltaT: number = 0

    private static _sceneRenderer: SceneRenderer
    private static _physicsSystem: PhysicsSystem
    private static _simulationSystem: SimulationSystem
    private static _inputSystem: InputSystem
    private static _analyticsSystem: AnalyticsSystem | undefined = undefined
    private static _dragModeSystem: DragModeSystem
    private static _performanceMonitorSystem: PerformanceMonitoringSystem

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

    public static get sceneRenderer() {
        return World._sceneRenderer
    }
    public static get physicsSystem() {
        return World._physicsSystem
    }
    public static get simulationSystem() {
        return World._simulationSystem
    }
    public static get inputSystem() {
        return World._inputSystem
    }
    public static get analyticsSystem() {
        return World._analyticsSystem
    }
    public static get dragModeSystem() {
        return World._dragModeSystem
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

    public static initWorld() {
        if (World._isAlive) return

        World._clock = new THREE.Clock()
        World._isAlive = true

        World._sceneRenderer = new SceneRenderer()
        World._physicsSystem = new PhysicsSystem()
        World._simulationSystem = new SimulationSystem()
        World._inputSystem = new InputSystem()
        World._dragModeSystem = new DragModeSystem()
        World._performanceMonitorSystem = new PerformanceMonitoringSystem()
        try {
            World._analyticsSystem = new AnalyticsSystem()
        } catch (_) {
            World._analyticsSystem = undefined
        }
    }

    public static destroyWorld() {
        if (!World._isAlive) return

        World._isAlive = false

        World._physicsSystem.destroy()
        World._sceneRenderer.destroy()
        World._simulationSystem.destroy()
        World._inputSystem.destroy()
        World._dragModeSystem.destroy()

        World._performanceMonitorSystem.destroy()
        World._analyticsSystem?.destroy()
    }

    public static updateWorld() {
        this._currentDeltaT = World._clock.getDelta()

        this._accumTimes.frames++

        this._accumTimes.totalTime += this.time(() => {
            this._accumTimes.simulationTime += this.time(() => World._simulationSystem.update(this._currentDeltaT))
            this._accumTimes.physicsTime += this.time(() => World._physicsSystem.update(this._currentDeltaT))
            this._accumTimes.inputTime += this.time(() => World._inputSystem.update(this._currentDeltaT))
            this._accumTimes.sceneTime += this.time(() => World._sceneRenderer.update(this._currentDeltaT))
            World._dragModeSystem.update(this._currentDeltaT)
        })

        World._analyticsSystem?.update(this._currentDeltaT)
        World._performanceMonitorSystem?.update(this._currentDeltaT)

        RobotDimensionTracker.update(this._currentDeltaT, World._sceneRenderer)
    }

    public static get currentDeltaT(): number {
        return this._currentDeltaT
    }

    private static time(func: () => void): number {
        const start = Date.now()
        func()
        return Date.now() - start
    }
}

export default World
