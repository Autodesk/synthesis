import Jolt from "@synthesis.adsk/jolt-physics"
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
    convertThreeVector3ToJoltRVec3,
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
    public deltaTransformation?: THREE.Matrix4

    public prefs: ZonePreferencesShared & P
    private preferenceKey: keyof UserPreferences

    public toRender: boolean | undefined
    public joltBodyId?: Jolt.BodyID
    public mesh?: THREE.Mesh
    public unsubscribers: (() => void)[] = []

    public abstract get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial }

    public constructor(
        parentAssembly: MirabufSceneObject,
        prefs: ZonePreferencesShared & P, // TODO maybe switch to `ZonePreferences`
        preferenceKey: keyof UserPreferences,
        render?: boolean
    ) {
        super()

        this._parentAssembly = parentAssembly
        this.toRender = render
        this.prefs = prefs
        this.preferenceKey = preferenceKey
    }

    public setup() {
        if (!this.prefs) return

        this.parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
            this.prefs.parentNode ?? this._parentAssembly.rootNodeId
        )
        if (!this.parentBodyId) return

        this.createDefaultSensor()
        if (!this.joltBodyId) {
            console.log("Failed to create protected zone. No Jolt Body")
            return
        }

        this.deltaTransformation = convertArrayToThreeMatrix4(this.prefs.deltaTransformation)
        const fieldTransformation = convertJoltMat44ToThreeMatrix4(
            World.physicsSystem.getBody(this.parentBodyId)!.GetWorldTransform()
        )
        const props: VisualProperties = deltaFieldTransformsPhysicalProp(this.deltaTransformation, fieldTransformation)

        this.setSensorProperties(props, this.joltBodyId)
        this.createVisualMesh(props)
        this.setupCollisionSubscribers()
    }

    // Creates a default sensor
    // Sets `this._joltBodyId` to equal the body id of the new sensor
    private createDefaultSensor() {
        const unitVector = new JOLT.Vec3(1, 1, 1)

        const settings = new JOLT.BoxShapeSettings(unitVector)
        this.joltBodyId = World.physicsSystem.createSensor(settings)

        JOLT.destroy(unitVector)
    }

    // Position/rotate/scale sensor to settings
    private setSensorProperties(props: VisualProperties, bodyId: Jolt.BodyID) {
        World.physicsSystem.setBodyPosition(bodyId, convertThreeVector3ToJoltRVec3(props.translation))
        World.physicsSystem.setBodyRotation(bodyId, convertThreeQuaternionToJoltQuat(props.rotation))

        const boundingVec = new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
        const shapeSettings = new JOLT.BoxShapeSettings(boundingVec)
        const shape = shapeSettings.Create()

        World.physicsSystem.setShape(bodyId, shape.Get(), false, Jolt.EActivation_Activate)

        JOLT.destroy(boundingVec)
        JOLT.destroy(shapeSettings)
        JOLT.destroy(shape)
    }

    // Creates a mesh for the user to visualize the sensor
    private createVisualMesh(props: VisualProperties) {
        const unitVector = new JOLT.Vec3(1, 1, 1)

        this.mesh = World.sceneRenderer.createBox(unitVector, ZoneSceneObject.transparentMaterial)
        World.sceneRenderer.scene.add(this.mesh)

        if (this.toRender) {
            this.mesh?.position.set(props.translation.x, props.translation.y, props.translation.z)
            this.mesh?.rotation.setFromQuaternion(props.rotation)
            this.mesh?.scale.set(props.scale.x, props.scale.y, props.scale.z)
        }

        JOLT.destroy(unitVector)
    }

    // Should be overridden by the subclasses
    public abstract setupCollisionSubscribers(): void

    public update() {
        if (!this.parentBodyId || !this.deltaTransformation || !this.joltBodyId || !this.prefs) return

        // Update translation, rotation, and scale
        const fieldTransformation = convertJoltMat44ToThreeMatrix4(
            World.physicsSystem.getBody(this.parentBodyId)!.GetWorldTransform()
        )
        const props = deltaFieldTransformsPhysicalProp(this.deltaTransformation, fieldTransformation)
        this.setSensorProperties(props, this.joltBodyId)

        if (!this.mesh) return

        this.toRender = PreferencesSystem.getUserPreference(this.preferenceKey) as boolean | undefined
        if (!this.toRender) {
            this.mesh.material = ZoneSceneObject.transparentMaterial
            return
        }

        // Mesh for visualization
        this.mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
        this.mesh.rotation.setFromQuaternion(props.rotation)
        this.mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)

        this.mesh.material = this.prefs.alliance == "red" ? this.materials.red : this.materials.blue
    }
}
