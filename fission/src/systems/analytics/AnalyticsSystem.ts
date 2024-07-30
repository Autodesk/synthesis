import { consent, event, exception, init, setUserId, setUserProperty } from "@haensl/google-analytics"

import WorldSystem from "../WorldSystem"
import PreferencesSystem from "../preferences/PreferencesSystem"
import World from "../World"
import APS from "@/aps/APS"

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

    public constructor() {
        super()

        init({
            measurementId: "G-6XNCRD7QNC",
            debug: import.meta.env.DEV,
            anonymizeIp: true,
            sendPageViews: false,
            trackingConsent: PreferencesSystem.getGlobalPreference<boolean>("ReportAnalytics"),
        })

        PreferencesSystem.addEventListener(
            e => e.prefName == "ReportAnalytics" && this.ConsentUpdate(e.prefValue as boolean)
        )

        let betaCode = document.cookie.match(BETA_CODE_COOKIE_REGEX)?.[0]
        if (betaCode) {
            betaCode = betaCode.substring(betaCode.indexOf("=") + 1, betaCode.indexOf(";"))

            this.SetUserProperty("Beta Code", betaCode)
        } else {
            console.debug("No code match")
        }

        if (MOBILE_USER_AGENT_REGEX.test(navigator.userAgent)) {
            this.SetUserProperty("Is Mobile", "true")
        } else {
            this.SetUserProperty("Is Mobile", "false")
        }
    }

    public Event(name: string, params?: { [key: string]: string | number }) {
        event({ name: name, params: params ?? {} })
    }

    public Exception(description: string, fatal?: boolean) {
        exception({ description: description, fatal: fatal ?? false })
    }

    public SetUserId(id: string) {
        setUserId({ id: id })
    }

    public SetUserProperty(name: string, value: string) {
        setUserProperty({ name: name, value: value })
    }

    private ConsentUpdate(granted: boolean) {
        consent(granted)
    }

    private currentSampleInterval() {
        return 0.001 * (Date.now() - this._lastSampleTime)
    }

    public Update(_: number): void {
        if (Date.now() - this._lastSampleTime > SAMPLE_INTERVAL) {
            const interval = this.currentSampleInterval()
            const times = World.accumTimes
            this.PushPerformanceSample(interval, times)
            World.resetAccumTimes()

            const apsCalls = APS.numApsCalls
            this.PushApsCounts(interval, apsCalls)
            APS.resetNumApsCalls()

            this._lastSampleTime = Date.now()
        }
    }

    public Destroy(): void {
        const interval = this.currentSampleInterval()
        const times = World.accumTimes
        this.PushPerformanceSample(interval, times)
        const apsCalls = APS.numApsCalls
        this.PushApsCounts(interval, apsCalls)
    }

    private PushPerformanceSample(interval: number, times: AccumTimes) {
        if (times.frames > 0 && interval > 1.0) {
            this.Event("Performance Sample", {
                frames: times.frames,
                avgTotal: times.totalTime / times.frames,
                avgPhysics: times.physicsTime / times.frames,
                avgScene: times.sceneTime / times.frames,
                avgInput: times.inputTime / times.frames,
                avgSimulation: times.simulationTime / times.frames,
            })
        }
    }

    private PushApsCounts(interval: number, calls: Map<string, number>) {
        if (interval > 1.0) {
            const entries = Object.fromEntries([...calls.entries()].map(v => [v[0], v[1] / interval]))
            this.Event("APS Calls per Minute", entries)
        }
    }
}

export default AnalyticsSystem
