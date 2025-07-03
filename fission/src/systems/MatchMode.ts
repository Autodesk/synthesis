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
    private matchModeType: MatchModeType = MatchModeType.Sandbox

    private initialTime: number = 0
    private timeLeft: number = 0
    private intervalId: number | null = null

    // Match Mode Config
    private autonomousTime: number = 15
    private teleopTime: number = 135

    private constructor() {}

    static getInstance(): MatchMode {
        MatchMode.instance ??= new MatchMode()
        return MatchMode.instance
    }

    setMatchModeConfig(config: MatchModeConfig) {
        this.autonomousTime = config.autonomousTime
        this.teleopTime = config.teleopTime
    }

    startTimer(duration: number, openModal: (modalName: string) => void) {
        this.initialTime = duration
        this.timeLeft = duration

        // Dispatch an event to update the time left in the UI
        new UpdateTimeLeft(this.initialTime).Dispatch()

        this.intervalId = window.setInterval(() => {
            this.timeLeft--

            if (this.timeLeft > 0) {
                new UpdateTimeLeft(this.timeLeft).Dispatch()
            }

            if (this.timeLeft <= 0) {
                this.stop(openModal)
            }
        }, 1000)
    }

    autonomousModeStart(openModal: (modalName: string) => void) {
        // TODO play the autonomous start sound
        this.matchModeType = MatchModeType.Autonomous
        this.startTimer(this.autonomousTime, openModal)
    }

    teleopModeStart(openModal: (modalName: string) => void) {
        // TODO play the teleop start sound
        this.matchModeType = MatchModeType.Teleop
        this.startTimer(this.teleopTime, openModal)
    }

    start(openModal: (modalName: string) => void) {
        this.matchEnabled = true
        this.autonomousModeStart(openModal)
        SimulationSystem.ResetScores()
    }

    stop(openModal: (modalName: string) => void) {
        clearInterval(this.intervalId as number)
        if (this.matchModeType === MatchModeType.Autonomous) {
            // Autonomous Mode Ended, Start Teleop Mode
            this.teleopModeStart(openModal)
        } else {
            // Teleop Mode Ended, Match Ended
            this.matchEnabled = false
            this.matchModeType = MatchModeType.MatchEnded
            if (openModal) openModal("match-results")
        }
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
