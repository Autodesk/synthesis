import { consent, event, exception, init, setUserId, setUserProperty } from "@haensl/google-analytics"
import APS from "@/aps/APS"
import PreferencesSystem from "../preferences/PreferencesSystem"
import World from "../World"
import WorldSystem from "../WorldSystem"

const SAMPLE_INTERVAL = 60000 // 1 minute
const BETA_CODE_COOKIE_REGEX = /access_code=.*(;|$)/
const MOBILE_USER_AGENT_REGEX = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i

declare const GIT_COMMIT: string

export interface AccumTimes {
    frames: number
    physicsTime: number
    sceneTime: number
    inputTime: number
    simulationTime: number
    totalTime: number
}
type MiraEvent = {
    key?: string
    type?: "robot" | "field"
    assemblyName?: string
    /**
     * Size (in bytes) of the mirabuf file
     */
    fileSize?: number
}
export type MatchEvent = {
    matchName: string
    isDefault?: boolean
    autonomousTime: number
    teleopTime: number
    endgameTime: number
    // Height penalty configuration
    hasHeightPenalty?: boolean
    maxHeight?: number
    heightLimitPenalty?: number
    ignoreRotation?: boolean
    // Side extension penalty configuration
    hasSideExtensionPenalty?: boolean
    sideMaxExtension?: number
    sideExtensionPenalty?: number
}

export interface AnalyticsEvents {
    "Performance Sample": {
        frames: number
        avgTotal: number
        avgPhysics: number
        avgScene: number
        avgInput: number
        avgSimulation: number
    }

    // APS Events
    "APS Calls per Minute": unknown
    "APS Login": unknown
    "APS Download": MiraEvent

    // Cache Events
    "Cache Get": MiraEvent
    "Cache Store": MiraEvent
    "Cache Remove": MiraEvent

    // Remote Download Events
    "Remote Download": MiraEvent
    "Local Upload": MiraEvent

    // Devtool Cache Events
    "Devtool Cache Persist": MiraEvent

    // Scheme Events
    "Scheme Applied": {
        isCustomized: boolean
        schemeName: string
    }

    // Match Mode Events
    "Match Start": MatchEvent
    "Match End": MatchEvent
    "Match Mode Config Created": MatchEvent
    "Match Mode Config Uploaded": MatchEvent

    // Graphics Settings Event
    "Graphics Settings": {
        lightIntensity: number
        fancyShadows: boolean
        maxFar: number
        cascades: number
        shadowMapSize: number
        antiAliasing: boolean
    }

    // Scene Interaction Events
    "Drag Mode Enabled": unknown
    "Drag Mode Disabled": {
        durationSeconds: number
    }

    // Main Menu Events
    "Mode Selected": {
        mode: string
    }

    "Command Executed": {
        command: string
    }
}

class AnalyticsSystem extends WorldSystem {
    private _lastSampleTime = Date.now()
    private _consent: boolean

    public constructor() {
        super()

        this._consent = PreferencesSystem.getGlobalPreference("ReportAnalytics")
        init({
            measurementId: "G-6XNCRD7QNC",
            debug: import.meta.env.DEV,
            anonymizeIp: true,
            sendPageViews: false,
            trackingConsent: this._consent,
        })

        PreferencesSystem.addPreferenceEventListener("ReportAnalytics", e => this.consentUpdate(e.prefValue))

        this.sendMetaData()
    }

    public event<K extends keyof AnalyticsEvents>(name: K, params?: AnalyticsEvents[K]) {
        console.log("AnalyticsEvent", name, params)
        event({ name: name, params: params ?? {} })
    }

    public exception(description: string, fatal?: boolean) {
        exception({ description: description, fatal: fatal ?? false })
    }

    public setUserId(id: string) {
        setUserId({ id: id })
    }

    public setUserProperty(name: string, value: unknown) {
        if (name.includes(" ")) {
            console.warn("GA user property names must not contain spaces")
            return
        }
        setUserProperty({ name: name, value: value })
    }

    private consentUpdate(granted: boolean) {
        this._consent = granted
        consent(granted)

        this.sendMetaData()
    }

    private sendMetaData() {
        this.setUserProperty("isInternal", import.meta.env.DEV)
        this.setUserProperty("commit", GIT_COMMIT)

        if (!this._consent) {
            return
        }

        let betaCode = document.cookie.match(BETA_CODE_COOKIE_REGEX)?.[0]
        if (betaCode) {
            betaCode = betaCode.substring(betaCode.indexOf("=") + 1, betaCode.indexOf(";"))
            this.setUserProperty("betaCode", betaCode)
        }
        this.setUserProperty("isMobile", MOBILE_USER_AGENT_REGEX.test(navigator.userAgent))
    }

    private currentSampleInterval() {
        return 0.001 * (Date.now() - this._lastSampleTime)
    }

    public update(_: number): void {
        if (Date.now() - this._lastSampleTime > SAMPLE_INTERVAL) {
            const interval = this.currentSampleInterval()
            const times = World.accumTimes
            this.pushPerformanceSample(interval, times)
            World.resetAccumTimes()

            const apsCalls = APS.numApsCalls
            this.pushAPSCounts(interval, apsCalls)
            APS.resetNumApsCalls()

            this._lastSampleTime = Date.now()
        }
    }

    public destroy(): void {
        const interval = this.currentSampleInterval()
        const times = World.accumTimes
        this.pushPerformanceSample(interval, times)
        const apsCalls = APS.numApsCalls
        this.pushAPSCounts(interval, apsCalls)
    }

    private pushPerformanceSample(interval: number, times: AccumTimes) {
        if (times.frames > 0 && interval > 1.0) {
            this.event("Performance Sample", {
                frames: times.frames,
                avgTotal: times.totalTime / times.frames,
                avgPhysics: times.physicsTime / times.frames,
                avgScene: times.sceneTime / times.frames,
                avgInput: times.inputTime / times.frames,
                avgSimulation: times.simulationTime / times.frames,
            })
        }
    }

    private pushAPSCounts(interval: number, calls: Map<string, number>) {
        if (interval > 1.0) {
            const entries = Object.fromEntries([...calls.entries()].map(v => [v[0], v[1] / interval]))
            this.event("APS Calls per Minute", entries)
        }
    }
}

export default AnalyticsSystem
