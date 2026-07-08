import type { PopperPlacementType } from "@mui/material"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"

/**
 * Stable identifiers for the DOM elements a tour step can point at. Components opt
 * in to being an anchor with {@link useTourAnchor}, registering their element under
 * one of these ids in the {@link TourProvider} registry.
 */
export type TourAnchorId =
    | "add-assembly"
    | "mode-dropdown"
    | "spawn-panel"
    | "configure-assembly-select"
    | "configure-intake-button"
    | "configure-panel"
    | "intake-show-zone"

/**
 * Describes a real user action the tour watches for so it can advance automatically.
 * Manual `<` / `>` navigation always works regardless of this; `advanceOn` only adds
 * automatic forward motion when the described action happens.
 *
 * - `panel-open`: a panel with the given component name opens (rising edge only, so an
 *   already-open panel does not immediately advance). `configMode` further narrows it to
 *   a ConfigurePanel opened in a specific mode.
 * - `spawn`: an asset of the given {@link MiraType} is spawned.
 * - `event`: a one-shot `EventSystem` event fires.
 */
export type AdvanceTrigger =
    | { kind: "panel-open"; panelName: string; configMode?: ConfigMode }
    | { kind: "spawn"; miraType: MiraType }
    | { kind: "event"; event: "ConfigurationSavedEvent" }

export interface TourStep {
    title: string
    body: string
    /** Anchor to point at. When omitted the card is centered on screen (e.g. the "drive it" step). */
    anchorId?: TourAnchorId
    /** Where the card sits relative to its anchor. Ignored for centered steps. */
    placement: PopperPlacementType
    /** Optional automatic advance trigger (hybrid model). */
    advanceOn?: AdvanceTrigger
}

/**
 * The onboarding tour, in order. Mirrors the 12-step Figma flow: spawn a field,
 * spawn a robot, configure the robot's intake, then drive. Ambiguous "reading" steps
 * are manual-only; the clear milestones auto-advance.
 */
export const TOUR_STEPS: TourStep[] = [
    {
        title: "Add a Field",
        body: "First we need a field. Open the assets library with the Add Assembly button.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        advanceOn: { kind: "panel-open", panelName: "ImportMirabufPanel" },
    },
    {
        title: "Open the Library",
        body: "The library lets you search fields and robots by year. Switch to the FIELDS tab and select the 2026 field.",
        anchorId: "spawn-panel",
        placement: "left",
        advanceOn: { kind: "spawn", miraType: MiraType.FIELD },
    },
    {
        title: "Add a Robot",
        body: "Now open the Add Assembly library again to spawn a robot.",
        anchorId: "add-assembly",
        placement: "bottom-start",
    },
    {
        title: "Choose a Robot",
        body: "With the library open, go to the 2026 section and select a robot.",
        anchorId: "spawn-panel",
        placement: "left",
        advanceOn: { kind: "spawn", miraType: MiraType.ROBOT },
    },
    {
        title: "Select an Assembly",
        body: "Pick which assembly you want to configure from this drop-down menu.",
        anchorId: "configure-assembly-select",
        placement: "bottom-start",
    },
    {
        title: "Choose Your Robot",
        body: "Choose the robot you just spawned. You can configure fields too, but that is not what we are after right now.",
        anchorId: "configure-assembly-select",
        placement: "bottom-start",
    },
    {
        title: "Pick What to Configure",
        body: "With your robot selected, choose what to configure. In this case, the intake.",
        anchorId: "configure-intake-button",
        placement: "bottom",
        advanceOn: { kind: "panel-open", panelName: "ConfigurePanel", configMode: ConfigMode.INTAKE },
    },
    {
        title: "Adjust the Intake",
        body: "The configure window is open. Move the intake so that it aligns with the robot's intake mechanism.",
        anchorId: "configure-panel",
        placement: "left",
    },
    {
        title: "Show the Intake Zone",
        body: "Enable 'Show intake zone indicator always'. This makes the intake zone much easier to see.",
        anchorId: "intake-show-zone",
        placement: "left",
    },
    {
        title: "Finish Up",
        body: "Once the intake is configured, close the configure menu by pressing Save.",
        anchorId: "configure-panel",
        placement: "left",
        advanceOn: { kind: "event", event: "ConfigurationSavedEvent" },
    },
    {
        title: "Drive Your Robot",
        body: "Now drive using WASD, and E for the intake. When a game piece enters the sphere, your newly configured intake picks it up.",
        placement: "top",
    },
    {
        title: "Switch Modes",
        body: "This drop-down switches between modes and changes the buttons available in the bar.",
        anchorId: "mode-dropdown",
        placement: "bottom-start",
    },
]

export const TOUR_STEP_COUNT = TOUR_STEPS.length
