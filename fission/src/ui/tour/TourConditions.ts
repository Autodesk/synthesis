import type { AppMode } from "@/systems/AppMode"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import type { TourCondition, TourStep, TourTargetId } from "./TourSteps"
import { TOUR_STEPS } from "./TourSteps"

export interface TourSnapshot {
    modal?: TourTargetId
    panels: { id?: TourTargetId; configMode?: ConfigMode }[]
    appMode: AppMode
    fieldCount: number
    robotCount: number
    spawnPending: boolean
}

const hasPanel = (snapshot: TourSnapshot, id: TourTargetId, configMode?: ConfigMode) =>
    snapshot.panels.some(p => p.id === id && (configMode === undefined || p.configMode === configMode))

interface TourConditionDef {
    holds: (snapshot: TourSnapshot) => boolean
    hint: string
}

export const CONDITIONS: Record<TourCondition, TourConditionDef> = {
    libraryOpen: {
        holds: s => s.modal === "LibraryModal",
        hint: "Open the assets library with the Add Assembly button to continue.",
    },
    field: {
        holds: s => s.fieldCount > 0,
        hint: "Spawn a field from the library to continue.",
    },
    robot: {
        holds: s => s.robotCount > 0,
        hint: "Spawn a robot from the library to continue.",
    },
    setupPanel: {
        holds: s => hasPanel(s, "InitialConfigPanel"),
        hint: "Finish Assembly Setup to continue.",
    },
    intakePanel: {
        holds: s => hasPanel(s, "ConfigurePanel", ConfigMode.INTAKE),
        hint: "Open the intake configuration from the top bar to continue.",
    },
    configureMode: {
        holds: s => s.appMode === "Configure",
        hint: "Switch back to Configure mode to continue the tour.",
    },
}

export function advanceConditionMet(step: TourStep, snapshot: TourSnapshot): boolean {
    if (!step.advanceOn) return true
    return CONDITIONS[step.advanceOn.condition].holds(snapshot) === (step.advanceOn.state ?? true)
}

export interface TourRuntime {
    step: number
    reported?: TourCondition
}

export interface TourResult {
    stepIndex: number
    runtime: TourRuntime
    toast?: string
}

/**
 * returning the nearest preceding step that created this condition
 *
 * This is used specifically when rewinding steps after a condition was not met.
 */
function producerOf(condition: TourCondition, before: number): number | undefined {
    for (let i = before - 1; i >= 0; i--) {
        const advanceOn = TOUR_STEPS[i].advanceOn
        if (advanceOn?.condition === condition && (advanceOn.state ?? true)) return i
    }
    return undefined
}

/**
 * given the current step and the progress of the simulator, returns what step the user should be on.
 *
 * Used for rewinding in the event of a user being outside the tour and advancing when step condition met
 */
export function reconcile(stepIndex: number, snapshot: TourSnapshot, previous: TourRuntime): TourResult {
    const step = TOUR_STEPS[stepIndex]

    const runtime: TourRuntime = previous.step !== stepIndex ? { step: stepIndex } : { ...previous }

    if (step.advanceOn && advanceConditionMet(step, snapshot)) {
        return { stepIndex: stepIndex + 1, runtime: { step: stepIndex + 1 } }
    }

    if (snapshot.spawnPending) return { stepIndex, runtime }

    const unmet = step.requires?.find(condition => !CONDITIONS[condition].holds(snapshot))
    if (!unmet) return { stepIndex, runtime: { ...runtime, reported: undefined } }
    if (unmet === runtime.reported) return { stepIndex, runtime }

    const target = producerOf(unmet, stepIndex) ?? stepIndex
    return {
        stepIndex: target,
        runtime: { step: target, reported: unmet },
        toast: runtime.reported === undefined ? CONDITIONS[unmet].hint : undefined,
    }
}
