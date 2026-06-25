import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { GlobalPreferences, ZonePreferencesShared } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import { deltaFieldTransformsPhysicalProp, type VisualProperties } from "@/util/threejs/MeshCreation"
import type MirabufSceneObject from "./MirabufSceneObject"

export default abstract class ZoneSceneObject<P extends object> extends SceneObject {
    public static lightRedMaterial = new THREE.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    public static lightBlueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })

    public static darkRedMaterial = new THREE.MeshPhongMaterial({
        color: 0xff0000,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })
    public static darkBlueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0022ff,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })

    static transparentMaterial = new THREE.MeshPhongMaterial({
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
    private preferenceKey: keyof GlobalPreferences

    public toRender: boolean | undefined
    public mesh?: THREE.Mesh

    public bounding?: Jolt.AABox

    public abstract get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial }

    set deltaTransformation(delta: THREE.Matrix4) {
        this._deltaTransHasUpdated = true
        this._deltaTransformation = delta
    }

    public constructor(
        parentAssembly: MirabufSceneObject,
        prefs: ZonePreferencesShared & P, // TODO maybe switch to `ZonePreferences`
        preferenceKey: keyof GlobalPreferences
    ) {
        super()

        this._parentAssembly = parentAssembly
        this.prefs = prefs
        this.preferenceKey = preferenceKey
        this.toRender = PreferencesSystem.getGlobalPreference(preferenceKey) as boolean | undefined
    }

    public setup() {
        if (!this.prefs) return

        this.parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
            this.prefs.parentNode ?? this._parentAssembly.rootNodeId
        )
        if (!this.parentBodyId) return

        this._deltaTransformation = convertArrayToThreeMatrix4(this.prefs.deltaTransformation)
        const fieldTransformation = convertJoltMat44ToThreeMatrix4(
            World.physicsSystem.getBody(this.parentBodyId)!.GetWorldTransform()
        )
        const props: VisualProperties = deltaFieldTransformsPhysicalProp(this._deltaTransformation, fieldTransformation)

        this.createVisualMesh(props)
        this.createBoundingBox(props)
    }

    private createBoundingBox(props: VisualProperties) {
        this.bounding = new JOLT.AABox().sFromTwoPoints(
            convertThreeVector3ToJoltVec3(props.translation),
            convertThreeVector3ToJoltVec3(props.scale)
        )
    }

    private setMeshProperties(props: VisualProperties) {
        if (!this.mesh) return

        this.mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
        this.mesh.rotation.setFromQuaternion(props.rotation)
        this.mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)

        this.mesh.material = this.prefs.alliance == "red" ? this.materials.red : this.materials.blue
    }

    // Creates a mesh for the user to visualize the sensor
    private createVisualMesh(props: VisualProperties) {
        const unitVector = new JOLT.Vec3(1, 1, 1)

        this.mesh = World.sceneRenderer.createBox(unitVector, ZoneSceneObject.transparentMaterial)
        World.sceneRenderer.scene.add(this.mesh)

        if (this.toRender) {
            this.setMeshProperties(props)
        }

        JOLT.destroy(unitVector)
    }

    private updateRenderPreferences() {
        // If we don't want to render, then there's no point in updating the transforms
        this.toRender = PreferencesSystem.getGlobalPreference(this.preferenceKey) as boolean | undefined
        if (!this.toRender && this.mesh) {
            this.mesh.material = ZoneSceneObject.transparentMaterial
            return
        }
    }

    /**
     * Returns `undefined` when the visual properties for this zone have not changed
     */
    private generateVisualProperties(): VisualProperties | undefined {
        // Update translation, rotation, and scale only if the field has moved
        const transform = World.physicsSystem.getBody(this.parentBodyId!)!.GetWorldTransform()
        if (
            this._cachedFieldTransformation &&
            transform.Equals(this._cachedFieldTransformation) &&
            !this._deltaTransHasUpdated
        )
            return undefined

        this._cachedFieldTransformation = transform
        this._deltaTransHasUpdated = false

        const fieldTransformation = convertJoltMat44ToThreeMatrix4(transform, true)
        return deltaFieldTransformsPhysicalProp(this._deltaTransformation!, fieldTransformation)
    }

    public update() {
        // console.log("updated")
        if (!this.parentBodyId || !this._deltaTransformation || !this.prefs) return

        this.checkObjectsInZone()

        const props = this.generateVisualProperties()
        if (props) {
            this.setMeshProperties(props)
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
        }
    }
}
