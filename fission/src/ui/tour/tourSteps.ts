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
    | "assembly-setup"
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
 * - `modal-open`: the modal with the given component name opens (rising edge only).
 * - `spawn`: an asset of the given {@link MiraType} is spawned.
 * - `event`: a one-shot `EventSystem` event fires.
 */
export type AdvanceTrigger =
    | { kind: "panel-open"; panelName: string; configMode?: ConfigMode }
    | { kind: "modal-open"; modalName: string }
    | { kind: "spawn"; miraType: MiraType }
    | { kind: "event"; event: "ConfigurationSavedEvent" }

/** Where an anchorless card is pinned on screen. Defaults to `"center"`. */
export type ScreenPosition = "center" | "top-left"

export interface TourStep {
    title: string
    body: string
    /** Anchor to point at. When omitted the card is centered on screen (e.g. the "drive it" step). */
    anchorId?: TourAnchorId
    /** Where the card sits relative to its anchor. Ignored for centered steps. */
    placement: PopperPlacementType
    /** For anchorless steps only: where the card is pinned on screen (default `"center"`). */
    screenPosition?: ScreenPosition
    /** Optional automatic advance trigger (hybrid model). */
    advanceOn?: AdvanceTrigger
    /**
     * Informational ("read this") step: dims the whole screen and blocks every click except this
     * card's own next/prev/skip. Use for steps that only explain UI the user should not act on yet
     * (e.g. the auto-selected assembly, or the intake panel before it is time to Save). The card
     * still anchors and points normally; it just floats above a blocking scrim. Actional ("do this")
     * steps omit this so the user can interact with the app underneath.
     */
    informational?: boolean
}

/**
 * The onboarding tour, in order: spawn a field, spawn a robot, configure the robot's intake, then
 * drive. Steps are either *actional* - the user performs the real interaction and (where an
 * {@link AdvanceTrigger} is set) the tour auto-advances - or *informational* ({@link TourStep.informational}),
 * which grey out and lock the app so the user can only read and click through.
 */
export const TOUR_STEPS: TourStep[] = [
    {
        title: "Add a Field",
        body: "First we need a field. Open the assets library with the Add Assembly button.",
        anchorId: "add-assembly",
        placement: "bottom-start",
        advanceOn: { kind: "modal-open", modalName: "LibraryModal" },
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
        advanceOn: { kind: "modal-open", modalName: "LibraryModal" },
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
        advanceOn: { kind: "panel-open", panelName: "ConfigurePanel", configMode: ConfigMode.INTAKE },
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
