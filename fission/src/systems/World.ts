import * as THREE from "three"
import { PerformanceMonitoringSystem } from "@/systems/PerformanceMonitor.ts"
import AnalyticsSystem, { type AccumTimes } from "./analytics/AnalyticsSystem"
import InputSystem from "./input/InputSystem"
import RobotDimensionTracker from "./match_mode/RobotDimensionTracker"
import PhysicsSystem from "./physics/PhysicsSystem"
import DragModeSystem from "./scene/DragModeSystem"
import SceneRenderer from "./scene/SceneRenderer"
import RobotPositionTracker from "./simulation/RobotPositionTracker"
import SimulationSystem from "./simulation/SimulationSystem"

class World {
    private static _isAlive: boolean = false
    private static _clock: THREE.Clock
    private static _currentDeltaT: number = 0

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

        SceneRenderer.setup()
        PhysicsSystem.setup()
        InputSystem.setup()
        DragModeSystem.setup()
        PerformanceMonitoringSystem.start()
        try {
            AnalyticsSystem.setup()
        } catch (_) {}
    }

    public static destroyWorld() {
        if (!World._isAlive) return

        World._isAlive = false

        PhysicsSystem.destroy()
        SceneRenderer.destroy()
        SimulationSystem.destroy()
        InputSystem.destroy()
        DragModeSystem.destroy()

        PerformanceMonitoringSystem.destroy()
        AnalyticsSystem?.destroy()
    }

    public static updateWorld() {
        this._currentDeltaT = World._clock.getDelta()

        this._accumTimes.frames++

        this._accumTimes.totalTime += this.time(() => {
            this._accumTimes.simulationTime += this.time(() => SimulationSystem.update(this._currentDeltaT))
            this._accumTimes.physicsTime += this.time(() => PhysicsSystem.update(this._currentDeltaT))
            this._accumTimes.inputTime += this.time(() => InputSystem.update(this._currentDeltaT))
            this._accumTimes.sceneTime += this.time(() => SceneRenderer.update(this._currentDeltaT))
            DragModeSystem.update(this._currentDeltaT)
        })

        AnalyticsSystem?.update(this._currentDeltaT)
        PerformanceMonitoringSystem?.update(this._currentDeltaT)

        RobotDimensionTracker.update()
        RobotPositionTracker.update()
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
