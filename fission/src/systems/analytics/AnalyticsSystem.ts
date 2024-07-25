import { consent, event, exception, init } from "@haensl/google-analytics"

import WorldSystem from "../WorldSystem"
import PreferencesSystem from "../preferences/PreferencesSystem"

class AnalyticsSystem extends WorldSystem {
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

    public Update(_: number): void {}
    public Destroy(): void {}
}

export default AnalyticsSystem
