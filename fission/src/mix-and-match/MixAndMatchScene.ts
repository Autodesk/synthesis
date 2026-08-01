import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { createMirabuf, type RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import { LayerReserve } from "@/systems/physics/PhysicsSystem"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import { convertArrayToThreeMatrix4, convertThreeMatrix4ToArray } from "@/util/TypeConversions"
import { componentWorldTransform, moveComponentBy, setComponentWorldTransform } from "./MixAndMatchPlacement"
import { subtreeOf, type ComponentState, type TimelineState, weldPairs } from "./MixAndMatchTimeline"
import type { ComponentId, LibraryPartRef, TransformArray } from "./MixAndMatchTypes"
import { weldBodies } from "./MixAndMatchWeld"
import PartLibrary from "./PartLibrary"

/** Freshly spawned parts are staged on a grid so they don't land inside each other. */
const STAGING_STEP_METERS = 1.0
const STAGING_ROW_LENGTH = 4

/**
 * Keeps welded groups rigid while a part is being dragged.
 *
 * The transform gizmo only knows about the one assembly it's attached to, so without this a frame
 * would slide out from under its welded pods and only snap back together on release.
 */
class WeldFollower extends SceneObject {
    private _scene: MixAndMatchScene
    private _lastDragged: Map<ComponentId, THREE.Matrix4> = new Map()

    public constructor(scene: MixAndMatchScene) {
        super()
        this._scene = scene
    }

    public setup(): void {}

    public update(): void {
        this._scene.components.forEach((component, componentId) => {
            if (!World.sceneRenderer.gizmosOnMirabuf.get(component.id)?.isDragging) {
                this._lastDragged.delete(componentId)
                return
            }

            const current = componentWorldTransform(component)
            const previous = this._lastDragged.get(componentId)
            this._lastDragged.set(componentId, current)
            if (!previous) return

            const delta = current.clone().multiply(previous.invert())
            this._scene.descendantsOf(componentId).forEach(descendantId => {
                const descendant = this._scene.components.get(descendantId)
                if (descendant) moveComponentBy(descendant, delta)
            })
        })
    }

    public dispose(): void {
        this._lastDragged.clear()
    }
}

/**
 * Owns the live scene objects behind a build and keeps them matching the timeline state.
 *
 * Each placed part stays its own independent assembly, parser and mechanism, exactly like a robot and
 * a field already coexist today. Nothing here merges mira documents.
 */
class MixAndMatchScene {
    private _components: Map<ComponentId, MirabufSceneObject> = new Map()
    private _componentBySceneObject: Map<number, ComponentId> = new Map()
    /** Which library part each live component was actually built from, which a resize changes. */
    private _componentRef: Map<ComponentId, LibraryPartRef> = new Map()
    private _state: TimelineState = { components: new Map() }
    private _pending: Promise<void> = Promise.resolve()
    private _spawnCount = 0

    /**
     * One robot layer for the whole build. Every component shares it, so parts of the same robot
     * never collide with each other and a build isn't capped at the size of the robot layer pool.
     */
    private _layerReserve: LayerReserve = new LayerReserve()

    private _weldFollower: WeldFollower
    private _weldFollowerId: number

    public get components(): ReadonlyMap<ComponentId, MirabufSceneObject> {
        return this._components
    }

    public constructor() {
        this._weldFollower = new WeldFollower(this)
        this._weldFollowerId = World.sceneRenderer.registerSceneObject(this._weldFollower)
    }

    public get(componentId: ComponentId): MirabufSceneObject | undefined {
        return this._components.get(componentId)
    }

    /** Resolves a clicked body to the component that owns it, whichever rigid node was actually hit. */
    public componentIdOfBody(bodyId: Jolt.BodyID): ComponentId | undefined {
        const associate = World.physicsSystem.getBodyAssociation(bodyId) as RigidNodeAssociate | undefined
        if (!associate?.sceneObject) return undefined

        return this._componentBySceneObject.get(associate.sceneObject.id)
    }

    /** The root body of a component, which is the body every weld anchors to. */
    public rootBodyOf(componentId: ComponentId): Jolt.BodyID | undefined {
        return this._components.get(componentId)?.getRootNodeId()
    }

    public descendantsOf(componentId: ComponentId): ComponentId[] {
        return subtreeOf(this._state.components, componentId).filter(id => id !== componentId)
    }

    /**
     * Loads a library part into the scene and stages it clear of what's already there.
     *
     * The assembly is created before it has a component id so the spawn entry can record where it
     * actually landed instead of a placeholder that a follow-up move has to correct.
     *
     * @returns The scene object and the world transform it landed at, or undefined if the part
     *          couldn't be loaded.
     */
    public async stage(
        libraryPartRef: LibraryPartRef
    ): Promise<{ component: MirabufSceneObject; transform: TransformArray } | undefined> {
        const assembly = await PartLibrary.load(libraryPartRef)
        if (!assembly) {
            console.error(`Could not load library part ${libraryPartRef}`)
            return undefined
        }

        const component = await createMirabuf(libraryPartRef, assembly)
        if (!component) {
            console.error(`Could not build a scene object for library part ${libraryPartRef}`)
            return undefined
        }

        World.sceneRenderer.registerSceneObject(component)
        this.configure(component)

        const slot = this._spawnCount++
        moveComponentBy(
            component,
            new THREE.Matrix4().makeTranslation(
                (slot % STAGING_ROW_LENGTH) * STAGING_STEP_METERS,
                0,
                Math.floor(slot / STAGING_ROW_LENGTH) * STAGING_STEP_METERS
            )
        )

        return { component, transform: [...convertThreeMatrix4ToArray(componentWorldTransform(component))] }
    }

    public bind(componentId: ComponentId, component: MirabufSceneObject) {
        this._components.set(componentId, component)
        this._componentBySceneObject.set(component.id, componentId)
    }

    /** Moves a component and everything welded onto it by the same world delta. */
    public moveTree(componentId: ComponentId, delta: THREE.Matrix4) {
        const component = this._components.get(componentId)
        if (!component) return

        moveComponentBy(component, delta)
        this.resyncGizmo(component)

        this.descendantsOf(componentId).forEach(descendantId => {
            const descendant = this._components.get(descendantId)
            if (!descendant) return

            moveComponentBy(descendant, delta)
            this.resyncGizmo(descendant)
        })
    }

    /**
     * Points a component's transform gizmo, if it has one, back at where the component now is.
     *
     * A gizmo caches its parent's pose when it attaches and drags the bodies from that cache, so any
     * placement done in code — a snap, a reconcile — leaves it pointing at the old pose and makes the
     * first frame of the next drag teleport the part back there.
     */
    private resyncGizmo(component: MirabufSceneObject) {
        World.sceneRenderer.gizmosOnMirabuf.get(component.id)?.syncToParent()
    }

    /**
     * Brings the scene in line with a timeline state: spawns components that appeared, drops ones that
     * went away, and re-places everything that moved.
     *
     * Calls are serialized, so scrubbing quickly can't interleave two reconciles.
     */
    public applyState(state: TimelineState): Promise<void> {
        this._pending = this._pending.then(() => this.reconcile(state)).catch(console.error)
        return this._pending
    }

    public remove(componentId: ComponentId) {
        const component = this._components.get(componentId)
        if (!component) return

        this._componentBySceneObject.delete(component.id)
        this._components.delete(componentId)
        this._componentRef.delete(componentId)
        World.sceneRenderer.removeSceneObject(component.id)
    }

    /**
     * Turns the recorded welds into real fixed constraints between the components' root bodies.
     *
     * The constraint locks whatever relative pose the two parts are actually sitting in, which is the
     * offset recorded at weld time unless the user has since repositioned the child on purpose.
     *
     * @param   state Timeline state to bake. Welds naming a missing component are skipped.
     * @returns How many welds were baked.
     */
    public bakeWelds(state: TimelineState): number {
        let baked = 0

        weldPairs(state).forEach(({ parentId, childId }) => {
            const parentBodyId = this.rootBodyOf(parentId)
            const childBodyId = this.rootBodyOf(childId)
            const parentBody = parentBodyId ? World.physicsSystem.getBody(parentBodyId) : undefined
            const childBody = childBodyId ? World.physicsSystem.getBody(childBodyId) : undefined

            if (!parentBody || !childBody) {
                console.warn(`Skipping weld ${childId} -> ${parentId}: missing root body`)
                return
            }

            weldBodies(parentBody, childBody)
            baked++
        })

        return baked
    }

    /**
     * Tears the build down.
     *
     * @param keepComponents Leave the spawned assemblies in the scene, for handing a finished build
     *                       off to normal simulation instead of throwing it away.
     */
    public dispose(keepComponents: boolean) {
        World.sceneRenderer.removeSceneObject(this._weldFollowerId)

        if (keepComponents) {
            // The build is now one robot made of many bodies, so it keeps the shared layer it was
            // assembled on rather than handing it back to the pool.
            this._components.forEach(component => component.enablePhysics())
        } else {
            ;[...this._components.keys()].forEach(componentId => this.remove(componentId))
            this._layerReserve.release()
        }

        this._components.clear()
        this._componentBySceneObject.clear()
        this._componentRef.clear()
    }

    /**
     * The assembly to save a finished build as.
     *
     * Welds form a tree, so the root of that tree is the natural stand-in for the whole robot; without
     * any welds it's simply the first part placed.
     */
    public rootAssembly(state: TimelineState): mirabuf.Assembly | undefined {
        const rootId = [...state.components.values()].find(component => !component.weld)?.id
        const component = rootId ? this._components.get(rootId) : undefined

        return component?.mirabufInstance.parser.assembly
    }

    private configure(component: MirabufSceneObject) {
        // Components are placed, not driven. A brain per part would hand out an input scheme per part
        // and fight the user for the keyboard while they build.
        component.brain = undefined

        component.getAllBodyIds().forEach(bodyId => {
            World.physicsSystem.setBodyObjectLayer(bodyId, this._layerReserve.layer)
        })
        component.mechanism.ghostBodies.forEach(bodyId => {
            World.physicsSystem.setBodyObjectLayer(bodyId, this._layerReserve.layer)
        })
        component.mechanism.layerReserve?.release()

        component.disablePhysics()
    }

    /**
     * The library part a component should actually be built from, which is a size variant when one has
     * been picked and the part it was spawned from otherwise.
     */
    private resolveRef(componentState: ComponentState): LibraryPartRef {
        if (!componentState.sizeOption) return componentState.libraryPartRef

        const size = PartLibrary.sizesFor(componentState.libraryPartRef).find(
            option => option.id === componentState.sizeOption
        )
        if (!size) console.warn(`Unknown size ${componentState.sizeOption} for ${componentState.libraryPartRef}`)

        return size?.partRef ?? componentState.libraryPartRef
    }

    private async reconcile(state: TimelineState) {
        this._state = state

        ;[...this._components.keys()]
            .filter(componentId => !state.components.has(componentId))
            .forEach(componentId => this.remove(componentId))

        for (const [componentId, componentState] of state.components) {
            const ref = this.resolveRef(componentState)

            // A resize swaps in a different assembly, so the old one is torn down and replaced. The
            // component is put back at the same root transform, which leaves welded neighbours where
            // they are; any gap the new size opens up is not auto-corrected.
            if (this._components.has(componentId) && this._componentRef.get(componentId) !== ref) {
                this.remove(componentId)
            }

            if (!this._components.has(componentId)) {
                const staged = await this.stage(ref)
                if (staged) {
                    this.bind(componentId, staged.component)
                    this._componentRef.set(componentId, ref)
                }
            }

            const component = this._components.get(componentId)
            if (!component) continue

            setComponentWorldTransform(component, convertArrayToThreeMatrix4(componentState.transform))
            this.resyncGizmo(component)
            // Re-asserted every sync: attaching and then dropping a transform gizmo re-enables physics
            // on its parent, and build mode wants collision off for the whole session.
            component.disablePhysics()
        }
    }
}

export default MixAndMatchScene
