import { useEffect, useId } from "react"
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
