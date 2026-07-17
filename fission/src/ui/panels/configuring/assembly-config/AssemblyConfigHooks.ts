import { useEffect } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"

/** Holds a physics pause for as long as the calling component is mounted (e.g. while editing a field-relative point). */
export function useHoldPhysicsPauseWhileMounted() {
    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])
}

/** Runs `callback` whenever the assembly config panel is saved (e.g. to persist in-progress edits). */
export function useConfigurationSavedListener(callback: () => void) {
    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", callback), [callback])
}
