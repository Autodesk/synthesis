import type { AppMode } from "@/systems/AppMode"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import type { TourCondition, TourStep, TourTargetId } from "./tourSteps"
import { TOUR_STEPS } from "./tourSteps"

export interface TourSnapshot {
    modal?: TourTargetId
    panels: { id?: TourTargetId; configMode?: ConfigMode }[]
    appMode: AppMode
    hasField: boolean
    hasRobot: boolean
    spawnPending: boolean
}

const hasPanel = (snapshot: TourSnapshot, id: TourTargetId, configMode?: ConfigMode) =>
    snapshot.panels.some(p => p.id === id && (configMode === undefined || p.configMode === configMode))

export const CONDITIONS: Record<TourCondition, { holds: (snapshot: TourSnapshot) => boolean; hint: string }> = {
    libraryOpen: {
        holds: s => s.modal === "LibraryModal",
        hint: "Open the assets library with the Add Assembly button to continue.",
    },
    field: {
        holds: s => s.hasField,
        hint: "Spawn a field from the library to continue.",
    },
    robot: {
        holds: s => s.hasRobot,
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

const MANUAL_HINT = "Use the tour card to continue, or press Skip to exit the tour."

export function stepHint(step: TourStep): string {
    return step.hint ?? (step.advanceOn ? CONDITIONS[step.advanceOn.condition].hint : MANUAL_HINT)
}

export interface TourRuntime {
    step: number
    armed: boolean
    reported?: TourCondition
}

export interface TourResult {
    stepIndex: number
    runtime: TourRuntime
    toast?: string
}

function producerOf(condition: TourCondition, before: number): number | undefined {
    for (let i = before - 1; i >= 0; i--) {
        const advanceOn = TOUR_STEPS[i].advanceOn
        if (advanceOn?.condition === condition && (advanceOn.state ?? true)) return i
    }
    return undefined
}

export function reconcile(stepIndex: number, snapshot: TourSnapshot, previous: TourRuntime): TourResult {
    const step = TOUR_STEPS[stepIndex]
    const entering = previous.step !== stepIndex
    const runtime: TourRuntime = entering ? { step: stepIndex, armed: false } : { ...previous }

    if (step.advanceOn) {
        const target = step.advanceOn.state ?? true
        if (CONDITIONS[step.advanceOn.condition].holds(snapshot) !== target) {
            runtime.armed = true
        } else if (runtime.armed) {
            const next = stepIndex + 1
            return { stepIndex: next, runtime: { step: next, armed: false } }
        }
    }

    if (snapshot.spawnPending) return { stepIndex, runtime }

    const unmet = step.requires?.find(condition => !CONDITIONS[condition].holds(snapshot))
    if (!unmet) return { stepIndex, runtime: { ...runtime, reported: undefined } }
    if (unmet === runtime.reported) return { stepIndex, runtime }

    const target = producerOf(unmet, stepIndex) ?? stepIndex
    return {
        stepIndex: target,
        runtime: { step: target, armed: false, reported: unmet },
        toast: runtime.reported === undefined ? CONDITIONS[unmet].hint : undefined,
    }
}
