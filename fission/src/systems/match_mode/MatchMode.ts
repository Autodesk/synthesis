import beep from "@/assets/sound-files/beep.wav"
import MatchEnd from "@/assets/sound-files/MatchEnd.wav"
import MatchResume from "@/assets/sound-files/MatchResume.wav"
import MatchStart from "@/assets/sound-files/MatchStart.wav"
import EventSystem from "@/systems/EventSystem.ts"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs.ts"
import World from "@/systems/World.ts"
import { globalCloseModal, globalOpenModal } from "@/ui/components/GlobalUIControls"
import MatchResultsModal from "@/ui/modals/MatchResultsModal"
import type { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { createMatchEventFromConfig } from "./MatchModeAnalyticsUtils"
import { SoundPlayer } from "../sound/SoundPlayer"
import { MatchModeType } from "./MatchModeTypes"
import RobotDimensionTracker from "./RobotDimensionTracker"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalAddToast, globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { CloseType } from "@/ui/helpers/UIProviderHelpers.ts"

// Register command: Toggle Match Mode
CommandRegistry.get().registerCommand({
    id: "toggle-match-mode",
    label: "Toggle Match Mode",
    description: "Toggle match mode, allowing you to simulate and run a full match.",
    keywords: ["match", "mode", "start", "play", "game", "simulate", "toggle"],
    perform: () => {
        if (MatchMode.getInstance().isMatchEnabled()) {
            MatchMode.getInstance().abort()
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

    private _startTime: number | null = null
    private _timeUsed: number = 0
    private _intervalId: number | null = null
    private _cancelHandler: (() => void) | null = null

    private _resultsModalId: string | null = null

    // Match Mode Config
    private _matchModeConfig: MatchModeConfig = DefaultMatchModeConfigs.fallbackValues()

    private constructor() {}

    get startTime() {
        return this._startTime
    }

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

    /**
     * Waits for the specified duration in match time to pass, accounting for delays from prior loop cycles. Resolves when the time has elapsed, rejects if cancelled.
     * @param duration The duration, in seconds, of the phase
     * @param updateTimeLeft Whether to dispatch scoreboard update events
     */
    async runForNext(duration: number, updateTimeLeft: boolean = true) {
        if (this._intervalId !== null) {
            console.warn("Timer already running")
            return
        }

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) EventSystem.dispatch("TimeChangedEvent", { time: duration })
        this._intervalId = window.setInterval(() => {
            const timeElapsed = (Date.now() - this._startTime! - this._timeUsed) / 1000
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
        }, 100)

        const remainingTime = this._startTime! + this._timeUsed - Date.now() + duration * 1000

        return new Promise((res, reject) => {
            setTimeout(res, remainingTime)
            this._cancelHandler = () => reject("Cancelled")
        })
            .then(() => {
                this._timeUsed += duration * 1000
            })
            .finally(() => {
                clearInterval(this._intervalId as number)
                this._intervalId = null
            })
    }

    autonomousModeStart() {
        void SoundPlayer.getInstance().play(MatchStart)
        this.setMatchModeType(MatchModeType.AUTONOMOUS)
        this.runForNext(this._matchModeConfig.autonomousTime)
            .then(() => this.autonomousModeEnd())
            .catch(() => {})
    }

    autonomousModeEnd() {
        void SoundPlayer.getInstance().play(MatchEnd)
        this.runForNext(3, false) // Delay between autonomous and teleop modes
            .then(() => this.teleopModeStart())
            .catch(() => {})
    }

    teleopModeStart() {
        void SoundPlayer.getInstance().play(MatchResume)
        this.setMatchModeType(MatchModeType.TELEOP)
        this.runForNext(this._matchModeConfig.teleopTime)
            .then(() => this.matchEnded())
            .catch(() => {})
    }

    endgameStart() {
        void SoundPlayer.getInstance().play(beep)
        this._matchModeType = MatchModeType.ENDGAME
        console.log("endgame start")
        this._endgame = true
    }

    async start(startTime: number | null, broadcast: boolean, useSpawnPositions: boolean) {
        startTime ??= Date.now() + 300 // Accounts for time it takes for robots to move to start positions and settle, and for multiplayer state to sync

        if (this._resultsModalId) {
            globalCloseModal(CloseType.ACCEPT, this._resultsModalId)
            this._resultsModalId = null
        }

        if (this._startTime !== null) {
            globalAddToast("warning", "Match mode already active")
            return
        }

        if (broadcast && World.multiplayerSystem) {
            World.multiplayerSystem.broadcast({
                type: "matchModeState",
                data: {
                    event: "start",
                    config: this._matchModeConfig,
                    moveRobots: useSpawnPositions,
                    startTime: World.multiplayerSystem.toServerTime(startTime),
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

        this.runForNext(0, false)
            .then(() => this.autonomousModeStart())
            .catch(() => {})
    }

    matchEnded() {
        void SoundPlayer.getInstance().play(MatchEnd)
        this.setMatchModeType(MatchModeType.MATCH_ENDED)

        const matchEvent = createMatchEventFromConfig(this._matchModeConfig)
        World.analyticsSystem?.event("Match End", matchEvent)
        this._resultsModalId = globalOpenModal(MatchResultsModal, undefined)
    }

    closeResultsModal() {
        if (this._resultsModalId) {
            globalCloseModal(CloseType.ACCEPT, this._resultsModalId)
            this._resultsModalId = null
        }
    }

    reset() {
        clearInterval(this._intervalId as number)
        this._startTime = null
        this._timeUsed = 0
        this._endgame = false
        this._cancelHandler?.()
        EventSystem.dispatch("TimeChangedEvent", { time: 0 })
        World.scoreTracker.resetScores()
    }

    abort(broadcast: boolean = true) {
        if (broadcast) {
            World.multiplayerSystem?.broadcast({
                type: "matchModeState",
                data: {
                    event: "cancel",
                },
            })
        }
        globalAddToast("info", "Match Mode Cancelled")
        this.sandboxModeStart()
    }
    sandboxModeStart() {
        this.setMatchModeType(MatchModeType.SANDBOX)
        this.reset()
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
