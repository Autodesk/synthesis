import * as THREE from "three"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import { PerformanceMonitoringSystem } from "@/systems/PerformanceMonitor.ts"
import AnalyticsSystem, { type AccumTimes } from "./analytics/AnalyticsSystem"
import InputSystem from "./input/InputSystem"
import RobotDimensionTracker from "./match_mode/RobotDimensionTracker"
import type MultiplayerSystem from "./multiplayer/MultiplayerSystem"
import PhysicsSystem from "./physics/PhysicsSystem"
import DragModeSystem from "./scene/DragModeSystem"
import SceneRenderer from "./scene/SceneRenderer"
import RobotPositionTracker from "./simulation/RobotPositionTracker"
import SimulationSystem from "./simulation/SimulationSystem"

class World {
    private static _instance?: World

    private _isAlive: boolean = false
    private _clock: THREE.Clock
    private _currentDeltaT: number = 0

    private _sceneRenderer: SceneRenderer
    private _physicsSystem: PhysicsSystem
    private _simulationSystem: SimulationSystem
    private _inputSystem: InputSystem
    private _multiplayerSystem?: MultiplayerSystem
    private _analyticsSystem: AnalyticsSystem | undefined = undefined
    private _dragModeSystem: DragModeSystem
    private _performanceMonitorSystem: PerformanceMonitoringSystem
    private _scoreTracker: ScoreTracker = new ScoreTracker()

    private _accumTimes: AccumTimes = {
        frames: 0,
        sceneTime: 0,
        physicsTime: 0,
        simulationTime: 0,
        inputTime: 0,
        totalTime: 0,
    }

    public static get accumTimes() {
        return this._instance?._accumTimes!
    }

    public static get isAlive() {
        return this._instance?._isAlive ?? false
    }

    public static get sceneRenderer() {
        return this._instance?._sceneRenderer!
    }

    public static get physicsSystem() {
        return this._instance?._physicsSystem!
    }

    public static get simulationSystem() {
        return this._instance?._simulationSystem!
    }
    public static get inputSystem() {
        return this._instance?._inputSystem!
    }
    public static get multiplayerSystem() {
        return this._instance?._multiplayerSystem
    }
    public static get analyticsSystem() {
        return this._instance?._analyticsSystem
    }
    public static get dragModeSystem() {
        return this._instance?._dragModeSystem!
    }
    public static get scoreTracker() {
        return this._instance?._scoreTracker!
    }

    public static getOwnRobots() {
        return World.multiplayerSystem?.getOwnRobots() ?? World.sceneRenderer.mirabufSceneObjects.getRobots()
    }

    public static getOwnObjects() {
        return World.multiplayerSystem?.getOwnObjects() ?? World.sceneRenderer.mirabufSceneObjects.getAll()
    }

    public static resetAccumTimes() {
        this._instance!._accumTimes! = {
            frames: 0,
            sceneTime: 0,
            physicsTime: 0,
            simulationTime: 0,
            inputTime: 0,
            totalTime: 0,
        }
    }

    public static setMultiplayerSystem(multiplayerSystem?: MultiplayerSystem) {
        this._instance!._multiplayerSystem = multiplayerSystem
    }

    public constructor() {
        this._clock = new THREE.Clock()
        this._isAlive = true

        this._sceneRenderer = new SceneRenderer()
        this._physicsSystem = new PhysicsSystem()
        this._simulationSystem = new SimulationSystem()
        this._inputSystem = new InputSystem()
        this._dragModeSystem = new DragModeSystem()
        this._performanceMonitorSystem = new PerformanceMonitoringSystem()

        try {
            this._analyticsSystem = new AnalyticsSystem()
        } catch (_) {
            this._analyticsSystem = undefined
        }
    }

    public static initWorld() {
        if (this._instance == null) {
            this._instance = new World()

            if (import.meta.env.DEV) {
                window.World = World
            }
        }
    }

    public destroy() {
        if (!this._isAlive) return

        this._isAlive = false

        this._physicsSystem.destroy()
        this._sceneRenderer.destroy()
        this._simulationSystem.destroy()
        this._inputSystem.destroy()
        this._multiplayerSystem?.destroy()
        this._dragModeSystem.destroy()

        this._performanceMonitorSystem.destroy()
        this._analyticsSystem?.destroy()
    }

    public static destroyWorld() {
        this._instance?.destroy()
        this._instance = undefined
    }

    public update() {
        this._currentDeltaT = this._clock.getDelta()

        this._accumTimes.frames++

        this._accumTimes.totalTime += this.time(() => {
            this._accumTimes.simulationTime += this.time(() => this._simulationSystem.update(this._currentDeltaT))
            this._accumTimes.physicsTime += this.time(() => this._physicsSystem.update(this._currentDeltaT))
            this._accumTimes.inputTime += this.time(() => this._inputSystem.update(this._currentDeltaT))
            this._accumTimes.sceneTime += this.time(() => this._sceneRenderer.update(this._currentDeltaT))
            this._dragModeSystem.update(this._currentDeltaT)
        })

        this._analyticsSystem?.update(this._currentDeltaT)
        this._performanceMonitorSystem?.update(this._currentDeltaT)

        RobotDimensionTracker.update()
        RobotPositionTracker.update()
    }

    public static updateWorld() {
        this._instance?.update()
    }

    public static get currentDeltaT(): number {
        return this._instance?._currentDeltaT!
    }

    private time(func: () => void): number {
        const start = Date.now()
        func()
        return Date.now() - start
    }
}

export default World

if (import.meta.hot) {
    // Restore the instance that survived the HMR reload
    if (import.meta.hot.data.world) {
        World["_instance"] = import.meta.hot.data.world
    }

    // Stash the instance before the module is replaced
    import.meta.hot.on("vite:beforeUpdate", () => {
        import.meta.hot!.data.world = World["_instance"]
    })
}
