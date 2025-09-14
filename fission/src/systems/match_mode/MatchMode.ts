import beep from "@/assets/sound-files/beep.wav"
import MatchEnd from "@/assets/sound-files/MatchEnd.wav"
import MatchResume from "@/assets/sound-files/MatchResume.wav"
import MatchStart from "@/assets/sound-files/MatchStart.wav"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs.ts"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
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
        new MatchStateChangeEvent(val).dispatch()
    }

    private _initialTime: number = 0
    private _timeLeft: number = 0
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

    async runTimer(duration: number, updateTimeLeft: boolean = true) {
        this._initialTime = duration
        this._timeLeft = duration

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) new UpdateTimeLeft(this._initialTime).dispatch()
        return new Promise<void>(res => {
            this._intervalId = window.setInterval(() => {
                this._timeLeft--

                if (this._timeLeft >= 0 && updateTimeLeft) {
                    new UpdateTimeLeft(this._timeLeft).dispatch()
                }

                // Checks if endgame has started
                if (
                    this._matchModeType === MatchModeType.TELEOP &&
                    this._timeLeft == this._matchModeConfig.endgameTime
                ) {
                    this.endgameStart()
                }

                if (this._timeLeft <= 0) {
                    console.log("resolving")
                    res()
                }
            }, 1000)
        }).finally(() => {
            clearInterval(this._intervalId as number)
        })
    }

    autonomousModeStart() {
        void SoundPlayer.getInstance().play(MatchStart)
        this.setMatchModeType(MatchModeType.AUTONOMOUS)
        this.runTimer(this._matchModeConfig.autonomousTime).then(() => this.autonomousModeEnd())
    }

    autonomousModeEnd() {
        void SoundPlayer.getInstance().play(MatchEnd)
        this.runTimer(3, false).then(() => this.teleopModeStart()) // Delay between autonomous and teleop modes
    }

    teleopModeStart() {
        void SoundPlayer.getInstance().play(MatchResume)
        this.setMatchModeType(MatchModeType.TELEOP)
        this.runTimer(this._matchModeConfig.teleopTime).then(() => this.matchEnded())
    }

    endgameStart() {
        void SoundPlayer.getInstance().play(beep)
        this._matchModeType = MatchModeType.ENDGAME
        console.log("endgame start")
        this._endgame = true
    }

    async start(broadcast = true, useSpawnPositions: boolean) {
        if (broadcast) {
            await World.multiplayerSystem?.broadcast({
                type: "matchModeState",
                data: { event: "start", config: this._matchModeConfig, moveRobots: useSpawnPositions },
            })
            console.log("sent multiplayer")
        }
        if (useSpawnPositions) {
            World.getOwnRobots().forEach(obj => obj.moveToSpawnLocation())
        }
        this.autonomousModeStart()
        ScoreTracker.resetScores()
        RobotDimensionTracker.matchStart()

        const matchEvent = createMatchEventFromConfig(this._matchModeConfig)
        World.analyticsSystem?.event("Match Start", matchEvent)
    }

    matchEnded() {
        void SoundPlayer.getInstance().play(MatchEnd)
        clearInterval(this._intervalId as number)
        this.setMatchModeType(MatchModeType.MATCH_ENDED)

        const matchEvent = createMatchEventFromConfig(this._matchModeConfig)
        World.analyticsSystem?.event("Match End", matchEvent)
        globalOpenModal(MatchResultsModal, undefined)
    }

    sandboxModeStart() {
        this.setMatchModeType(MatchModeType.SANDBOX)
        clearInterval(this._intervalId as number)
        this._initialTime = 0
        this._timeLeft = 0
        new UpdateTimeLeft(this._timeLeft).dispatch()
        ScoreTracker.resetScores()
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

export class UpdateTimeLeft extends Event {
    public static readonly EVENT_KEY = "UpdateTimeLeft"

    public readonly time: string

    constructor(time: number) {
        super(UpdateTimeLeft.EVENT_KEY)
        this.time = time.toFixed(0)
    }

    public dispatch(): void {
        window.dispatchEvent(this)
    }

    public static addListener(func: (e: UpdateTimeLeft) => void) {
        window.addEventListener(UpdateTimeLeft.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: UpdateTimeLeft) => void) {
        window.removeEventListener(UpdateTimeLeft.EVENT_KEY, func as (e: Event) => void)
    }
}

export class MatchStateChangeEvent extends Event {
    public static readonly EVENT_KEY = "MatchEnd"

    public readonly matchModeType: MatchModeType
    constructor(matchModeType: MatchModeType) {
        super(MatchStateChangeEvent.EVENT_KEY)
        this.matchModeType = matchModeType
    }

    public dispatch(): void {
        window.dispatchEvent(this)
    }

    public static addListener(func: (e: MatchStateChangeEvent) => void) {
        window.addEventListener(MatchStateChangeEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: MatchStateChangeEvent) => void) {
        window.removeEventListener(MatchStateChangeEvent.EVENT_KEY, func as (e: Event) => void)
    }
}
