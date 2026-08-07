import type * as THREE from "three"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import EventSystem from "@/systems/EventSystem"
import { PAUSE_REF_MIX_AND_MATCH } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import { convertThreeMatrix4ToArray } from "@/util/TypeConversions"
import { downloadBlob } from "@/util/Utility"
import { mergeAssemblies } from "./MixAndMatchAssemblyMerge"
import MixAndMatchBuild from "./MixAndMatchBuild"
import { writeSessionToAssembly } from "./MixAndMatchDocument"
import { componentWorldTransform, mateFacesTransform, relativeOffsetBetween } from "./MixAndMatchPlacement"
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
    private static _selected: ComponentId | undefined

    public static get build(): MixAndMatchBuild | undefined {
        return this._build
    }

    public static get scene(): MixAndMatchScene | undefined {
        return this._scene
    }

    public static get isActive(): boolean {
        return this._build != null
    }

    /** The component currently selected in the assembly tree, shared with panels like Snap to Face. */
    public static get selected(): ComponentId | undefined {
        return this._selected
    }

    public static setSelected(componentId: ComponentId | undefined) {
        this._selected = componentId
        EventSystem.dispatch("MixAndMatchStateChangedEvent")
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

    /** Leaves build mode, discarding the build-time scene objects. */
    public static exit() {
        if (!this._build) return

        this._scene?.dispose()
        this._scene = undefined
        this._build = undefined
        this._selected = undefined
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
     * Rotates and slides `componentId` so a face picked on it lands flush against a face picked on
     * `targetId`, facing it. A one-shot alignment nudge: it forms no relationship between the two
     * parts, and welding stays a separate explicit action.
     *
     * Only moves the live scene object; nothing is written to the timeline until `commitPlacement` is
     * called, so the caller can preview the snap and back out with `discardPreview` if unwanted.
     *
     * @param movingPoint  World-space point clicked on `componentId`.
     * @param movingNormal World-space surface normal at `movingPoint`.
     * @param targetPoint  World-space point clicked on `targetId`.
     * @param targetNormal World-space surface normal at `targetPoint`.
     */
    public static previewMateFaces(
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
    }

    /** Snaps the scene back to the last committed timeline state, undoing any uncommitted preview move. */
    public static async discardPreview() {
        await this.sync()
    }

    /**
     * Welds `childId` onto `parentId`.
     *
     * The recorded offset is between the two components' root bodies. Everything welded here is rigid.
     * Joints authored inside a library part are untouched.
     *
     * @returns Whether the weld was recorded.
     */
    public static async weld(parentId: ComponentId, childId: ComponentId): Promise<boolean> {
        const [build, scene] = this.require()
        const parent = scene?.get(parentId)
        const child = scene?.get(childId)
        if (!build || !parent || !child) return false

        const relativeOffset = relativeOffsetBetween(componentWorldTransform(parent), componentWorldTransform(child))
        const welded = build.weld(parentId, childId, [...convertThreeMatrix4ToArray(relativeOffset)])
        await this.sync()

        return welded
    }

    public static async deleteComponent(componentId: ComponentId) {
        const [build] = this.require()
        if (!build) return

        build.delete(componentId)
        await this.sync()
    }

    /**
     * Merges every placed component into one real `mirabuf.Assembly` per weld tree - the shape the
     * simulator loads like any other robot - and hands those off to normal simulation in place of
     * the build-time scene.
     *
     * @returns Whether the build was finished. Refused while scrubbed or while nothing is placed.
     */
    public static async finish(): Promise<boolean> {
        const [build, scene] = this.require()
        if (!build || !scene) return false

        if (build.isScrubbed) {
            globalAddToast("warning", "Rolled Back", "Resume from the playhead before finishing.")
            return false
        }
        if (build.state.components.size === 0) {
            globalAddToast("warning", "Nothing to Finish", "Add at least one part first.")
            return false
        }

        // Merged and stamped before the scene is torn down so a re-opened robot can replay this
        // exact build.
        const merged = mergeAssemblies(build.state, scene.assembliesByComponent())
        merged.forEach(assembly => writeSessionToAssembly(assembly, build.session))

        this.exit()

        for (const assembly of merged) {
            const sceneObject = await createMirabuf(assembly.info?.GUID ?? "mix-and-match-build", assembly)
            if (sceneObject) World.sceneRenderer.registerSceneObject(sceneObject)
        }
        globalAddToast("info", "Build Finished", "Build finished")

        return true
    }

    /**
     * Saves the build's merged assembly as an ordinary tagged `.mira` file, ready to be re-opened
     * for editing. Every other consumer sees a normal robot file. Unlike `finish`, the build-time
     * scene is left alone - this is a snapshot, not an exit.
     *
     * Refuses if any component is stranded (not welded into the rest of the build): an export must
     * resolve into exactly one robot, not several loose ones.
     */
    public static async exportBuild(): Promise<boolean> {
        const [build, scene] = this.require()
        if (!build || !scene) return false

        if (build.state.components.size === 0) {
            globalAddToast("warning", "Nothing to Export", "Add at least one part first.")
            return false
        }

        const merged = mergeAssemblies(build.state, scene.assembliesByComponent())
        if (merged.length !== 1) {
            globalAddToast(
                "error",
                "Export Failed",
                "Weld every part together into one connected build before exporting."
            )
            return false
        }

        const [assembly] = merged
        writeSessionToAssembly(assembly, build.session)

        const name = assembly.info?.name ?? "Mix and Match Robot"
        const encoded = mirabuf.Assembly.encode(assembly).finish()
        downloadBlob(`${name}.mira`, encoded.buffer as ArrayBuffer)

        // Cached as well so the saved build shows up in the part library, ready to be re-opened.
        await MirabufCachingService.storeAssemblyInCache(assembly, { miraType: MiraType.ROBOT, name })
        globalAddToast("info", "Exported", `Saved ${name}.mira`)

        return true
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
