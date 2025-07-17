import WorldSystem from "@/systems/WorldSystem.ts"
import { globalAddToast, globalOpenPanel } from "@/components/GlobalUIControls.ts"
import World from "@/systems/World.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import GraphicsSettingsPanel from "@/ui/panels/GraphicsSettingsPanel"
import React from "react"

export class PerformanceMonitoringSystem extends WorldSystem {
    isCritical: boolean = false
    activeCount: number = 0
    antiCount: number = 0
    lastTime = performance.now()
    constructor() {
        super()
        setInterval(() => {
            this.reset()
        }, 15000)
    }
    public update(_: number) {
        const time = performance.now() - this.lastTime
        const newIsCritical = time > 150
        if (newIsCritical == this.isCritical) {
            this.activeCount++
        } else {
            this.antiCount++
            if (this.antiCount > 10 && this.antiCount > 0.5 * this.activeCount) {
                this.isCritical = newIsCritical
                const oldActive = this.activeCount
                this.activeCount = this.antiCount
                this.antiCount = oldActive
                if (this.isCritical) {
                    PreferencesSystem.resetGraphicsPreferences()
                    World.sceneRenderer.changeCSMSettings(PreferencesSystem.getGraphicsPreferences())
                    globalOpenPanel(React.createElement(GraphicsSettingsPanel))
                    globalAddToast("warning", "Performance Issues Detected", "Reverting to simple graphics")
                }
            }
        }

        this.lastTime = performance.now()
    }

    public reset() {
        this.activeCount = 0
        this.antiCount = 0
    }

    public destroy() {}
}
