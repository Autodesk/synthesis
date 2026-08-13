import type { PopperPlacementType } from "@mui/material"
import type { FunctionComponent } from "react"

export type TourTargetId = "LibraryModal" | "ConfigurePanel" | "InitialConfigPanel"

interface TourTagged {
    tourId?: TourTargetId
}

export function tourTarget<P>(component: FunctionComponent<P>, id: TourTargetId): FunctionComponent<P> {
    ;(component as FunctionComponent<P> & TourTagged).tourId = id
    return component
}

export function tourIdOf(component: FunctionComponent | undefined): TourTargetId | undefined {
    return (component as (FunctionComponent & TourTagged) | undefined)?.tourId
}

export type TourAnchorId =
    | "add-assembly"
    | "mode-dropdown"
    | "spawn-panel"
    | "assembly-setup"
    | "configure-assembly-select"
    | "configure-intake-button"
    | "configure-panel"
    | "intake-show-zone"

export type TourCondition = "libraryOpen" | "field" | "robot" | "setupPanel" | "intakePanel" | "configureMode"

export type TourStepId =
    | "add-field"
    | "spawn-field"
    | "add-robot"
    | "spawn-robot"
    | "setup-assembly"
    | "select-assembly"
    | "pick-config"
    | "adjust-intake"
    | "show-intake-zone"
    | "save-config"
    | "drive"
    | "switch-modes"

// for anchorless cards
export type ScreenPosition = "center" | "top-left"

export type TourFocus = "anchor" | "screen"

export interface TourStep {
    id: TourStepId
    title: string
    body: string
    anchorId?: TourAnchorId
    placement: PopperPlacementType
    screenPosition?: ScreenPosition
    advanceOn?: { condition: TourCondition; state?: boolean }
    requires?: TourCondition[]
    focus?: TourFocus
}

export const TOUR_STEPS: TourStep[] = [
    {
        id: "add-field",
        title: "Add a Field",
        body: "First we need a field. Open the assets library with the Add Assembly button.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        focus: "anchor",
        advanceOn: { condition: "libraryOpen" },
    },
    {
        id: "spawn-field",
        title: "Open the Library",
        body: "The library groups fields and robots by year. Pick the newest year tab and spawn the field.",
        anchorId: "spawn-panel",
        placement: "left",
        focus: "anchor",
        advanceOn: { condition: "field" },
        requires: ["libraryOpen"],
    },
    {
        id: "add-robot",
        title: "Add a Robot",
        body: "Now open the Add Assembly library again to spawn a robot.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        focus: "anchor",
        advanceOn: { condition: "libraryOpen" },
        requires: ["field"],
    },
    {
        id: "spawn-robot",
        title: "Choose a Robot",
        body: "With the library open, pick a robot from the newest year tab.",
        anchorId: "spawn-panel",
        placement: "left",
        focus: "anchor",
        advanceOn: { condition: "robot" },
        requires: ["libraryOpen", "field"],
    },
    {
        id: "setup-assembly",
        title: "Set Up Your Assembly",
        body: "Select an input scheme for your robot, or just press Finish and a compatible scheme is assigned automatically. You can change the input scheme, alliance, and station later.",
        anchorId: "assembly-setup",
        placement: "left",
        advanceOn: { condition: "setupPanel", state: false },
    },
    {
        id: "select-assembly",
        title: "Select an Assembly",
        body: "Your spawned robot is automatically selected here for configuration. You could switch to another assembly from this drop-down, but we will stick with your robot.",
        anchorId: "configure-assembly-select",
        placement: "bottom-start",
        focus: "screen",
        requires: ["robot", "configureMode"],
    },
    {
        id: "pick-config",
        title: "Pick What to Configure",
        body: "With your robot selected, choose what to configure. In this case, the intake.",
        anchorId: "configure-intake-button",
        placement: "bottom",
        focus: "anchor",
        advanceOn: { condition: "intakePanel" },
        requires: ["robot", "configureMode"],
    },
    {
        id: "adjust-intake",
        title: "Adjust the Intake",
        body: "This is the Configure Assets panel. Here you can align the intake with the robot's intake mechanism and tune how it picks up game pieces.",
        anchorId: "configure-panel",
        placement: "left",
        focus: "screen",
        requires: ["intakePanel"],
    },
    {
        id: "show-intake-zone",
        title: "Show the Intake Zone",
        body: "The 'Show intake zone indicator always' toggle keeps the intake's pickup zone visible, making it much easier to see.",
        anchorId: "intake-show-zone",
        placement: "left",
        focus: "screen",
        requires: ["intakePanel"],
    },
    {
        id: "save-config",
        title: "Finish Up",
        body: "When you are happy with the intake, press Save to apply your configuration and close the panel.",
        anchorId: "configure-panel",
        placement: "left",
        advanceOn: { condition: "intakePanel", state: false },
    },
    {
        id: "drive",
        title: "Drive Your Robot",
        body: "Now drive your robot with the input scheme you picked, and trigger its intake. When a game piece enters the sphere, your newly configured intake picks it up. Press Next when you are ready to move on.",
        placement: "top",
        screenPosition: "top-left",
        requires: ["robot"],
    },
    {
        id: "switch-modes",
        title: "Switch Modes",
        body: "This drop-down switches between modes and changes the buttons available in the bar. Press Done to finish the tour.",
        anchorId: "mode-dropdown",
        placement: "bottom-start",
    },
]
