import SimulationSystem from "./simulation/SimulationSystem"
import { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"

export enum MatchModeType {
    Sandbox = 0,
    Autonomous = 1,
    Teleop = 2,
    MatchEnded = 3,
}

class MatchMode {
    private static instance: MatchMode
    private matchEnabled: boolean = false
    private endgame: boolean = false
    private matchModeType: MatchModeType = MatchModeType.Sandbox

    private initialTime: number = 0
    private timeLeft: number = 0
    private intervalId: number | null = null

    // Match Mode Config
    private matchModeConfig: MatchModeConfig = {
        id: "default",
        name: "Default",
        isDefault: true,
        autonomousTime: 15,
        teleopTime: 135,
        endgameTime: 20,
    }

    private constructor() {}

    static getInstance(): MatchMode {
        MatchMode.instance ??= new MatchMode()
        return MatchMode.instance
    }

    setMatchModeConfig(config: MatchModeConfig) {
        this.matchModeConfig = config
    }

    startTimer(duration: number, functionCall: () => void, updateTimeLeft: boolean = true) {
        this.initialTime = duration
        this.timeLeft = duration

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) new UpdateTimeLeft(this.initialTime).Dispatch()

        this.intervalId = window.setInterval(() => {
            this.timeLeft--

            if (this.timeLeft >= 0 && updateTimeLeft) {
                new UpdateTimeLeft(this.timeLeft).Dispatch()
            }

            // Checks if endgame has started
            if (this.matchModeType === MatchModeType.Teleop && this.timeLeft == this.matchModeConfig.endgameTime) {
                this.endgameStart()
            }

            if (this.timeLeft <= 0) {
                clearInterval(this.intervalId as number)
                functionCall()
            }
        }, 1000)
    }

    autonomousModeStart(openModal: (modalName: string) => void) {
        // TODO play the autonomous start sound
        this.matchModeType = MatchModeType.Autonomous
        this.startTimer(this.matchModeConfig.autonomousTime, () => this.teleopModeStart(openModal))
    }

    teleopModeStart(openModal: (modalName: string) => void) {
        // TODO play the teleop start sound
        this.matchModeType = MatchModeType.Teleop
        this.startTimer(this.matchModeConfig.teleopTime, () => this.matchEnded(openModal))
    }

    endgameStart() {
        // TODO play the endgame start sound
        this.endgame = true
    }

    start(openModal: (modalName: string) => void) {
        this.matchEnabled = true
        this.autonomousModeStart(openModal)
        SimulationSystem.ResetScores()
    }

    matchEnded(openModal: (modalName: string) => void) {
        // TODO play the match end sound
        clearInterval(this.intervalId as number)
        this.matchEnabled = false
        this.matchModeType = MatchModeType.MatchEnded
        if (openModal) openModal("match-results")
    }

    sandboxModeStart() {
        this.matchEnabled = false
        this.matchModeType = MatchModeType.Sandbox
        clearInterval(this.intervalId as number)
        this.initialTime = 0
        this.timeLeft = 0
        new UpdateTimeLeft(this.timeLeft).Dispatch()
    }

    isMatchEnabled(): boolean {
        return this.matchEnabled
    }

    isEndgame(): boolean {
        return this.endgame
    }

    getMatchModeType(): MatchModeType {
        return this.matchModeType
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

    public Dispatch(): void {
        window.dispatchEvent(this)
    }

    public static AddListener(func: (e: UpdateTimeLeft) => void) {
        window.addEventListener(UpdateTimeLeft.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: UpdateTimeLeft) => void) {
        window.removeEventListener(UpdateTimeLeft.EVENT_KEY, func as (e: Event) => void)
    }
}
