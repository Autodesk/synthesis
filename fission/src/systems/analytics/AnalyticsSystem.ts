import googleAnalytics from "@analytics/google-analytics"
import Analytics, { AnalyticsInstance } from "analytics"
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
    private _analytics: AnalyticsInstance
    private _userId: string | null = null

    public constructor() {
        super()

        this._consent = PreferencesSystem.getGlobalPreference("ReportAnalytics")
        this._analytics = Analytics({
            app: "synthesis-fission",
            version: COMMIT_HASH,
            plugins: [
                googleAnalytics({
                    measurementIds: ["G-6XNCRD7QNC"],
                    anonymize_ip: true,
                }),
            ],
        })
        PreferencesSystem.addPreferenceEventListener("ReportAnalytics", e => this.consentUpdate(e.prefValue))
        this._analytics.ready(() => {
            console.log(this._analytics)
        })
        this.sendMetaData()
        setTimeout(() => this._analytics.page())
    }

    public event(name: string, params?: Record<string, unknown>) {
        if (!this._consent) return
        console.log("SENDING", name)
        setTimeout(() => this._analytics.track(name, params))
    }

    public registerUser() {
        if (!this._consent) return

        this._userId = window.localStorage.getItem("AnalyticsKey")
        if (this._userId == null) {
            this._userId = crypto.randomUUID()
            window.localStorage.setItem("AnalyticsKey", this._userId)
        }
    }

    public exception(description: string, fatal: boolean = false) {
        this.event("exception", { description: description, fatal: fatal })
    }

    private consentUpdate(granted: boolean) {
        this._consent = granted
        this.sendMetaData()
    }

    private sendMetaData() {
        if (!this._consent) return
        if (!this._userId) this.registerUser()

        const properties: Record<string, unknown> = {}
        properties["Internal Traffic"] = import.meta.env.DEV

        let betaCode = document.cookie.match(BETA_CODE_COOKIE_REGEX)?.[0]
        if (betaCode) {
            betaCode = betaCode.substring(betaCode.indexOf("=") + 1, betaCode.indexOf(";"))
            properties["Beta Code"] = betaCode
        }
        properties["Is Mobile"] = MOBILE_USER_AGENT_REGEX.test(navigator.userAgent)
        setTimeout(() => this._analytics.identify(this._userId!, properties))
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
