import { useEffect, useState } from "react"
import type PartPickingMode from "@/systems/scene/PartPickingMode.ts"
import type { PartSelection } from "@/systems/scene/PartPickingMode.ts"

/**
 * Shares the enable-toggle/subscribe wiring common to every picking-mode subpanel (WheelAssignment,
 * DeleteParts): tracks whether picking is active, mirrors it onto the mode, seeds + subscribes to the
 * mode's pending selection list. Callers own their own row rendering.
 */
export function usePickingMode<T extends PartSelection>(
    mode: PartPickingMode<T>,
    subscribe: (onChange: (items: T[]) => void) => () => void
): { enabled: boolean; setEnabled: (enabled: boolean | ((prev: boolean) => boolean)) => void; items: T[] } {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [items, setItems] = useState<T[]>([...mode.pending.values()])

    useEffect(() => subscribe(setItems), [subscribe])

    useEffect(() => {
        mode.enabled = enabled
        return () => {
            mode.enabled = false
        }
    }, [mode, enabled])

    useEffect(() => {
        setEnabled(mode.pendingCount == 0)
        // Intentionally mount-only: seeds the toggle from pre-existing pending state, not a live sync.
    }, [])

    return { enabled, setEnabled, items }
}
