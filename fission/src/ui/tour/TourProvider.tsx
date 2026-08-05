import { type ReactNode, useCallback, useEffect, useMemo, useRef, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { useIsMobile } from "@/ui/helpers/useIsMobile"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { Modal, Panel } from "@/ui/helpers/UIProviderHelpers"
import { TourContext, type TourContextValue } from "./TourProviderHelpers"
import type { AdvanceTrigger, TourAnchorId } from "./tourSteps"
import { TOUR_STEPS, tourIdOf } from "./tourSteps"

/** True when a panel matching the trigger is currently open. */
function isPanelOpen(panels: Panel<unknown, unknown>[], trigger: Extract<AdvanceTrigger, { kind: "panel-open" }>) {
    return panels.some(p => {
        if (tourIdOf(p.content) !== trigger.target) return false
        if (trigger.configMode === undefined) return true
        const custom = (p.props as unknown as { custom?: { configMode?: number } })?.custom
        return custom?.configMode === trigger.configMode
    })
}

/** True when the open modal matches the trigger. */
function isModalOpen(
    modal: Modal<unknown, unknown> | undefined,
    trigger: Extract<AdvanceTrigger, { kind: "modal-open" }>
) {
    return tourIdOf(modal?.content) === trigger.target
}

export const TourProvider: React.FC<{ children?: ReactNode }> = ({ children }) => {
    const { panels, modal } = useUIContext()
    const isMobile = useIsMobile()

    const [active, setActive] = useState(false)
    const [stepIndex, setStepIndex] = useState(0)

    // Anchor registry. The Map lives in a ref (stable identity); a version counter
    // triggers overlay re-resolution when elements mount/unmount (e.g. panels opening).
    const anchorsRef = useRef(new Map<TourAnchorId, HTMLElement>())
    const [anchorVersion, setAnchorVersion] = useState(0)

    const registerAnchor = useCallback((id: TourAnchorId, el: HTMLElement | null) => {
        const anchors = anchorsRef.current
        if (el) anchors.set(id, el)
        else anchors.delete(id)
        setAnchorVersion(v => v + 1)
    }, [])

    const getAnchor = useCallback((id: TourAnchorId) => anchorsRef.current.get(id) ?? null, [])

    const markSeen = useCallback(() => {
        PreferencesSystem.setUserPreference("HasSeenOnboardingTour", true)
        PreferencesSystem.savePreferences()
    }, [])

    const finish = useCallback(() => {
        setActive(false)
        setStepIndex(0)
        markSeen()
    }, [markSeen])

    const next = useCallback(() => {
        setStepIndex(i => {
            if (i >= TOUR_STEPS.length - 1) {
                finish()
                return i
            }
            return i + 1
        })
    }, [finish])

    const prev = useCallback(() => setStepIndex(i => Math.max(0, i - 1)), [])

    const skip = useCallback(() => finish(), [finish])

    // First-visit trigger. Desktop only, once per browser (persisted preference).
    const startedRef = useRef(false)
    useEffect(() => {
        if (isMobile) {
            // Never run on mobile; end the tour if the viewport crosses into mobile mid-run.
            setActive(prevActive => (prevActive ? false : prevActive))
            return
        }
        if (startedRef.current) return
        if (!PreferencesSystem.getUserPreference("HasSeenOnboardingTour")) {
            startedRef.current = true
            setActive(true)
        }
    }, [isMobile])

    // Auto-advance on `panel-open` / `modal-open` triggers, rising-edge only so an already-open
    // screen (e.g. the Library still showing after a field spawn) does not skip a step.
    const edgeRef = useRef<{ step: number; wasOpen: boolean }>({ step: -1, wasOpen: false })
    useEffect(() => {
        if (!active) return
        const trigger = TOUR_STEPS[stepIndex]?.advanceOn
        if (trigger?.kind !== "panel-open" && trigger?.kind !== "modal-open") return

        const openNow = trigger.kind === "panel-open" ? isPanelOpen(panels, trigger) : isModalOpen(modal, trigger)
        if (edgeRef.current.step !== stepIndex) {
            // Entering this step: seed the baseline; never advance on the same tick.
            edgeRef.current = { step: stepIndex, wasOpen: openNow }
            return
        }
        if (openNow && !edgeRef.current.wasOpen) {
            edgeRef.current.wasOpen = true
            next()
            return
        }
        edgeRef.current.wasOpen = openNow
    }, [active, stepIndex, panels, modal, next])

    // Auto-advance on `spawn` / `event` triggers via EventSystem.
    useEffect(() => {
        if (!active) return
        const trigger = TOUR_STEPS[stepIndex]?.advanceOn
        if (!trigger) return
        if (trigger.kind === "spawn") {
            return EventSystem.listen("MirabufObjectChangeEvent", obj => {
                if (obj && obj.miraType === trigger.miraType) next()
            })
        }
        if (trigger.kind === "event") {
            return EventSystem.listen(trigger.event, () => next())
        }
    }, [active, stepIndex, next])

    const value = useMemo<TourContextValue>(
        () => ({ active, stepIndex, next, prev, skip, registerAnchor, getAnchor, anchorVersion }),
        [active, stepIndex, next, prev, skip, registerAnchor, getAnchor, anchorVersion]
    )

    return <TourContext.Provider value={value}>{children}</TourContext.Provider>
}
