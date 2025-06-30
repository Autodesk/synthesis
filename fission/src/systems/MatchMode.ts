import SimulationSystem from "./simulation/SimulationSystem"
import { SoundPlayer } from "./sound/SoundPlayer"
import EndgameSonar from "@/assets/sound-files/EndgameSonar.wav"
import MatchStart from "@/assets/sound-files/MatchStart.wav"
import MatchEnd from "@/assets/sound-files/MatchEnd.wav"
import MatchResume from "@/assets/sound-files/MatchResume.wav"

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

    private constructor() {}

    static getInstance(): MatchMode {
        MatchMode.instance ??= new MatchMode()
        return MatchMode.instance
    }

    startTimer(duration: number, functionCall: () => void, updateTimeLeft: boolean = true) {
        this.initialTime = duration
        this.timeLeft = duration

        // Dispatch an event to update the time left in the UI
        if (updateTimeLeft) new UpdateTimeLeft(this.initialTime).Dispatch()

        this.intervalId = window.setInterval(() => {
            this.timeLeft--

            // Updates the time left in the UI
            if (this.timeLeft >= 0 && updateTimeLeft) {
                new UpdateTimeLeft(this.timeLeft).Dispatch()
            }

            // Checks if endgame has started
            if (this.matchModeType === MatchModeType.Teleop && this.timeLeft == 20) {
                this.endgameStart()
            }

            if (this.timeLeft <= 0) {
                clearInterval(this.intervalId as number)
                functionCall()
            }
        }, 1000)
    }

    autonomousModeStart(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchStart)
        this.matchModeType = MatchModeType.Autonomous
        this.startTimer(15, () => this.autonomousModeEnd(openModal))
    }

    autonomousModeEnd(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchEnd)
        this.startTimer(3, () => this.teleopModeStart(openModal), false) // Delay between autonomous and teleop modes
    }

    teleopModeStart(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchResume)
        this.matchModeType = MatchModeType.Teleop
        this.startTimer(135, () => this.matchEnded(openModal)) // 2 minutes and 15 seconds
    }

    endgameStart() {
        SoundPlayer.play(EndgameSonar)
    }

    start(openModal: (modalName: string) => void) {
        this.matchEnabled = true
        this.autonomousModeStart(openModal)
        SimulationSystem.ResetScores()
    }

    matchEnded(openModal: (modalName: string) => void) {
        SoundPlayer.play(MatchEnd)
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
