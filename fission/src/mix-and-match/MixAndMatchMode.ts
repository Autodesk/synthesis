import * as THREE from "three"
import EventSystem from "@/systems/EventSystem"
import { PAUSE_REF_MIX_AND_MATCH } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import { convertThreeMatrix4ToArray } from "@/util/TypeConversions"
import MixAndMatchBuild from "./MixAndMatchBuild"
import {
    componentWorldBounds,
    componentWorldTransform,
    mateFacesTransform,
    snapToFaceOffset,
} from "./MixAndMatchPlacement"
import MixAndMatchScene from "./MixAndMatchScene"
import type { ComponentId, LibraryPartRef, MixAndMatchSession } from "./MixAndMatchTypes"

/**
 * Lifecycle and user-facing operations for mix-and-match build mode.
 *
 * Physics is paused and collision disabled for the entire session rather than per part: the user is
 * arranging geometry, not simulating it, and parts are expected to overlap while being positioned.
 */
class MixAndMatchMode {
    private static _build: MixAndMatchBuild | undefined
    private static _scene: MixAndMatchScene | undefined

    public static get build(): MixAndMatchBuild | undefined {
        return this._build
    }

    public static get scene(): MixAndMatchScene | undefined {
        return this._scene
    }

    public static get isActive(): boolean {
        return this._build != null
    }

    /**
     * Enters build mode.
     *
     * @param   session Existing session to resume, e.g. one read off a saved mira. Omit for a new build.
     * @returns The active build. Re-entering while already active returns the current one untouched.
     */
    public static async enter(session?: MixAndMatchSession): Promise<MixAndMatchBuild> {
        if (this._build) return this._build

        World.physicsSystem.holdPause(PAUSE_REF_MIX_AND_MATCH)
        this._build = new MixAndMatchBuild(session)
        this._scene = new MixAndMatchScene()

        // Resuming replays the whole timeline rather than reconstructing an equivalent arrangement.
        await this._scene.applyState(this._build.state)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")

        return this._build
    }

    /**
     * Leaves build mode.
     *
     * @param keepComponents Hand the assembled parts off to normal simulation instead of removing
     *                       them. Set when finishing a build, cleared when abandoning one.
     */
    public static exit(keepComponents: boolean = false) {
        if (!this._build) return

        this._scene?.dispose(keepComponents)
        this._scene = undefined
        this._build = undefined
        World.physicsSystem.releasePause(PAUSE_REF_MIX_AND_MATCH)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")
    }

    /** Adds a library part to the build and stages it clear of what's already placed. */
    public static async spawnPart(libraryPartRef: LibraryPartRef): Promise<ComponentId | undefined> {
        const [build, scene] = this.require()
        if (!build || !scene) return undefined

        // Staged before it gets a component id, so the spawn entry records where it actually landed.
        const staged = await scene.stage(libraryPartRef)
        if (!staged) return undefined

        const componentId = build.spawn(libraryPartRef, staged.transform)
        scene.bind(componentId, staged.component)
        await this.sync()

        return componentId
    }

    /** Records where the user left a component after dragging its gizmo. */
    public static async commitPlacement(componentId: ComponentId) {
        const [build, scene] = this.require()
        const component = scene?.get(componentId)
        if (!build || !component) return

        build.move(componentId, [...convertThreeMatrix4ToArray(componentWorldTransform(component))])
        await this.sync()
    }

    /**
     * Slides `componentId` flush against `targetId`. A one-shot alignment nudge: it forms no
     * relationship between the two parts, and welding stays a separate explicit action.
     */
    public static async snapToFace(componentId: ComponentId, targetId: ComponentId) {
        const [build, scene] = this.require()
        const component = scene?.get(componentId)
        const target = scene?.get(targetId)
        if (!build || !scene || !component || !target || componentId === targetId) return

        const offset = snapToFaceOffset(componentWorldBounds(component), componentWorldBounds(target))

        scene.moveTree(componentId, new THREE.Matrix4().makeTranslation(offset.x, offset.y, offset.z))

        await this.commitPlacement(componentId)
    }

    /**
     * Rotates and slides `componentId` so a face picked on it lands flush against a face picked on
     * `targetId`, facing it. A one-shot alignment nudge: it forms no relationship between the two
     * parts, and welding stays a separate explicit action.
     *
     * @param movingPoint  World-space point clicked on `componentId`.
     * @param movingNormal World-space surface normal at `movingPoint`.
     * @param targetPoint  World-space point clicked on `targetId`.
     * @param targetNormal World-space surface normal at `targetPoint`.
     */
    public static async mateFaces(
        componentId: ComponentId,
        targetId: ComponentId,
        movingPoint: THREE.Vector3,
        movingNormal: THREE.Vector3,
        targetPoint: THREE.Vector3,
        targetNormal: THREE.Vector3
    ) {
        const [build, scene] = this.require()
        const component = scene?.get(componentId)
        const target = scene?.get(targetId)
        if (!build || !scene || !component || !target || componentId === targetId) return

        const delta = mateFacesTransform(movingPoint, movingNormal, targetPoint, targetNormal)
        scene.moveTree(componentId, delta)

        await this.commitPlacement(componentId)
    }

    public static async deleteComponent(componentId: ComponentId) {
        const [build] = this.require()
        if (!build) return

        build.delete(componentId)
        await this.sync()
    }

    /** Moves the timeline playhead. Scrubbing is a preview; it never edits the timeline. */
    public static async scrubTo(marker: number) {
        const [build] = this.require()
        if (!build) return

        build.scrubTo(marker)
        await this.sync()
    }

    private static require(): [MixAndMatchBuild | undefined, MixAndMatchScene | undefined] {
        if (!this._build || !this._scene) console.warn("Mix and match operation attempted outside of build mode")

        return [this._build, this._scene]
    }

    private static async sync() {
        if (!this._build || !this._scene) return

        await this._scene.applyState(this._build.state)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")
    }
}

export default MixAndMatchMode
