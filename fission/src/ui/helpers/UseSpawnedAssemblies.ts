import { useEffect, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"

const readSpawned = (): MirabufSceneObject[] => (World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll() : [])

export function useSpawnedAssemblies() {
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>(readSpawned)
    const [selectedConfigAssembly, setSelectedConfigAssembly] = useState<MirabufSceneObject | undefined>(undefined)

    useEffect(() => {
        // `spawned` is the assembly just added / null when one removed
        const sync = (spawned?: MirabufSceneObject | null) => {
            const current = readSpawned()
            setAssemblies(current)

            // selected assembly automatically when it is first spawned
            setSelectedConfigAssembly(
                prev => spawned ?? (prev && current.some(a => a.id === prev.id) ? prev : undefined)
            )
        }

        const unsubChange = EventSystem.listen("MirabufObjectChangeEvent", sync)
        const unsubSaved = EventSystem.listen("ConfigurationSavedEvent", () => sync())
        return () => {
            unsubChange()
            unsubSaved()
        }
    }, [])

    return { assemblies, selectedConfigAssembly, setSelectedConfigAssembly }
}
