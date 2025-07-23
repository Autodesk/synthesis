import { consent, event, exception, init, setUserId, setUserProperty } from "@haensl/google-analytics"
import APS from "@/aps/APS"
import PreferencesSystem from "../preferences/PreferencesSystem"
import World from "../World"
import WorldSystem from "../WorldSystem"

const SAMPLE_INTERVAL = 60000 // 1 minute
const BETA_CODE_COOKIE_REGEX = /access_code=.*(;|$)/
const MOBILE_USER_AGENT_REGEX = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i

export interface AccumTimes {
    frames: number
    physicsTime: number
    sceneTime: number
    inputTime: number
    simulationTime: number
    totalTime: number
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

    public event(name: string, params?: { [key: string]: string | number }) {
        event({ name: name, params: params ?? {} })
    }

    public exception(description: string, fatal?: boolean) {
        exception({ description: description, fatal: fatal ?? false })
    }

    public setUserId(id: string) {
        setUserId({ id: id })
    }

    public setUserProperty(name: string, value: string) {
        setUserProperty({ name: name, value: value })
    }

    private consentUpdate(granted: boolean) {
        this._consent = granted
        consent(granted)

        this.sendMetaData()
    }

    private sendMetaData() {
        if (import.meta.env.DEV) {
            this.setUserProperty("Internal Traffic", "true")
        }

        if (!this._consent) {
            return
        }

        let betaCode = document.cookie.match(BETA_CODE_COOKIE_REGEX)?.[0]
        if (betaCode) {
            betaCode = betaCode.substring(betaCode.indexOf("=") + 1, betaCode.indexOf(";"))

            this.setUserProperty("Beta Code", betaCode)
        }

        if (MOBILE_USER_AGENT_REGEX.test(navigator.userAgent)) {
            this.setUserProperty("Is Mobile", "true")
        } else {
            this.setUserProperty("Is Mobile", "false")
        }
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
