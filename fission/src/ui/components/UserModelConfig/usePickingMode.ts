import { useEffect, useState } from "react"
import type PartPickingMode from "@/systems/scene/PartPickingMode.ts"
import type { PartSelection } from "@/systems/scene/PartPickingMode.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"

export function usePickingMode<T extends PartSelection>(
    mode: PartPickingMode<T>,
    subscribe: (onChange: (items: T[]) => void) => () => void,
    sceneObject: MirabufSceneObject
): { enabled: boolean; setEnabled: (enabled: boolean | ((prev: boolean) => boolean)) => void; items: T[] } {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [items, setItems] = useState<T[]>([...mode.pending.values()])

    useEffect(() => subscribe(setItems), [subscribe])

    useEffect(() => {
        if (enabled) {
            mode.enable(sceneObject)
        } else {
            mode.disable()
        }
    }, [mode, enabled, sceneObject])

    // biome-ignore lint/correctness/useExhaustiveDependencies: Only want to run on mount
    useEffect(() => {
        setEnabled(mode.pendingCount == 0)
        return () => {
            mode.disable()
        }
    }, [])

    return { enabled, setEnabled, items }
}
