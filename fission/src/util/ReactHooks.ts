import { useEffect, useId } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"

export function useHoldPhysicsPause() {
    const id = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(id)
        return () => {
            World.physicsSystem.releasePause(id)
        }
    })
}

/** Runs `callback` whenever the assembly config panel is saved (e.g. to persist in-progress edits). */
export function useConfigurationSavedListener(callback: () => void) {
    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", callback), [callback])
}
