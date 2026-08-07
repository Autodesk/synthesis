import EventSystem from "@/systems/EventSystem"
import { replayTimeline, type TimelineState } from "./MixAndMatchTimeline"
import {
    type ComponentId,
    createEmptySession,
    type LibraryPartRef,
    type MixAndMatchSession,
    type TimelineEntry,
    type TransformArray,
} from "./MixAndMatchTypes"

const COMPONENT_ID_PREFIX = "c"

function nextComponentId(session: MixAndMatchSession): ComponentId {
    const highest = session.components.reduce((max, component) => {
        const parsed = Number.parseInt(component.id.slice(COMPONENT_ID_PREFIX.length), 10)
        return component.id.startsWith(COMPONENT_ID_PREFIX) && Number.isFinite(parsed) ? Math.max(max, parsed) : max
    }, 0)

    return `${COMPONENT_ID_PREFIX}${highest + 1}`
}

/**
 * An in-progress build: the session document, the timeline playhead, and the state the timeline
 * currently describes. Every user action goes through here as a timeline entry.
 */
class MixAndMatchBuild {
    private _session: MixAndMatchSession
    private _marker: number
    private _state: TimelineState

    public get session(): Readonly<MixAndMatchSession> {
        return this._session
    }

    public get timeline(): readonly TimelineEntry[] {
        return this._session.timeline
    }

    public get marker(): number {
        return this._marker
    }

    public get state(): TimelineState {
        return this._state
    }

    public get isScrubbed(): boolean {
        return this._marker < this._session.timeline.length
    }

    public get discardedByNextEdit(): number {
        return this._session.timeline.length - this._marker
    }

    public constructor(session: MixAndMatchSession = createEmptySession()) {
        this._session = session
        this._marker = session.timeline.length
        this._state = replayTimeline(session.timeline, this._marker)
    }

    public scrubTo(marker: number) {
        const clamped = Math.max(0, Math.min(marker, this._session.timeline.length))
        if (clamped === this._marker) return

        this._marker = clamped
        this.refresh()
    }

    public spawn(libraryPartRef: LibraryPartRef, transform: TransformArray): ComponentId {
        const componentId = nextComponentId(this._session)
        this._session.components.push({ id: componentId, libraryPartRef })
        this.commit({ type: "spawn", componentId, libraryPartRef, transform: [...transform] })

        return componentId
    }

    public move(componentId: ComponentId, transform: TransformArray) {
        this.commit({ type: "move", componentId, transform: [...transform] })
    }

    /**
     * Welds `childId` onto `parentId`. Replaces the child's existing weld, if any: a component has
     * exactly one active external weld.
     *
     * @returns Whether the weld was recorded. Rejected when it would weld a component to itself or
     *          close the weld tree into a cycle.
     */
    public weld(parentId: ComponentId, childId: ComponentId, relativeOffset: TransformArray): boolean {
        if (parentId === childId) return false
        if (!this._state.components.has(parentId) || !this._state.components.has(childId)) return false

        this.commit({ type: "weld", componentA: parentId, componentB: childId, relativeOffset: [...relativeOffset] })
        if (this._state.components.get(childId)?.weld?.parentId === parentId) return true

        // Replay refused it, which only happens for a weld that would close the tree into a cycle.
        // Drop the entry again as it never took affect.
        this._session.timeline.pop()
        this._marker = this._session.timeline.length
        this.refresh()
        console.warn(`Rejected weld ${childId} -> ${parentId}: would create a cycle`)

        return false
    }

    public resize(componentId: ComponentId, sizeOption: string) {
        this.commit({ type: "resize", componentId, sizeOption })
    }

    public delete(componentId: ComponentId) {
        this.commit({ type: "delete", componentId })
    }

    /**
     * Appends an entry, discarding anything after the playhead first. Callers that can strand history
     * should check {@link discardedByNextEdit} and confirm with the user beforehand.
     */
    public commit(entry: TimelineEntry) {
        this._session.timeline.length = this._marker
        this._session.timeline.push(entry)
        this._marker = this._session.timeline.length
        this.refresh()
    }

    private refresh() {
        this._state = replayTimeline(this._session.timeline, this._marker)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")
    }
}

export default MixAndMatchBuild
