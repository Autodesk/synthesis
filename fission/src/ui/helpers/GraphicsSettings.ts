import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

export type GraphicsPreset = "Fast" | "Balanced" | "Fancy"

export const GRAPHICS_PRESETS: Record<
    GraphicsPreset,
    {
        lightIntensity: number
        fancyShadows: boolean
        maxFar: number
        cascades: number
        shadowMapSize: number
        antiAliasing: boolean
    }
> = {
    Fast: {
        lightIntensity: 3,
        fancyShadows: false,
        maxFar: 20,
        cascades: 3,
        shadowMapSize: 0,
        antiAliasing: false,
    },
    Balanced: {
        lightIntensity: 5,
        fancyShadows: true,
        maxFar: 30,
        cascades: 4,
        shadowMapSize: 2048,
        antiAliasing: false,
    },
    Fancy: {
        lightIntensity: 8,
        fancyShadows: true,
        maxFar: 40,
        cascades: 6,
        shadowMapSize: 8192,
        antiAliasing: true,
    },
}

export const isMobileDevice = (): boolean => {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
}

export const isLowEndDevice = (): boolean => {
    if (isMobileDevice()) return true

    const memory =
        typeof navigator !== "undefined" && "deviceMemory" in navigator
            ? (navigator as Navigator & { deviceMemory?: number }).deviceMemory
            : undefined
    if (memory !== undefined && memory <= 4) return true // 4GB or less RAM

    const cores = navigator.hardwareConcurrency
    if (cores && cores <= 2) return true // 2 cores or less

    const userAgent = navigator.userAgent.toLowerCase()
    if (userAgent.includes("android") && (userAgent.includes("lite") || userAgent.includes("go"))) return true

    return false
}

export const autoOptimizeGraphics = (toastType: "long" | "short" | "none" = "none"): GraphicsPreset => {
    if (isLowEndDevice()) {
        if (toastType === "long") {
            globalAddToast?.(
                "info",
                "Graphics Optimized",
                "We've set your graphics to 'Fast' mode for optimal performance on your device. You can change this in Graphics Settings."
            )
        } else if (toastType === "short") {
            globalAddToast?.("info", "Success", "Auto-optimized graphics to 'Fast'.")
        }
        return "Fast"
    } else {
        if (toastType === "long") {
            globalAddToast?.(
                "info",
                "Graphics Optimized",
                "We've set your graphics to 'Balanced' mode for optimal performance on your device. You can change this in Graphics Settings."
            )
        } else if (toastType === "short") {
            globalAddToast?.("info", "Success", "Auto-optimized graphics to 'Balanced'.")
        }
        return "Balanced"
    }
}

export const applyInitialGraphicsSettings = (): void => {
    // Check if graphics optimization has already been applied
    const optimizationApplied = PreferencesSystem.getGlobalPreference("GraphicsOptimizationApplied")

    // If optimization hasn't been applied yet and device should use fast mode, apply fast settings
    if (!optimizationApplied) {
        const presetToApply = autoOptimizeGraphics("long")
        const settings = GRAPHICS_PRESETS[presetToApply]
        PreferencesSystem.getGraphicsPreferences().lightIntensity = settings.lightIntensity
        PreferencesSystem.getGraphicsPreferences().fancyShadows = settings.fancyShadows
        PreferencesSystem.getGraphicsPreferences().maxFar = settings.maxFar
        PreferencesSystem.getGraphicsPreferences().cascades = settings.cascades
        PreferencesSystem.getGraphicsPreferences().shadowMapSize = settings.shadowMapSize
        PreferencesSystem.getGraphicsPreferences().antiAliasing = settings.antiAliasing

        // Mark that optimization has been applied
        PreferencesSystem.setGlobalPreference("GraphicsOptimizationApplied", true)
        PreferencesSystem.savePreferences()
    }
}
