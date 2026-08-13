import { type ReactNode, useCallback, useEffect, useMemo, useRef, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import InputSystem, { ESCAPE_PRIORITY } from "@/systems/input/InputSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import World from "@/systems/World.ts"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useIsMobile } from "@/ui/helpers/useIsMobile"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { hasPendingSpawn } from "@/ui/modals/mirabuf/LibrarySpawnActions"
import type { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import { advanceConditionMet, reconcile, type TourRuntime, type TourSnapshot } from "./TourConditions"
import { TourContext, type TourContextValue } from "./TourProviderHelpers"
import type { TourAnchorId } from "./TourSteps"
import { TOUR_STEPS, tourIdOf } from "./TourSteps"

const readWorld = () => ({
    fieldCount: World.isAlive && World.sceneRenderer.mirabufSceneObjects.getField() !== undefined ? 1 : 0,
    robotCount: World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getRobots().length : 0,
})

export const TourProvider: React.FC<{ children?: ReactNode }> = ({ children }) => {
    const { panels, modal, addToast } = useUIContext()
    const { appMode } = useStateContext()
    const isMobile = useIsMobile()

    const [active, setActive] = useState(false)
    const [stepIndex, setStepIndex] = useState(0)
    const [world, setWorld] = useState(readWorld)

    useEffect(() => EventSystem.listen("MirabufObjectChangeEvent", () => setWorld(readWorld())), [])

    const [spawnPending, setSpawnPending] = useState(hasPendingSpawn)

    useEffect(() => EventSystem.listen("SpawnPendingChangeEvent", setSpawnPending), [])

    // Anchor registry. The Map lives in a ref (stable identity); a version counter
    // triggers overlay re-resolution when elements mount/unmount (e.g. panels opening).
    const anchorsRef = useRef(new Map<TourAnchorId, HTMLElement>())
    const [anchorVersion, setAnchorVersion] = useState(0)

    const registerAnchor = useCallback((id: TourAnchorId, element: HTMLElement | null) => {
        const anchors = anchorsRef.current
        if (element) anchors.set(id, element)
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
            setActive(false)
            return
        }
        if (startedRef.current) return
        if (!PreferencesSystem.getUserPreference("HasSeenOnboardingTour")) {
            startedRef.current = true
            setActive(true)
        }
    }, [isMobile])

    const runtimeRef = useRef<TourRuntime>({ step: -1 })

    useEffect(() => {
        if (isMobile) return
        return EventSystem.listen("TourRestartEvent", () => {
            runtimeRef.current = { step: -1 }
            setStepIndex(0)
            setActive(true)
        })
    }, [isMobile])

    const snapshot = useMemo<TourSnapshot>(
        () => ({
            modal: tourIdOf(modal?.content),
            panels: panels.map(p => ({
                id: tourIdOf(p.content),
                configMode: (p.props.custom as { configMode?: ConfigMode } | undefined)?.configMode,
            })),
            appMode,
            ...world,
            spawnPending,
        }),
        [modal, panels, appMode, world, spawnPending]
    )

    useEffect(() => {
        if (!active) return
        const result = reconcile(stepIndex, snapshot, runtimeRef.current)
        runtimeRef.current = result.runtime

        if (result.toast) addToast("warning", result.toast)
        if (result.stepIndex >= TOUR_STEPS.length) finish()
        else if (result.stepIndex !== stepIndex) setStepIndex(result.stepIndex)
    }, [active, stepIndex, snapshot, addToast, finish])

    const canAdvance = active ? advanceConditionMet(TOUR_STEPS[stepIndex], snapshot) : true

    useEffect(() => {
        if (!active) return
        // tour has the highest priority for 'esc'. Then next is modals and panels
        return InputSystem.addEscapeHandler(() => {
            skip()
            return true
        }, ESCAPE_PRIORITY.TOUR)
    }, [active, skip])

    const value = useMemo<TourContextValue>(
        () => ({ active, stepIndex, canAdvance, next, prev, skip, registerAnchor, getAnchor, anchorVersion }),
        [active, stepIndex, canAdvance, next, prev, skip, registerAnchor, getAnchor, anchorVersion]
    )

    return <TourContext.Provider value={value}>{children}</TourContext.Provider>
}
