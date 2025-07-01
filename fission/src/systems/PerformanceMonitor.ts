import WorldSystem from "@/systems/WorldSystem.ts"
import { Global_AddToast, Global_OpenPanel } from "@/components/GlobalUIControls.ts"
import World from "@/systems/World.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"

export class PerformanceMonitorSystem extends WorldSystem {
    isCritical: boolean = false
    activeCount: number = 0
    antiCount: number = 0
    lastTime = performance.now()
    constructor() {
        super()
        setInterval(() => {
            this.Reset()
        }, 15000)
    }
    public Update(_: number) {
        const time = performance.now() - this.lastTime
        const newIsCritical = time > 150
        if (newIsCritical == this.isCritical) {
            this.activeCount++
        } else {
            this.antiCount++
            if (this.antiCount <= 10 || this.antiCount <= 0.5 * this.activeCount) return
            
            this.isCritical = newIsCritical
            const oldActive = this.activeCount
            this.activeCount = this.antiCount
            this.antiCount = oldActive
            if (this.isCritical) {
                PreferencesSystem.resetGraphicsPreferences()
                World.SceneRenderer.changeCSMSettings(PreferencesSystem.getGraphicsPreferences())
                Global_OpenPanel?.("graphics-settings")
                Global_AddToast?.("warning", "Performance Issues Detected", "Reverting to simple graphics")
            }
        }

        this.lastTime = performance.now()
    }

    public Reset() {
        this.activeCount = 0
        this.antiCount = 0
    }

    public Destroy() {}
}
