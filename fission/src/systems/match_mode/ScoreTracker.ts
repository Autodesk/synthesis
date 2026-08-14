import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import type { Alliance } from "@/systems/preferences/PreferenceTypes.ts"
import World from "@/systems/World.ts"
import EventSystem from "@/systems/EventSystem.ts"

export default class ScoreTracker {
    private _redScore: number = 0
    private _blueScore: number = 0
    private _perRobotScore: Map<MirabufSceneObject, number> = new Map()

    public get redScore() {
        return this._redScore
    }
    public get blueScore() {
        return this._blueScore
    }

    public get perRobotScore(): ReadonlyMap<MirabufSceneObject, number> {
        return this._perRobotScore
    }

    public resetScores(): void {
        this._redScore = 0
        this._blueScore = 0
        this._perRobotScore = new Map()
        World.sceneRenderer.mirabufSceneObjects.getField()?.scoringZones?.forEach(zone => zone.reset())
        this.notifyChange()
    }

    public addPerRobotScore(robot: MirabufSceneObject, scoreToAdd: number): void {
        const currentRobotScore = this._perRobotScore.get(robot) ?? 0
        this._perRobotScore.set(robot, currentRobotScore + scoreToAdd)
    }

    public addPoints(alliance: Alliance, points: number, notify: boolean = true) {
        if (alliance == "red") {
            this._redScore += points
        } else {
            this._blueScore += points
        }
        if (notify) {
            this.notifyChange()
        }
    }

    private notifyChange() {
        EventSystem.dispatch("ScoreChangedEvent", { red: this.redScore, blue: this.blueScore })
    }

    /**
     * In order for this function to work with match mode, the caller must adhere to either of the following contracts, but not both:
     * 1. The parameter `broadcastPenalty` should be set to `false`
     * 2. This function should only be called to penalize robots owned by the client
     *
     * Basically, every client is responsible for broadcasting penalties committed by their robot
     */
    public robotPenalty(
        robot: MirabufSceneObject,
        penaltyPoints: number,
        penaltyInfo: string,
        broadcastPenalty: boolean = true
    ): void {
        if (broadcastPenalty && World.multiplayerSystem) {
            World.multiplayerSystem.broadcast({
                type: "matchModePenalty",
                data: {
                    objectId: robot.id,
                    points: penaltyPoints,
                    description: penaltyInfo,
                },
            })
        }

        // Display a toast showing that a penalty was committed
        globalAddToast(
            "warning",
            "PENALTY COMMITTED",
            `Robot ${robot.nameTag?.text()} (${robot.assemblyName}), Committed Penalty: ${penaltyInfo}`
        )
        // Update match score
        if (robot.alliance == "red") {
            this._blueScore += penaltyPoints
        } else {
            this._redScore += penaltyPoints
        }
        this.notifyChange()
        // Update per robot score
        this.addPerRobotScore(robot, -penaltyPoints)
    }
}
