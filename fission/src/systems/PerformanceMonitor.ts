import { globalAddToast, globalOpenModal } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import SceneRenderer from "./scene/SceneRenderer"
import SettingsModal from "@/ui/modals/configuring/SettingsModal"

export class PerformanceMonitoringSystem {
    private static _isCritical: boolean = false
    private static _activeCount: number = 0
    private static _antiCount: number = 0
    private static _lastTime = performance.now()

    public static start() {
        setInterval(() => {
            this.reset()
        }, 15000)
    }
    public static update(_: number) {
        const time = performance.now() - this._lastTime
        const newIsCritical = time > 150
        if (newIsCritical == this._isCritical) {
            this._activeCount++
        } else {
            this._antiCount++
            if (this._antiCount > 10 && this._antiCount > 0.5 * this._activeCount) {
                this._isCritical = newIsCritical
                const oldActive = this._activeCount
                this._activeCount = this._antiCount
                this._antiCount = oldActive
                if (this._isCritical) {
                    PreferencesSystem.resetGraphicsPreferences()
                    SceneRenderer.changeCSMSettings(PreferencesSystem.getGraphicsPreferences())
                    globalOpenModal(SettingsModal, { initialTab: "graphics" })
                    globalAddToast("warning", "Performance Issues Detected", "Reverting to simple graphics")
                }
            }
        }

        this._lastTime = performance.now()
    }

    public static reset() {
        this._activeCount = 0
        this._antiCount = 0
    }

    public static destroy() {}
}
