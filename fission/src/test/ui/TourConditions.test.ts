import { describe, expect, test } from "vitest"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import {
    advanceConditionMet,
    reconcile,
    type TourResult,
    type TourRuntime,
    type TourSnapshot,
} from "@/ui/tour/TourConditions"
import { TOUR_STEPS } from "@/ui/tour/TourSteps"

const stepOf = (title: string) => TOUR_STEPS.findIndex(step => step.title === title)

const snapshot = (overrides: Partial<TourSnapshot> = {}): TourSnapshot => ({
    panels: [],
    appMode: "Configure",
    fieldCount: 0,
    robotCount: 0,
    spawnPending: false,
    ...overrides,
})

const fresh: TourRuntime = { step: -1, armed: false }

function run(stepIndex: number, snapshots: TourSnapshot[], runtime: TourRuntime = fresh): TourResult {
    let result: TourResult = { stepIndex, runtime }
    for (const s of snapshots) {
        result = reconcile(result.stepIndex, s, result.runtime)
    }
    return result
}

function settle(stepIndex: number, s: TourSnapshot, runtime: TourRuntime = fresh): TourResult {
    let result: TourResult = { stepIndex, runtime }
    for (let i = 0; i < TOUR_STEPS.length + 1; i++) {
        const pass = reconcile(result.stepIndex, s, result.runtime)
        if (pass.stepIndex === result.stepIndex && i > 0) return pass
        result = pass
    }
    throw new Error("reconcile never settled")
}

describe("tour reconciler", () => {
    test("does not advance on the tick a step is entered", () => {
        const result = run(stepOf("Add a Field"), [snapshot({ modal: "LibraryModal" })])
        expect(result.stepIndex).toBe(stepOf("Add a Field"))
    })

    test("advances once the condition rises", () => {
        const result = run(stepOf("Add a Field"), [snapshot(), snapshot({ modal: "LibraryModal" })])
        expect(result.stepIndex).toBe(stepOf("Open the Library"))
    })

    test("advances when the step's screen closes", () => {
        const open = snapshot({ fieldCount: 1, robotCount: 1, panels: [{ id: "InitialConfigPanel" }] })
        const result = run(stepOf("Set Up Your Assembly"), [open, { ...open, panels: [] }])
        expect(result.stepIndex).toBe(stepOf("Select an Assembly"))
    })

    test("rewinds to the step that reopens a screen the user closed", () => {
        const result = run(stepOf("Choose a Robot"), [snapshot({ fieldCount: 1 })])
        expect(result.stepIndex).toBe(stepOf("Add a Robot"))
        expect(result.toast).toBeDefined()
    })

    test("cascades back to the first step when everything is gone", () => {
        expect(settle(stepOf("Choose a Robot"), snapshot()).stepIndex).toBe(stepOf("Add a Field"))
    })

    test("explains a cascade once, with the reason the tour moved", () => {
        const empty = snapshot()
        const toasts: string[] = []
        let result: TourResult = { stepIndex: stepOf("Select an Assembly"), runtime: fresh }
        for (let i = 0; i < TOUR_STEPS.length; i++) {
            result = reconcile(result.stepIndex, empty, result.runtime)
            if (result.toast) toasts.push(result.toast)
        }
        expect(toasts).toEqual(["Spawn a robot from the library to continue."])
    })

    test("holds the step, once, when nothing in the tour re-establishes the condition", () => {
        const gameplay = snapshot({ robotCount: 1, fieldCount: 1, appMode: "Gameplay" })
        const first = run(stepOf("Select an Assembly"), [gameplay])
        expect(first.stepIndex).toBe(stepOf("Select an Assembly"))
        expect(first.toast).toBeDefined()

        const second = reconcile(first.stepIndex, gameplay, first.runtime)
        expect(second.toast).toBeUndefined()
    })

    test("leaves requirements alone while a spawn is in flight", () => {
        const result = run(stepOf("Choose a Robot"), [snapshot({ fieldCount: 1, spawnPending: true })])
        expect(result.stepIndex).toBe(stepOf("Choose a Robot"))
        expect(result.toast).toBeUndefined()
    })

    test("completes a spawn step even though the library closed first", () => {
        const result = run(stepOf("Choose a Robot"), [
            snapshot({ fieldCount: 1, modal: "LibraryModal" }),
            snapshot({ fieldCount: 1, spawnPending: true }),
            snapshot({ fieldCount: 1, robotCount: 1 }),
        ])
        expect(result.stepIndex).toBe(stepOf("Set Up Your Assembly"))
    })

    test("completes a spawn step when the user already had one of that asset", () => {
        const result = run(stepOf("Choose a Robot"), [
            snapshot({ fieldCount: 1, robotCount: 1, modal: "LibraryModal" }),
            snapshot({ fieldCount: 1, robotCount: 1, spawnPending: true }),
            snapshot({ fieldCount: 1, robotCount: 2 }),
        ])
        expect(result.stepIndex).toBe(stepOf("Set Up Your Assembly"))
    })

    test("offers Next on a step whose screen is already closed", () => {
        const closed = snapshot({ fieldCount: 1, robotCount: 1 })
        expect(advanceConditionMet(TOUR_STEPS[stepOf("Set Up Your Assembly")], closed)).toBe(true)
        expect(advanceConditionMet(TOUR_STEPS[stepOf("Finish Up")], closed)).toBe(true)
    })

    test("withholds Next while a step is still waiting on its condition", () => {
        const empty = snapshot()
        expect(advanceConditionMet(TOUR_STEPS[stepOf("Add a Field")], empty)).toBe(false)
        expect(advanceConditionMet(TOUR_STEPS[stepOf("Choose a Robot")], empty)).toBe(false)
    })

    test("recovers the intake panel by sending the user back to the step that opens it", () => {
        const configured = snapshot({ fieldCount: 1, robotCount: 1 })
        const result = run(stepOf("Adjust the Intake"), [
            { ...configured, panels: [{ id: "ConfigurePanel", configMode: ConfigMode.INTAKE }] },
            configured,
        ])
        expect(result.stepIndex).toBe(stepOf("Pick What to Configure"))
    })
})
