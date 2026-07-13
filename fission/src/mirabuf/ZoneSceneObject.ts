import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { UserPreferences, ZonePreferencesShared } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import {
    deltaFieldTransformsPhysicalProp as deltaAndFieldTransformsToVisualProp,
    type VisualProperties,
} from "@/util/threejs/MeshCreation"
import type MirabufSceneObject from "./MirabufSceneObject"

export default abstract class ZoneSceneObject<P extends object> extends SceneObject {
    private static readonly transparentMaterial = new THREE.MeshPhongMaterial({
        color: 0x0000,
        shininess: 0.0,
        opacity: 0.0,
        transparent: true,
    })

    private _parentAssembly: MirabufSceneObject
    public parentBodyId?: Jolt.BodyID

    // Visual Properties Cache
    private _deltaTransformation?: THREE.Matrix4
    private _deltaTransHasUpdated: boolean = false
    private _cachedFieldTransformation?: Jolt.RMat44

    public prefs: ZonePreferencesShared & P
    private preferenceKey: keyof UserPreferences

    public toRender: boolean | undefined
    public mesh?: THREE.Mesh

    public bounding?: Jolt.OrientedBox

    public abstract get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial }

    public set deltaTransformation(delta: THREE.Matrix4) {
        this._deltaTransHasUpdated = true
        this._deltaTransformation = delta
    }

    public constructor(
        parentAssembly: MirabufSceneObject,
        prefs: ZonePreferencesShared & P,
        preferenceKey: keyof UserPreferences
    ) {
        super()

        this._parentAssembly = parentAssembly
        this.prefs = prefs
        this.preferenceKey = preferenceKey
        this.toRender = PreferencesSystem.getUserPreference(preferenceKey) as boolean | undefined
    }

    public setup() {
        this.parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
            this.prefs.parentNode ?? this._parentAssembly.rootNodeId
        )

        if (!this.parentBodyId) return

        this._deltaTransformation = convertArrayToThreeMatrix4(this.prefs.deltaTransformation)
        this._cachedFieldTransformation = World.physicsSystem.getBody(this.parentBodyId)!.GetWorldTransform()

        const fieldTransformation = convertJoltMat44ToThreeMatrix4(this._cachedFieldTransformation)
        const props: VisualProperties = deltaAndFieldTransformsToVisualProp(
            this._deltaTransformation,
            fieldTransformation
        )

        this.createVisualMesh(props)
        this.createBoundingBox(props)
    }

    /**
     * Constructs a bounding box given a set of visual properties, putting it in `this.bounding`
     *
     * The bounding box should be guaranteed to fit the same physical space as `this.mesh`
     */
    private createBoundingBox(props: VisualProperties) {
        if (!this.mesh) return
        if (this.bounding) JOLT.destroy(this.bounding)

        const halfExtent = convertThreeVector3ToJoltVec3(props.scale).Div(2)
        const transform = new JOLT.Mat44().sRotationTranslation(
            convertThreeQuaternionToJoltQuat(props.rotation),
            convertThreeVector3ToJoltVec3(props.translation)
        )

        this.bounding = new JOLT.OrientedBox(transform, halfExtent)
    }

    private setMeshProperties(props: VisualProperties) {
        if (!this.mesh) return

        this.mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
        this.mesh.rotation.setFromQuaternion(props.rotation)
        this.mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)
    }

    // Creates a mesh for the user to visualize the sensor
    private createVisualMesh(props: VisualProperties) {
        const unitVector = new JOLT.Vec3(1, 1, 1)

        this.mesh = World.sceneRenderer.createBox(unitVector, ZoneSceneObject.transparentMaterial)
        World.sceneRenderer.addObject(this.mesh)

        this.setMeshProperties(props)
        this.updateRenderPreferences()

        JOLT.destroy(unitVector)
    }

    private updateRenderPreferences() {
        if (!this.mesh) {
            console.error("No mesh present in zone")
            return
        }

        this.toRender = PreferencesSystem.getUserPreference(this.preferenceKey) as boolean | undefined
        this.mesh.material = this.toRender ? this.material() : ZoneSceneObject.transparentMaterial
    }

    /**
     * @returns `undefined` when the visual properties for this zone have not changed
     */
    private generateVisualProperties(): VisualProperties | undefined {
        // `GetWorldTransform` returns a reference, destroying it causes a memory out of bounds later
        const newTransform = World.physicsSystem.getBody(this.parentBodyId!)!.GetWorldTransform()
        const transformHasNotUpdated =
            this._cachedFieldTransformation && newTransform.Equals(this._cachedFieldTransformation)

        // Update translation, rotation, and scale only if the field has moved
        if (transformHasNotUpdated && !this._deltaTransHasUpdated) return undefined

        if (this._cachedFieldTransformation) JOLT.destroy(this._cachedFieldTransformation)
        this._cachedFieldTransformation = newTransform
        this._deltaTransHasUpdated = false

        const fieldTransformation = convertJoltMat44ToThreeMatrix4(this._cachedFieldTransformation)
        return deltaAndFieldTransformsToVisualProp(this._deltaTransformation!, fieldTransformation)
    }

    private material() {
        const { red, blue } = this.materials
        return this.prefs.alliance == "red" ? red : blue
    }

    public update() {
        if (!this.parentBodyId || !this._deltaTransformation) return

        this.checkObjectsInZone()

        const props = this.generateVisualProperties()
        if (props) {
            this.setMeshProperties(props)
            this.createBoundingBox(props)
        }

        this.updateRenderPreferences()
    }

    public abstract checkObjectsInZone(): void

    public dispose(): void {
        if (this.bounding) {
            JOLT.destroy(this.bounding)
        }

        if (this.mesh) {
            World.sceneRenderer.removeObject(this.mesh)
            this.mesh.geometry.dispose()
        }

        if (this._cachedFieldTransformation) {
            JOLT.destroy(this._cachedFieldTransformation)
        }
    }
}
