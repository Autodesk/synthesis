import beep from "@/assets/sound-files/beep.wav"
import MatchEnd from "@/assets/sound-files/MatchEnd.wav"
import MatchResume from "@/assets/sound-files/MatchResume.wav"
import MatchStart from "@/assets/sound-files/MatchStart.wav"
import EventSystem from "@/systems/EventSystem.ts"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs.ts"
import World from "@/systems/World.ts"
import { globalOpenModal } from "@/ui/components/GlobalUIControls"
import MatchResultsModal from "@/ui/modals/MatchResultsModal"
import type { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { createMatchEventFromConfig } from "./MatchModeAnalyticsUtils"
import { SoundPlayer } from "../sound/SoundPlayer"
import { MatchModeType } from "./MatchModeTypes"
import RobotDimensionTracker from "./RobotDimensionTracker"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalAddToast, globalOpenPanel } from "@/ui/components/GlobalUIControls"

// Register command: Toggle Match Mode
CommandRegistry.get().registerCommand({
    id: "toggle-match-mode",
    label: "Toggle Match Mode",
    description: "Toggle match mode, allowing you to simulate and run a full match.",
    keywords: ["match", "mode", "start", "play", "game", "simulate", "toggle"],
    perform: () => {
        if (MatchMode.getInstance().isMatchEnabled()) {
            MatchMode.getInstance().sandboxModeStart()
            globalAddToast("info", "Match Mode Cancelled")
        } else {
            import("@/ui/panels/configuring/MatchModeConfigPanel").then(m => {
                globalOpenPanel(m.default, undefined)
            })
        }
    },
})

class MatchMode {
    private static _instance: MatchMode
    private _endgame: boolean = false
    private _matchModeType: MatchModeType = MatchModeType.SANDBOX

    private setMatchModeType(val: MatchModeType) {
        this._matchModeType = val
        EventSystem.dispatch("MatchStateChangedEvent", { mode: val })
    }

    private _startTime: number = 0
    private _timeUsed: number = 0
    private _intervalId: number | null = null

    // Match Mode Config
    private _matchModeConfig: MatchModeConfig = DefaultMatchModeConfigs.fallbackValues()

    private constructor() {}

    static getInstance(): MatchMode {
        MatchMode._instance ??= new MatchMode()
        return MatchMode._instance
    }

    setMatchModeConfig(config: MatchModeConfig) {
        this._matchModeConfig = config
        RobotDimensionTracker.setConfigValues(
            config.ignoreRotation,
            config.maxHeight,
            config.heightLimitPenalty,
            config.sideMaxExtension,
            config.sideExtensionPenalty
        )
    }

    get matchModeConfig() {
        return this._matchModeConfig
    }

    async runForNext(duration: number, updateTimeLeft: boolean = true) {
        if (this._intervalId !== null) {
            console.warn("Timer already running")
        }

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) EventSystem.dispatch("TimeChangedEvent", { time: duration })
        this._intervalId = window.setInterval(() => {
            const timeElapsed = (Date.now() - this._startTime - this._timeUsed) / 1000
            const timeLeft = duration - timeElapsed

            if (timeLeft >= 0 && updateTimeLeft) {
                EventSystem.dispatch("TimeChangedEvent", { time: timeLeft })
            }

            // Checks if endgame has started
            if (
                this._matchModeType === MatchModeType.TELEOP &&
                timeLeft <= this._matchModeConfig.endgameTime &&
                !this._endgame
            ) {
                this.endgameStart()
            }
        }, 200)

        const remainingTime = this._startTime + this._timeUsed - Date.now() + duration * 1000
        return new Promise<void>(res => setTimeout(res, remainingTime)).finally(() => {
            this._timeUsed += duration * 1000
            clearInterval(this._intervalId as number)
            this._intervalId = null
        })
    }

    autonomousModeStart() {
        void SoundPlayer.getInstance().play(MatchStart)
        this.setMatchModeType(MatchModeType.AUTONOMOUS)
        this.runForNext(this._matchModeConfig.autonomousTime).then(() => this.autonomousModeEnd())
    }

    autonomousModeEnd() {
        void SoundPlayer.getInstance().play(MatchEnd)
        this.runForNext(3, false).then(() => this.teleopModeStart()) // Delay between autonomous and teleop modes
    }

    teleopModeStart() {
        void SoundPlayer.getInstance().play(MatchResume)
        this.setMatchModeType(MatchModeType.TELEOP)
        this.runForNext(this._matchModeConfig.teleopTime).then(() => this.matchEnded())
    }

    endgameStart() {
        void SoundPlayer.getInstance().play(beep)
        this._matchModeType = MatchModeType.ENDGAME
        console.log("endgame start")
        this._endgame = true
    }

    async start(broadcast = true, useSpawnPositions: boolean) {
        const startTime = Date.now() + 300 // Accounts for time it takes for robots to move to start positions and settle, and for multiplayer state to sync
        if (broadcast && World.multiplayerSystem) {
            await World.multiplayerSystem.broadcast({
                type: "matchModeState",
                data: {
                    event: "start",
                    config: this._matchModeConfig,
                    moveRobots: useSpawnPositions,
                },
            })
        }

        this._startTime = startTime

        if (useSpawnPositions) {
            World.getOwnRobots().forEach(obj => obj.moveToSpawnLocation())
        }

        World.scoreTracker.resetScores()
        RobotDimensionTracker.matchStart()

        const matchEvent = createMatchEventFromConfig(this._matchModeConfig)
        World.analyticsSystem?.event("Match Start", matchEvent)

        this.runForNext(0, false).then(() => this.autonomousModeStart())
    }

    matchEnded() {
        void SoundPlayer.getInstance().play(MatchEnd)
        this.setMatchModeType(MatchModeType.MATCH_ENDED)

        const matchEvent = createMatchEventFromConfig(this._matchModeConfig)
        World.analyticsSystem?.event("Match End", matchEvent)
        globalOpenModal(MatchResultsModal, undefined)
    }

    sandboxModeStart() {
        this.setMatchModeType(MatchModeType.SANDBOX)
        clearInterval(this._intervalId as number)
        this._startTime = 0
        this._timeUsed = 0
        EventSystem.dispatch("TimeChangedEvent", { time: 0 })
        World.scoreTracker.resetScores()
    }

    isMatchEnabled(): boolean {
        return !(this._matchModeType == MatchModeType.SANDBOX || this._matchModeType == MatchModeType.MATCH_ENDED)
    }

    isEndgame(): boolean {
        return this._endgame
    }

    getMatchModeType(): MatchModeType {
        return this._matchModeType
    }
}

export default MatchMode
