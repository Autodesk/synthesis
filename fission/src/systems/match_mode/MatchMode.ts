import SimulationSystem from "../simulation/SimulationSystem"
import { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { SoundPlayer } from "../sound/SoundPlayer"
import beep from "@/assets/sound-files/beep.wav"
import MatchEnd from "@/assets/sound-files/MatchEnd.wav"
import MatchResume from "@/assets/sound-files/MatchResume.wav"
import RobotDimensionTracker from "./RobotDimensionTracker"
import MatchStart from "@/assets/sound-files/MatchStart.wav"

export enum MatchModeType {
    SANDBOX = "Sandbox",
    AUTONOMOUS = "Autonomous",
    TELEOP = "Teleop",
    ENDGAME = "Endgame",
    MATCH_ENDED = "Match Ended",
}

// Default match mode timing values
export const DEFAULT_AUTONOMOUS_TIME = 15
export const DEFAULT_TELEOP_TIME = 135
export const DEFAULT_ENDGAME_TIME = 20
export const DEFAULT_IGNORE_ROTATION = true
export const DEFAULT_MAX_HEIGHT = Infinity
export const DEFAULT_HEIGHT_PENALTY = 2

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
    private _matchModeConfig: MatchModeConfig = {
        id: "default",
        name: "Default",
        isDefault: true,
        autonomousTime: DEFAULT_AUTONOMOUS_TIME,
        teleopTime: DEFAULT_TELEOP_TIME,
        endgameTime: DEFAULT_ENDGAME_TIME,
        ignoreRotation: DEFAULT_IGNORE_ROTATION,
        maxHeight: DEFAULT_MAX_HEIGHT,
        heightPenalty: DEFAULT_HEIGHT_PENALTY,
    }

    private constructor() {}

    static getInstance(): MatchMode {
        MatchMode._instance ??= new MatchMode()
        return MatchMode._instance
    }

    setMatchModeConfig(config: MatchModeConfig) {
        this._matchModeConfig = config
        RobotDimensionTracker.setConfigValues(config.ignoreRotation, config.maxHeight, config.heightPenalty)
    }

    startTimer(duration: number, functionCall: () => void, updateTimeLeft: boolean = true) {
        this._initialTime = duration
        this._timeLeft = duration

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) new UpdateTimeLeft(this._initialTime).dispatch()

        this._intervalId = window.setInterval(() => {
            this._timeLeft--

            if (this._timeLeft >= 0 && updateTimeLeft) {
                new UpdateTimeLeft(this._timeLeft).dispatch()
            }

            // Checks if endgame has started
            if (this._matchModeType === MatchModeType.TELEOP && this._timeLeft == this._matchModeConfig.endgameTime) {
                this.endgameStart()
            }

            if (this._timeLeft <= 0) {
                clearInterval(this._intervalId as number)
                functionCall()
            }
        }, 1000)
    }

    autonomousModeStart(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchStart)
        this.setMatchModeType(MatchModeType.AUTONOMOUS)
        this.startTimer(this._matchModeConfig.autonomousTime, () => this.autonomousModeEnd(openModal))
    }

    autonomousModeEnd(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchEnd)
        this.startTimer(3, () => this.teleopModeStart(openModal), false) // Delay between autonomous and teleop modes
    }

    teleopModeStart(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchResume)
        this.setMatchModeType(MatchModeType.TELEOP)
        this.startTimer(this._matchModeConfig.teleopTime, () => this.matchEnded(openModal))
    }

    endgameStart() {
        SoundPlayer.play(beep)
        this._matchModeType = MatchModeType.ENDGAME
        this._endgame = true
    }

    start(openModal: (modalName: string) => void) {
        this.autonomousModeStart(openModal)
        SimulationSystem.resetScores()
    }

    matchEnded(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchEnd)
        clearInterval(this._intervalId as number)
        this.setMatchModeType(MatchModeType.MATCH_ENDED)
        if (openModal) openModal("match-results")
    }

    sandboxModeStart() {
        this.setMatchModeType(MatchModeType.SANDBOX)
        clearInterval(this._intervalId as number)
        this._initialTime = 0
        this._timeLeft = 0
        new UpdateTimeLeft(this._timeLeft).dispatch()
        SimulationSystem.resetScores()
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

    public readonly autonomousTime: string

    constructor(autonomousTime: number) {
        super(UpdateTimeLeft.EVENT_KEY)
        this.autonomousTime = autonomousTime.toFixed(0)
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
