import type { PopperPlacementType } from "@mui/material"
import type { FunctionComponent } from "react"
import { MiraType } from "@/mirabuf/MiraType"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"

export type TourTargetId = "LibraryModal" | "ConfigurePanel"

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

export type AdvanceTrigger =
    | { kind: "panel-open"; target: TourTargetId; configMode?: ConfigMode }
    | { kind: "modal-open"; target: TourTargetId }
    | { kind: "spawn"; miraType: MiraType }
    | { kind: "event"; event: "ConfigurationSavedEvent" }

// for anchorless cards
export type ScreenPosition = "center" | "top-left"

export interface TourStep {
    title: string
    body: string
    anchorId?: TourAnchorId
    placement: PopperPlacementType
    screenPosition?: ScreenPosition
    advanceOn?: AdvanceTrigger
    informational?: boolean
}

export const TOUR_STEPS: TourStep[] = [
    {
        title: "Add a Field",
        body: "First we need a field. Open the assets library with the Add Assembly button.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        advanceOn: { kind: "modal-open", target: "LibraryModal" },
    },
    {
        title: "Open the Library",
        body: "The library groups fields and robots by year. Select the 2026 year tab and spawn the field.",
        anchorId: "spawn-panel",
        placement: "left",
        advanceOn: { kind: "spawn", miraType: MiraType.FIELD },
    },
    {
        title: "Add a Robot",
        body: "Now open the Add Assembly library again to spawn a robot.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        advanceOn: { kind: "modal-open", target: "LibraryModal" },
    },
    {
        title: "Choose a Robot",
        body: "With the library open, on the 2026 year tab, pick a robot.",
        anchorId: "spawn-panel",
        placement: "left",
        advanceOn: { kind: "spawn", miraType: MiraType.ROBOT },
    },
    {
        title: "Set Up Your Assembly",
        body: "Select an input scheme for your robot, or just press Finish and the Ernie (WASD) scheme is assigned automatically. You can change the input scheme, alliance, and station later.",
        anchorId: "assembly-setup",
        placement: "left",
        advanceOn: { kind: "event", event: "ConfigurationSavedEvent" },
    },
    {
        title: "Select an Assembly",
        body: "Your spawned robot is automatically selected here for configuration. You could switch to another assembly from this drop-down, but we will stick with your robot.",
        anchorId: "configure-assembly-select",
        placement: "bottom-start",
        informational: true,
    },
    {
        title: "Pick What to Configure",
        body: "With your robot selected, choose what to configure. In this case, the intake.",
        anchorId: "configure-intake-button",
        placement: "bottom",
        advanceOn: { kind: "panel-open", target: "ConfigurePanel", configMode: ConfigMode.INTAKE },
    },
    {
        title: "Adjust the Intake",
        body: "This is the Configure Assets panel. Here you can align the intake with the robot's intake mechanism and tune how it picks up game pieces.",
        anchorId: "configure-panel",
        placement: "left",
        informational: true,
    },
    {
        title: "Show the Intake Zone",
        body: "The 'Show intake zone indicator always' toggle keeps the intake's pickup zone visible, making it much easier to see.",
        anchorId: "intake-show-zone",
        placement: "left",
        informational: true,
    },
    {
        title: "Finish Up",
        body: "When you are happy with the intake, press Save to apply your configuration and close the panel.",
        anchorId: "configure-panel",
        placement: "left",
        advanceOn: { kind: "event", event: "ConfigurationSavedEvent" },
    },
    {
        title: "Drive Your Robot",
        body: "Now drive using WASD, and E for the intake. When a game piece enters the sphere, your newly configured intake picks it up.",
        placement: "top",
        screenPosition: "top-left",
    },
    {
        title: "Switch Modes",
        body: "This drop-down switches between modes and changes the buttons available in the bar.",
        anchorId: "mode-dropdown",
        placement: "bottom-start",
    },
]

export const TOUR_STEP_COUNT = TOUR_STEPS.length
