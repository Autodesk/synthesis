/**
 * Document schema for a mix-and-match build session.
 *
 * The timeline is the source of truth, not the derived weld graph. Re-entering build mode replays
 * every entry in order to restore the exact state as of the last "OK", rather than reconstructing an
 * equivalent-looking arrangement.
 */

/** Key under `Parts.user_data` that the serialized session lives at. Same field `urdfImport` uses. */
export const MIX_AND_MATCH_USER_DATA_KEY = "mixAndMatchSession"

export const MIX_AND_MATCH_SESSION_VERSION = 1

/** Identifies one placed component within a session. Unique per session, not globally. */
export type ComponentId = string

/** Points at the library part a component was spawned from. Hash key into `MirabufCachingService`. */
export type LibraryPartRef = string

/** Column-major 4x4, matching `convertThreeMatrix4ToArray` / `convertArrayToThreeMatrix4`. */
export type TransformArray = number[]

export interface MixAndMatchComponent {
    id: ComponentId
    libraryPartRef: LibraryPartRef
}

/** A part was added to the build. `transform` is the world transform of its root body. */
export interface SpawnTimelineEntry {
    type: "spawn"
    componentId: ComponentId
    libraryPartRef: LibraryPartRef
    transform: TransformArray
}

/** A part was repositioned. `transform` is absolute, not a delta, so replay never accumulates error. */
export interface MoveTimelineEntry {
    type: "move"
    componentId: ComponentId
    transform: TransformArray
}

/**
 * `componentB` was welded onto `componentA`; B is the child. A component has at most one active weld,
 * so a later weld naming the same B replaces this one.
 */
export interface WeldTimelineEntry {
    type: "weld"
    componentA: ComponentId
    componentB: ComponentId
    relativeOffset: TransformArray
}

export interface ResizeTimelineEntry {
    type: "resize"
    componentId: ComponentId
    sizeOption: string
}

export interface DeleteTimelineEntry {
    type: "delete"
    componentId: ComponentId
}

export type TimelineEntry =
    | SpawnTimelineEntry
    | MoveTimelineEntry
    | WeldTimelineEntry
    | ResizeTimelineEntry
    | DeleteTimelineEntry

export type TimelineEntryType = TimelineEntry["type"]

export interface MixAndMatchSession {
    version: typeof MIX_AND_MATCH_SESSION_VERSION
    components: MixAndMatchComponent[]
    timeline: TimelineEntry[]
}

export function createEmptySession(): MixAndMatchSession {
    return { version: MIX_AND_MATCH_SESSION_VERSION, components: [], timeline: [] }
}
