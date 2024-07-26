import { consent, event, exception, init } from "@haensl/google-analytics"

import WorldSystem from "../WorldSystem"
import PreferencesSystem from "../preferences/PreferencesSystem"
import World from "../World"
import APS from "@/aps/APS"

const SAMPLE_INTERVAL = 60000 // 1 minute

export interface AccumTimes {
    frames: number,
    physicsTime: number,
    sceneTime: number,
    inputTime: number,
    simulationTime: number,
    totalTime: number
}

class AnalyticsSystem extends WorldSystem {
    
    private _lastSampleTime = Date.now()
    
    public constructor() {
        super()

        init({
            measurementId: "G-6XNCRD7QNC",
            debug: import.meta.env.DEV,
            sendPageViews: true,
            trackingConsent: PreferencesSystem.getGlobalPreference<boolean>("ReportAnalytics"),
        })

        PreferencesSystem.addEventListener(
            e => e.prefName == "ReportAnalytics" && this.ConsentUpdate(e.prefValue as boolean)
        )
    }

    public Event(name: string, params?: { [key: string]: string | number }) {
        event({ name: name, params: params ?? {} })
    }

    public Exception(description: string, fatal?: boolean) {
        exception({ description: description, fatal: fatal ?? false })
    }

    private ConsentUpdate(granted: boolean) {
        consent(granted)
    }

    public Update(_: number): void {
        if (Date.now() - this._lastSampleTime > SAMPLE_INTERVAL) {
            const times = World.accumTimes
            this.PushPerformanceSample(times)
            World.resetAccumTimes()

            const apsCalls = APS.numApsCalls
            this.PushApsCounts(0.001 * (Date.now() - this._lastSampleTime), apsCalls)
            APS.resetNumApsCalls()

            this._lastSampleTime = Date.now()
        }
    }

    public Destroy(): void {
        const times = World.accumTimes
        this.PushPerformanceSample(times)
        const apsCalls = APS.numApsCalls
        this.PushApsCounts(0.001 * (Date.now() - this._lastSampleTime), apsCalls)
    }

    private PushPerformanceSample(times: AccumTimes) {
        if (times.frames > 0) {
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
        if (interval > 0) {
            const entries = Object.fromEntries([...calls.entries()].map(v => [v[0], v[1] / interval]))
            entries.total = [...calls.values()].reduce((a, b) => a + b)
            this.Event("APS Calls per Minute", entries)
        }
    }
}

export default AnalyticsSystem
