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
import {
    deltaFieldTransformsPhysicalProp as deltaAndFieldTransformsToVisualProp,
    type VisualProperties,
} from "@/util/threejs/MeshCreation"
import type MirabufSceneObject from "./MirabufSceneObject"
import { copyJoltRMat44, renderAABox } from "@/util/Utility"
import ProtectedZoneSceneObject from "./ProtectedZoneSceneObject"

export default abstract class ZoneSceneObject<P extends object> extends SceneObject {
    public static readonly lightRedMaterial = new THREE.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    public static readonly lightBlueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })

    public static readonly darkRedMaterial = new THREE.MeshPhongMaterial({
        color: 0xff0000,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })
    public static readonly darkBlueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0022ff,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })

    static readonly transparentMaterial = new THREE.MeshPhongMaterial({
        color: 0x0000,
        shininess: 0.0,
        opacity: 0.0,
        transparent: true,
    })

    static count: number = 0

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

    // Debugging variables
    private lineTmp?: THREE.Line
    private hasLogged: boolean = false

    set deltaTransformation(delta: THREE.Matrix4) {
        console.log("Updating Delta Transform")
        this._deltaTransHasUpdated = true
        this._deltaTransformation = delta
    }

    public constructor(
        parentAssembly: MirabufSceneObject,
        prefs: ZonePreferencesShared & P,
        preferenceKey: keyof GlobalPreferences
    ) {
        super()

        this._parentAssembly = parentAssembly
        this.prefs = prefs
        this.preferenceKey = preferenceKey
        this.toRender = PreferencesSystem.getGlobalPreference(preferenceKey) as boolean | undefined
    }

    public setup() {
        this.parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
            this.prefs.parentNode ?? this._parentAssembly.rootNodeId
        )
        if (!this.parentBodyId) return

        this._deltaTransformation = convertArrayToThreeMatrix4(this.prefs.deltaTransformation)
        this._cachedFieldTransformation = copyJoltRMat44(
            World.physicsSystem.getBody(this.parentBodyId)!.GetWorldTransform()
        )

        const fieldTransformation = convertJoltMat44ToThreeMatrix4(this._cachedFieldTransformation)
        const props: VisualProperties = deltaAndFieldTransformsToVisualProp(
            this._deltaTransformation,
            fieldTransformation
        )

        console.log(`Setup Transform: ${JSON.stringify(fieldTransformation)}`)

        this.createVisualMesh(props)
        this.createBoundingBox(props)
    }

    private createBoundingBox(props: VisualProperties) {
        if (this.bounding) JOLT.destroy(this.bounding)
        if (this.lineTmp) World.sceneRenderer.removeObject(this.lineTmp)

        const origin = new JOLT.Vec3(0, 0, 0)
        const unit = new JOLT.Vec3(1, 1, 1)

        this.bounding = new JOLT.AABox(origin, unit)
        this.bounding = this.bounding.Scaled(convertThreeVector3ToJoltVec3(props.scale).Div(2))
        this.bounding.TranslateVec3(convertThreeVector3ToJoltVec3(props.translation))

        this.lineTmp = renderAABox(this.bounding)

        JOLT.destroy(origin)
        JOLT.destroy(unit)
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
        World.sceneRenderer.addObject(this.mesh)

        if (this.toRender) {
            this.setMeshProperties(props)
        }

        JOLT.destroy(unitVector)
    }

    private updateRenderPreferences() {
        this.toRender = PreferencesSystem.getGlobalPreference(this.preferenceKey) as boolean | undefined
        if (!this.toRender && this.mesh) {
            this.mesh.material = ZoneSceneObject.transparentMaterial
        }
    }

    /**
     * Returns `undefined` when the visual properties for this zone have not changed
     */
    private generateVisualProperties(): VisualProperties | undefined {
        // Update translation, rotation, and scale only if the field has moved
        const newTransform = World.physicsSystem.getBody(this.parentBodyId!)!.GetWorldTransform()
        if (!this.hasLogged && this instanceof ProtectedZoneSceneObject) {
            console.log(
                `Old     Transform: ${JSON.stringify(convertJoltMat44ToThreeMatrix4(this._cachedFieldTransformation!))}`
            )
            console.log(`Updated Transform: ${JSON.stringify(convertJoltMat44ToThreeMatrix4(newTransform))}`)

            const hasChanged = this._cachedFieldTransformation && newTransform.Equals(this._cachedFieldTransformation)
            console.log(`F.T. changed: ${!hasChanged}`)
            this.hasLogged = true
        }

        const transformHasNotUpdated =
            this._cachedFieldTransformation && newTransform.Equals(this._cachedFieldTransformation)

        if (transformHasNotUpdated && !this._deltaTransHasUpdated) return undefined

        this._cachedFieldTransformation = copyJoltRMat44(newTransform)
        this._deltaTransHasUpdated = false

        const fieldTransformation = convertJoltMat44ToThreeMatrix4(this._cachedFieldTransformation)
        return deltaAndFieldTransformsToVisualProp(this._deltaTransformation!, fieldTransformation)
    }

    public update() {
        if (!this.parentBodyId || !this._deltaTransformation) return

        this.checkObjectsInZone()

        // Try to update the zone
        // TODO
        // Why the frick did initially caching the field transform cause the mesh to render in the wrong spot?
        // Why and where are the transforms changing?
        const props = this.generateVisualProperties()
        if (props) {
            // For some reason, the visual properties only get reset exactly once across all zones
            // Only in the blue scoring zone, for the 2023 field
            console.log(`Setting Visual Properties: ${ZoneSceneObject.count++}`)
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
        }
    }
}
