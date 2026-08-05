import { useEffect, useId } from "react"
import World from "@/systems/World.ts"

export function usePause() {
    const pauseHandle = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(pauseHandle)
        return () => {
            World.physicsSystem.releasePause(pauseHandle)
        }
    }, [pauseHandle])
}
