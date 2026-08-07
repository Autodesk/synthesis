import { useEffect } from "react"
import EventSystem from "@/systems/EventSystem.ts"

/** Runs `callback` whenever the assembly config panel is saved (e.g. to persist in-progress edits). */
export function useConfigurationSavedListener(callback: () => void) {
    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", callback), [callback])
}
