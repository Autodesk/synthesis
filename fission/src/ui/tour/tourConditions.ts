import type { AppMode } from "@/systems/AppMode"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import type { TourCondition, TourStep, TourTargetId } from "./tourSteps"
import { TOUR_STEPS } from "./tourSteps"

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
    count?: (snapshot: TourSnapshot) => number
    hint: string
}

export const CONDITIONS: Record<TourCondition, TourConditionDef> = {
    libraryOpen: {
        holds: s => s.modal === "LibraryModal",
        hint: "Open the assets library with the Add Assembly button to continue.",
    },
    field: {
        holds: s => s.fieldCount > 0,
        count: s => s.fieldCount,
        hint: "Spawn a field from the library to continue.",
    },
    robot: {
        holds: s => s.robotCount > 0,
        count: s => s.robotCount,
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

export function advanceConditionMet(step: TourStep, snapshot: TourSnapshot): boolean {
    if (!step.advanceOn) return true
    return CONDITIONS[step.advanceOn.condition].holds(snapshot) === (step.advanceOn.state ?? true)
}

export interface TourRuntime {
    step: number
    armed: boolean
    reported?: TourCondition
    baseline?: number
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

function countFor(stepIndex: number, snapshot: TourSnapshot): number | undefined {
    const condition = TOUR_STEPS[stepIndex]?.advanceOn?.condition
    return condition ? CONDITIONS[condition].count?.(snapshot) : undefined
}

export function reconcile(stepIndex: number, snapshot: TourSnapshot, previous: TourRuntime): TourResult {
    const step = TOUR_STEPS[stepIndex]

    const runtimeAt = (target: number, reported?: TourCondition): TourRuntime => ({
        step: target,
        armed: false,
        reported,
        baseline: countFor(target, snapshot),
    })

    const runtime: TourRuntime = previous.step !== stepIndex ? runtimeAt(stepIndex) : { ...previous }

    if (step.advanceOn) {
        const holds = advanceConditionMet(step, snapshot)
        const count = countFor(stepIndex, snapshot)
        const grown = count !== undefined && runtime.baseline !== undefined && count > runtime.baseline

        if (grown || (holds && runtime.armed)) return { stepIndex: stepIndex + 1, runtime: runtimeAt(stepIndex + 1) }
        if (!holds) runtime.armed = true
    }

    if (snapshot.spawnPending) return { stepIndex, runtime }

    const unmet = step.requires?.find(condition => !CONDITIONS[condition].holds(snapshot))
    if (!unmet) return { stepIndex, runtime: { ...runtime, reported: undefined } }
    if (unmet === runtime.reported) return { stepIndex, runtime }

    const target = producerOf(unmet, stepIndex) ?? stepIndex
    return {
        stepIndex: target,
        runtime: runtimeAt(target, unmet),
        toast: runtime.reported === undefined ? CONDITIONS[unmet].hint : undefined,
    }
}
