import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import type { mirabuf } from "@/proto/mirabuf"
import type {
    FieldConfiguration,
    LocalSceneObjectId,
    RemoteSceneObjectId,
    RobotConfiguration,
    UpdateObjectData,
} from "@/systems/multiplayer/types"
import { BodyAssociate } from "@/systems/physics/BodyAssociate.ts"
import { OnContactAddedEvent } from "@/systems/physics/ContactEvents"
import type Mechanism from "@/systems/physics/Mechanism"
import type { LayerReserve } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    type Alliance,
    defaultFieldSpawnLocation,
    defaultRobotSpawnLocation,
    type EjectorPreferences,
    type FieldPreferences,
    type IntakePreferences,
    type ProtectedZonePreferences,
    type ScoringZonePreferences,
    type SpawnLocation,
    type Station,
} from "@/systems/preferences/PreferenceTypes"
import type { CustomOrbitControls } from "@/systems/scene/CameraControls"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import type Brain from "@/systems/simulation/Brain"
import type { SimConfigData } from "@/systems/simulation/SimConfigShared"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import type { ContextData, ContextSupplier } from "@/ui/components/ContextMenuData"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import type { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import { SceneOverlayTag } from "@/ui/components/SceneOverlayEvents"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertJoltMat44ToThreeMatrix4,
    convertJoltRVec3ToJoltVec3,
    convertJoltVec3ToJoltRVec3,
    convertJoltVec3ToThreeVector3,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import { createMeshForShape } from "@/util/threejs/MeshCreation.ts"
import SceneObject from "../systems/scene/SceneObject"
import EjectableSceneObject from "./EjectableSceneObject"
import FieldMiraEditor, { devtoolHandlers, devtoolKeys } from "./FieldMiraEditor"
import IntakeSensorSceneObject from "./IntakeSensorSceneObject"
import MirabufInstance from "./MirabufInstance"
import { MiraType } from "./MirabufLoader"
import MirabufParser, { ParseErrorSeverity, type RigidNodeId, type RigidNodeReadOnly } from "./MirabufParser"
import ProtectedZoneSceneObject from "./ProtectedZoneSceneObject"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"

const DEBUG_BODIES = false

interface RnDebugMeshes {
    colliderMesh: THREE.Mesh
    comMesh: THREE.Mesh
}

/**
 * The goal with the spotlight assembly is to provide a contextual target assembly
 * the user would like to modify. Generally this will be which even assembly was
 * last spawned in, however, systems (such as the configuration UI) can elect
 * assemblies to be in the spotlight when moving from interface to interface.
 */
let spotlightAssembly: number | undefined

export function setSpotlightAssembly(assembly: MirabufSceneObject) {
    spotlightAssembly = assembly.id
}

// TODO: If nothing is in the spotlight, select last entry before defaulting to undefined
export function getSpotlightAssembly(): MirabufSceneObject | undefined {
    return World.sceneRenderer.sceneObjects.get(spotlightAssembly ?? 0) as MirabufSceneObject
}

class MirabufSceneObject extends SceneObject implements ContextSupplier {
    private readonly _assemblyName: string
    private readonly _mirabufInstance: MirabufInstance
    private readonly _mechanism: Mechanism

    private _brain: Brain | undefined
    private _alliance: Alliance | undefined
    private _station: Station | undefined

    private _debugBodies: Map<string, RnDebugMeshes> | null
    private _physicsLayerReserve: LayerReserve | undefined

    private _intakePreferences: IntakePreferences | undefined
    private _ejectorPreferences: EjectorPreferences | undefined
    private _simConfigData: SimConfigData | undefined

    private _fieldPreferences: FieldPreferences | undefined

    private _ejectables: EjectableSceneObject[] = []
    private _intakeSensor?: IntakeSensorSceneObject
    private _scoringZones: ScoringZoneSceneObject[] = []
    private _protectedZones: ProtectedZoneSceneObject[] = []

    private _nameOverride?: string
    private _nameTag: SceneOverlayTag | undefined
    private _centerOfMassIndicator: THREE.Mesh | undefined
    private _basePositionTransform: THREE.Vector3 | undefined
    private _intakeActive = false
    private _ejectorActive = false

    private _multiplayerOwningClientId?: string

    private _lastEjectableToastTime = 0
    private static readonly EJECTABLE_TOAST_COOLDOWN_MS = 500

    private _collision?: (event: OnContactAddedEvent) => void

    public get scoringZones(): Readonly<ScoringZoneSceneObject[]> {
        return this._scoringZones
    }

    public set nameOverride(name: string | undefined) {
        this._nameOverride = name
    }

    public set multiplayerOwningClientId(id: string | undefined) {
        this._multiplayerOwningClientId = id
    }

    public get multiplayerOwnerName(): string | undefined {
        if (this._multiplayerOwningClientId == null) return undefined
        return World.multiplayerSystem?._clientToInfoMap?.get(this._multiplayerOwningClientId)?.displayName
    }

    public get intakeActive() {
        return this._intakeActive
    }

    public get ejectorActive() {
        return this._ejectorActive
    }

    public set intakeActive(a: boolean) {
        this._intakeActive = a
    }

    public set ejectorActive(a: boolean) {
        this._ejectorActive = a
    }

    public set mirabufInstance(a: MirabufInstance) {
        this.mirabufInstance = a
    }

    get mirabufInstance() {
        return this._mirabufInstance
    }

    get mechanism() {
        return this._mechanism
    }

    get assemblyName() {
        return this._assemblyName
    }

    get intakePreferences() {
        return this._intakePreferences
    }

    get ejectorPreferences() {
        return this._ejectorPreferences
    }

    get simConfigData() {
        return this._simConfigData
    }

    get fieldPreferences() {
        return this._fieldPreferences
    }

    get nameTag() {
        return this._nameTag
    }

    get isOwnObject() {
        return this._multiplayerOwningClientId == undefined
    }

    public get activeEjectables(): Jolt.BodyID[] {
        return this._ejectables.map(e => e.gamePieceBodyId!).filter(x => x !== undefined)
    }

    public get miraType(): MiraType {
        return this._mirabufInstance.parser.assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD
    }

    public get rootNodeId(): string {
        return this._mirabufInstance.parser.rootNode
    }

    public get brain() {
        return this._brain
    }

    public get alliance() {
        return this._alliance
    }

    public get station() {
        return this._station
    }

    public set brain(brain: Brain | undefined) {
        this._brain = brain
        const simLayer = World.simulationSystem.getSimulationLayer(this._mechanism)!
        simLayer.setBrain(brain)
    }

    public set alliance(alliance: Alliance | undefined) {
        this._alliance = alliance
    }

    public set station(station: Station | undefined) {
        this._station = station
    }

    public constructor(
        mirabufInstance: MirabufInstance,
        assemblyName: string,
        progressHandle?: ProgressHandle,
        multiplayerOwnerId?: string
    ) {
        super()
        this._mirabufInstance = mirabufInstance
        this._assemblyName = assemblyName
        this._multiplayerOwningClientId = multiplayerOwnerId

        progressHandle?.update("Creating mechanism...", 0.9)

        this._mechanism = World.physicsSystem.createMechanismFromParser(this._mirabufInstance.parser)
        if (this._mechanism.layerReserve) this._physicsLayerReserve = this._mechanism.layerReserve

        this._debugBodies = null

        this.getPreferences()

        if (this.miraType === MiraType.ROBOT) {
            // creating nametag for robots
            this._nameTag = new SceneOverlayTag(() => {
                const name =
                    this._nameOverride ??
                    (this._brain instanceof SynthesisBrain
                        ? this._brain.inputSchemeName
                        : this._brain instanceof WPILibBrain
                          ? "Magic"
                          : "Not Configured")
                if (World.multiplayerSystem != null) {
                    return `${name} (${this.alliance === "red" ? "R" : this.alliance === "blue" ? "B" : "..."}${this.station ?? ""})`
                }
                return name
            })

            // Detects when something collides with the robot
            this._collision = (event: OnContactAddedEvent) => {
                const body1 = event.message.body1
                const body2 = event.message.body2

                if (body1.GetIndexAndSequenceNumber() === this.getRootNodeId()?.GetIndexAndSequenceNumber()) {
                    this.recordRobotCollision(body2)
                } else if (body2.GetIndexAndSequenceNumber() === this.getRootNodeId()?.GetIndexAndSequenceNumber()) {
                    this.recordRobotCollision(body1)
                }
            }
            OnContactAddedEvent.addListener(this._collision)

            // Center of Mass Indicator
            const material = new THREE.MeshBasicMaterial({
                color: 0xff00ff, // purple
                transparent: true,
                opacity: 0.1,
                wireframe: true,
            })
            material.depthTest = false
            this._centerOfMassIndicator = new THREE.Mesh(new THREE.SphereGeometry(0.02), material)
            this._centerOfMassIndicator.visible = false
            World.sceneRenderer.scene.add(this._centerOfMassIndicator)
        }
    }

    public setup(): void {
        // Rendering
        this._mirabufInstance.addToScene(World.sceneRenderer.scene)

        if (DEBUG_BODIES) {
            this._debugBodies = new Map()
            this._mechanism.nodeToBody.forEach((bodyId, rnName) => {
                const body = World.physicsSystem.getBody(bodyId)

                const colliderMesh = this.createMeshForShape(body.GetShape())
                const comMesh = World.sceneRenderer.createSphere(0.05)
                World.sceneRenderer.scene.add(colliderMesh)
                World.sceneRenderer.scene.add(comMesh)
                ;(comMesh.material as THREE.Material).depthTest = false
                this._debugBodies!.set(rnName, {
                    colliderMesh: colliderMesh,
                    comMesh: comMesh,
                })
            })
        }

        const rigidNodes = this._mirabufInstance.parser.rigidNodes
        this._mechanism.nodeToBody.forEach((bodyId, rigidNodeId) => {
            const rigidNode = rigidNodes.get(rigidNodeId)
            if (!rigidNode) {
                console.warn("Found a RigidNodeId with no related RigidNode. Skipping for now...")
                return
            }
            World.physicsSystem.setBodyAssociation(new RigidNodeAssociate(this, rigidNode, bodyId))
        })

        // Simulation
        if (this.miraType === MiraType.ROBOT) {
            World.simulationSystem.registerMechanism(this._mechanism)
            const simLayer = World.simulationSystem.getSimulationLayer(this._mechanism)!
            this._brain = new SynthesisBrain(this, this._assemblyName)
            simLayer.setBrain(this._brain)
        }

        // Intake
        this.updateIntakeSensor()
        this.updateScoringZones()
        this.updateProtectedZones()

        if (this.isOwnObject) {
            setSpotlightAssembly(this)
        }

        this.updateBatches()

        this._basePositionTransform = this.getPositionTransform(new THREE.Vector3())

        this.moveToSpawnLocation()

        const cameraControls = World.sceneRenderer.currentCameraControls as CustomOrbitControls

        if (this.isOwnObject && (this.miraType === MiraType.ROBOT || !cameraControls.focusProvider)) {
            cameraControls.focusProvider = this
        }

        MirabufObjectChangeEvent.dispatch(this)
    }

    // Centered in xz plane, bottom surface of object
    public getPositionTransform(vec: THREE.Vector3 = new THREE.Vector3()) {
        const box = this.computeBoundingBox()
        const transform = box.getCenter(vec)
        transform.setY(box.min.y)
        return transform
    }

    public moveToSpawnLocation() {
        let pos: SpawnLocation = defaultRobotSpawnLocation()
        const referencePos = new THREE.Vector3()
        if (this.miraType == MiraType.FIELD) {
            pos = defaultFieldSpawnLocation()
        } else {
            const field = World.sceneRenderer.mirabufSceneObjects.getField()
            const fieldLocations = field?.fieldPreferences?.spawnLocations
            if (this._alliance != null && this._station != null && fieldLocations != null) {
                pos = fieldLocations[this._alliance][this._station]
            } else {
                pos = fieldLocations?.default ?? pos
            }
            field?.getPositionTransform(referencePos)
        }
        this.setObjectPosition(pos, referencePos)
    }

    private setObjectPosition(initialPos: SpawnLocation, referencePosition: THREE.Vector3) {
        const bounds = this.computeBoundingBox()
        if (!Number.isFinite(bounds.min.y)) return

        // If anyone has ideas on how to make this more concise I would appreciate.
        // It took much longer than expected to deal with this
        // (set position seems to use some arbitrary part of the robot, Dozer's is like half a meter in front to the left and 2471's is in the center)
        const bodyCenter = convertThreeVector3ToJoltVec3(bounds.getCenter(new THREE.Vector3()))
        const rotatedBasePositionTransform = this._basePositionTransform!.clone().applyAxisAngle(
            new THREE.Vector3(0, 1, 0),
            initialPos.yaw
        )
        const initialTranslation = new JOLT.Vec3(
            initialPos.pos[0] - rotatedBasePositionTransform.x + referencePosition.x,
            initialPos.pos[1] - rotatedBasePositionTransform.y + referencePosition.y,
            initialPos.pos[2] - rotatedBasePositionTransform.z + referencePosition.z
        )
        const initialRotation = JOLT.Quat.prototype.sRotation(new JOLT.Vec3(0, 1, 0), initialPos.yaw)
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            const jBodyId = this._mechanism.getBodyByNodeId(rn.id)
            if (!jBodyId) return
            const offset = convertJoltRVec3ToJoltVec3(
                World.physicsSystem.getBody(jBodyId).GetPosition().Sub(bodyCenter)
            )
            const newPos = convertJoltVec3ToJoltRVec3(initialTranslation)
            World.physicsSystem.setBodyPositionRotationAndVelocity(
                jBodyId,
                newPos,
                initialRotation,
                new JOLT.Vec3(),
                new JOLT.Vec3()
            )

            JOLT.destroy(offset)
            JOLT.destroy(newPos)
        })
        JOLT.destroy(initialTranslation)
        JOLT.destroy(initialRotation)
        this.updateMeshTransforms()
    }

    public update(): void {
        if (this.ejectorActive) {
            this.eject()
        }

        this.updateMeshTransforms()
        this.updateBatches()
        this.updateNameTag()
    }

    public dispose(): void {
        if (this._intakeSensor) {
            World.sceneRenderer.removeSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        this._ejectables.forEach(e => World.sceneRenderer.removeSceneObject(e.id))

        this._scoringZones.forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._scoringZones = []

        this._protectedZones.forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._protectedZones = []

        this._mechanism.nodeToBody.forEach(bodyId => {
            World.physicsSystem.removeBodyAssociation(bodyId)
        })

        this._nameTag?.dispose()
        World.simulationSystem.unregisterMechanism(this._mechanism)
        World.physicsSystem.destroyMechanism(this._mechanism)
        this._mirabufInstance.dispose(World.sceneRenderer.scene)
        this._debugBodies?.forEach(x => {
            World.sceneRenderer.scene.remove(x.colliderMesh, x.comMesh)
            x.colliderMesh.geometry.dispose()
            x.comMesh.geometry.dispose()
            ;(x.colliderMesh.material as THREE.Material).dispose()
            ;(x.comMesh.material as THREE.Material).dispose()
        })
        this._debugBodies?.clear()
        this._physicsLayerReserve?.release()
        if (this._centerOfMassIndicator) {
            World.sceneRenderer.scene.remove(this._centerOfMassIndicator)
            this._centerOfMassIndicator = undefined
        }

        if (this._brain && this._brain instanceof SynthesisBrain) {
            this._brain.clearControls()
        }
        MirabufObjectChangeEvent.dispatch(null)
    }

    public eject() {
        if (this._ejectables.length === 0) return

        const order = this._ejectorPreferences?.ejectOrder
        let ejectable: EjectableSceneObject | undefined

        if (order === "FIFO") ejectable = this._ejectables.shift()
        else ejectable = this._ejectables.pop()

        if (!ejectable) return

        ejectable.eject()
        World.sceneRenderer.removeSceneObject(ejectable.id)
    }

    private createMeshForShape(shape: Jolt.Shape): THREE.Mesh {
        const geometry = createMeshForShape(shape)

        const material = new THREE.MeshStandardMaterial({
            color: 0x33ff33,
            wireframe: true,
        })
        const mesh = new THREE.Mesh(geometry, material)
        mesh.castShadow = true

        return mesh
    }

    /**
     * Matches mesh transforms to their Jolt counterparts.
     */
    public updateMeshTransforms() {
        let weightedCOM = new JOLT.RVec3(0, 0, 0)
        let totalMass = 0
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            if (!this._mirabufInstance.meshes.size) return // if this.dispose() has been ran then return
            const bodyId = this._mechanism.getBodyByNodeId(rn.id)!
            const body = World.physicsSystem.getBody(bodyId)
            if (!body) return
            const transform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())
            this.updateNodeParts(rn, transform)

            if (Number.isNaN(body.GetPosition().GetX())) {
                const vel = body.GetLinearVelocity()
                const pos = body.GetPosition()
                console.warn(
                    `Invalid Position.\nPosition => ${pos.GetX()}, ${pos.GetY()}, ${pos.GetZ()}\nVelocity => ${vel.GetX()}, ${vel.GetY()}, ${vel.GetZ()}`
                )
            }

            if (this._debugBodies) {
                const { colliderMesh, comMesh } = this._debugBodies.get(rn.id)!
                colliderMesh.position.setFromMatrixPosition(transform)
                colliderMesh.rotation.setFromRotationMatrix(transform)

                const comTransform = convertJoltMat44ToThreeMatrix4(body.GetCenterOfMassTransform())

                comMesh.position.setFromMatrixPosition(comTransform)
                comMesh.rotation.setFromRotationMatrix(comTransform)
            }
            if (this._centerOfMassIndicator) {
                const inverseMass = body.GetMotionProperties().GetInverseMass()

                if (inverseMass > 0) {
                    const mass = 1 / inverseMass
                    weightedCOM = weightedCOM.AddRVec3(body.GetCenterOfMassPosition().Mul(mass))
                    totalMass += mass
                }
            }
        })
        if (this._centerOfMassIndicator) {
            const netCoM = totalMass > 0 ? weightedCOM.Div(totalMass) : weightedCOM
            this._centerOfMassIndicator.position.set(netCoM.GetX(), netCoM.GetY(), netCoM.GetZ())
            this._centerOfMassIndicator.visible = PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")
        }
    }

    public updateNodeParts(rn: RigidNodeReadOnly, transform: THREE.Matrix4) {
        rn.parts.forEach(part => {
            const partTransform = this._mirabufInstance.parser.globalTransforms
                .get(part)!
                .clone()
                .premultiply(transform)
            const meshes = this._mirabufInstance.meshes.get(part) ?? []
            meshes.forEach(([mesh, index]) => {
                mesh.setMatrixAt(index, partTransform)
                // Only update instanceMatrix for InstancedMesh
                if ("instanceMatrix" in mesh) {
                    mesh.instanceMatrix.needsUpdate = true
                }
            })
        })
    }

    /** Updates the batch computations */
    private updateBatches() {
        this._mirabufInstance.batches.forEach(x => {
            x.computeBoundingBox()
            x.computeBoundingSphere()
        })
    }

    /** Updates the position of the nametag relative to the robots position */
    private updateNameTag() {
        if (this._nameTag && PreferencesSystem.getGlobalPreference("RenderSceneTags")) {
            this._nameTag.color = this._alliance
            const boundingBox = this.computeBoundingBox()
            this._nameTag.position = World.sceneRenderer.worldToPixelSpace(
                new THREE.Vector3(
                    (boundingBox.max.x + boundingBox.min.x) / 2,
                    boundingBox.max.y + 0.1,
                    (boundingBox.max.z + boundingBox.min.z) / 2
                )
            )
        }
    }

    public updateIntakeSensor() {
        if (this._intakeSensor) {
            World.sceneRenderer.removeSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        // Do we have an intake, and is it something other than the default. Config will default to root node at least.
        if (this._intakePreferences && this._intakePreferences.parentNode) {
            this._intakeSensor = new IntakeSensorSceneObject(this)
            World.sceneRenderer.registerSceneObject(this._intakeSensor)
        }
    }

    public setIntakeVisualIndicatorVisible(visible: boolean) {
        if (this._intakeSensor) {
            this._intakeSensor.setVisualIndicatorVisible(visible)
        }
    }

    public setEjectable(bodyId?: Jolt.BodyID): boolean {
        if (!bodyId) {
            return false
        }

        if (!this._ejectorPreferences?.parentNode) {
            console.log(bodyId)
            const now = Date.now()
            if (
                (!World.multiplayerSystem || World.multiplayerSystem?.getOwnRobots().includes(this)) &&
                now - this._lastEjectableToastTime > MirabufSceneObject.EJECTABLE_TOAST_COOLDOWN_MS
            ) {
                console.log(`Configure an ejector first.`)
                globalAddToast("info", "Configure Ejector", "Configure an ejector first.")
                this._lastEjectableToastTime = now
            }

            return false
        }

        // 2) don’t exceed your configured maxPieces
        const max = this._intakePreferences?.maxPieces ?? 1
        if (this._ejectables.length >= max) return false

        // 3) avoid duplicates
        const key = bodyId.GetIndexAndSequenceNumber()
        if (this._ejectables.some(e => e.gamePieceBodyId!.GetIndexAndSequenceNumber() === key)) return false

        const ejectable = new EjectableSceneObject(this, bodyId)
        this._ejectables.push(ejectable)
        World.sceneRenderer.registerSceneObject(ejectable)
        return true
    }

    public updateScoringZones(render?: boolean) {
        this._scoringZones.filter(zone => zone.id != -1).forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._scoringZones = []

        if (this._fieldPreferences && this._fieldPreferences.scoringZones) {
            for (let i = 0; i < this._fieldPreferences.scoringZones.length; i++) {
                const newZone = new ScoringZoneSceneObject(
                    this,
                    i,
                    render ?? PreferencesSystem.getGlobalPreference("RenderScoringZones")
                )
                this._scoringZones.push(newZone)
                World.sceneRenderer.registerSceneObject(newZone)
            }
        }
    }

    public updateProtectedZones(render?: boolean) {
        this._protectedZones
            .filter(zone => zone.id != -1)
            .forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._protectedZones = []

        if (this.fieldPreferences && this.fieldPreferences.protectedZones) {
            for (let i = 0; i < this.fieldPreferences.protectedZones.length; i++) {
                const newZone = new ProtectedZoneSceneObject(
                    this,
                    i,
                    render ?? PreferencesSystem.getGlobalPreference("RenderProtectedZones")
                )
                this._protectedZones.push(newZone)
                World.sceneRenderer.registerSceneObject(newZone)
            }
        }
    }

    public removeScoringZoneObject(zone: ScoringZonePreferences) {
        const index = this._fieldPreferences?.scoringZones?.indexOf(zone) ?? -1
        if (index == -1) return

        const zoneObject = this._scoringZones[index]
        if (zoneObject == null) return

        World.sceneRenderer.removeSceneObject(zoneObject.id)
        zoneObject.id = -1
    }

    public removeProtectedZoneObject(zone: ProtectedZonePreferences) {
        const index = this._fieldPreferences?.protectedZones?.indexOf(zone) ?? -1
        if (index == -1) return

        const zoneObject = this._protectedZones[index]
        if (zoneObject == null) return

        World.sceneRenderer.removeSceneObject(zoneObject.id)
        zoneObject.id = -1
    }

    /**
     * Calculates the bounding box of the mirabuf object.
     *
     * @returns The bounding box of the mirabuf object.
     */
    private computeBoundingBox(): THREE.Box3 {
        const box = new THREE.Box3()
        this._mirabufInstance.batches.forEach(batch => {
            if (batch.boundingBox) box.union(batch.boundingBox)
        })

        return box
    }

    /**
     * Gets the maximum dimensions (length, width, height) of the mirabuf object.
     *
     * @returns An object containing the width (x), height (y), and depth (z) dimensions in meters.
     */
    public getDimensions(): { width: number; height: number; depth: number } {
        const boundingBox = this.computeBoundingBox()
        const size = new THREE.Vector3()
        boundingBox.getSize(size)

        return {
            width: size.x,
            height: size.y,
            depth: size.z,
        }
    }

    /**
     * Calculates the robot's dimensions as if it had no rotation applied.
     *
     * @returns the object containing the width (x), height (y), and depth (z) dimensions in meters.
     */
    public getDimensionsWithoutRotation(): {
        width: number
        height: number
        depth: number
    } {
        const rootNodeId = this.getRootNodeId()
        if (!rootNodeId) {
            console.warn("No root node found for robot, using regular dimensions")
            return this.getDimensions()
        }

        const rootBody = World.physicsSystem.getBody(rootNodeId)
        const rootTransform = convertJoltMat44ToThreeMatrix4(rootBody.GetWorldTransform())

        const rootPosition = new THREE.Vector3()
        const rootRotation = new THREE.Quaternion()
        const rootScale = new THREE.Vector3()
        rootTransform.decompose(rootPosition, rootRotation, rootScale)

        // Create inverse rotation matrix to "undo" the robot's rotation
        const inverseRotation = new THREE.Matrix4().makeRotationFromQuaternion(rootRotation.clone().invert())

        const unrotatedBox = new THREE.Box3()

        this._mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
            const bodyId = this._mechanism.getBodyByNodeId(rigidNode.id)
            if (!bodyId) return

            const body = World.physicsSystem.getBody(bodyId)
            const bodyTransform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())

            const shape = body.GetShape()
            const scale = new JOLT.Vec3(1, 1, 1)
            const triangleContext = new JOLT.ShapeGetTriangles(
                shape,
                JOLT.AABox.prototype.sBiggest(),
                shape.GetCenterOfMass(),
                JOLT.Quat.prototype.sIdentity(),
                scale
            )

            try {
                const vertices = new Float32Array(
                    JOLT.HEAP32.buffer,
                    triangleContext.GetVerticesData(),
                    triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT
                )

                for (let i = 0; i < vertices.length; i += 3) {
                    const vertex = new THREE.Vector3(vertices[i], vertices[i + 1], vertices[i + 2])

                    vertex.applyMatrix4(bodyTransform).applyMatrix4(inverseRotation)

                    unrotatedBox.expandByPoint(vertex)
                }
            } finally {
                JOLT.destroy(triangleContext)
                JOLT.destroy(scale)
            }
        })

        // Fallback if no vertices were processed
        if (unrotatedBox.isEmpty()) {
            console.warn("Could not process physics shapes, using regular dimensions")
            return this.getDimensions()
        }

        const unrotatedSize = new THREE.Vector3()
        unrotatedBox.getSize(unrotatedSize)

        return {
            width: unrotatedSize.x,
            height: unrotatedSize.y,
            depth: unrotatedSize.z,
        }
    }

    /**
     * Once a gizmo is created and attached to this mirabuf object, this will be executed to align the gizmo correctly.
     *
     * @param gizmo Gizmo attached to the mirabuf object
     */
    public postGizmoCreation(gizmo: GizmoSceneObject) {
        const jRootId = this.getRootNodeId()
        if (!jRootId) {
            console.error("No root node found.")
            return
        }

        const jBody = World.physicsSystem.getBody(jRootId)
        if (jBody.IsStatic()) {
            const aaBox = jBody.GetWorldSpaceBounds()
            const mat = new THREE.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
            const center = aaBox.mMin.Add(aaBox.mMax).Div(2.0)
            mat.compose(
                convertJoltVec3ToThreeVector3(center),
                new THREE.Quaternion(0, 0, 0, 1),
                new THREE.Vector3(1, 1, 1)
            )
            gizmo.setTransform(mat)
        } else {
            gizmo.setTransform(convertJoltMat44ToThreeMatrix4(jBody.GetCenterOfMassTransform()))
        }
    }

    public async sendPreferences() {
        if (!World.multiplayerSystem) return
        const data = this.getPreferenceData()
        await World.multiplayerSystem.broadcast({
            type: "configureObject",
            data: {
                sceneObjectKey: this.id as RemoteSceneObjectId,
                objectConfigurationData: data,
            },
        })
    }

    public getPreferences(): void {
        const robotPrefs = PreferencesSystem.getRobotPreferences(this.assemblyName)
        if (robotPrefs) {
            this._intakePreferences = robotPrefs.intake
            // Ensure backwards compatibility for showZoneAlways field
            if (this._intakePreferences && this._intakePreferences.showZoneAlways === undefined) {
                this._intakePreferences.showZoneAlways = false
            }
            this._ejectorPreferences = robotPrefs.ejector
            this._simConfigData = robotPrefs.simConfig

            this.sendPreferences()
        }

        this._fieldPreferences = PreferencesSystem.getFieldPreferences(this.assemblyName)

        // For fields, sync devtool data with field preferences
        if (this.miraType === MiraType.FIELD) {
            const parts = this._mirabufInstance.parser.assembly.data?.parts
            if (parts) {
                const editor = new FieldMiraEditor(parts)
                devtoolKeys.forEach(key => {
                    devtoolHandlers[key].set(this, editor.getUserData(key))
                })
                PreferencesSystem.setFieldPreferences(this.assemblyName, this._fieldPreferences)
                PreferencesSystem.savePreferences()
            }
        }
    }

    public getPreferenceData(): FieldConfiguration | RobotConfiguration {
        return this.miraType == MiraType.FIELD
            ? {
                  fieldPreferences: JSON.stringify(this._fieldPreferences),
              }
            : {
                  intakePreferences: JSON.stringify(this._intakePreferences),
                  ejectorPreferences: JSON.stringify(this._ejectorPreferences),
                  alliance: this._alliance,
                  station: this.station,
              }
    }

    public setPreferenceData(preferences: FieldConfiguration | RobotConfiguration) {
        if (this.miraType === MiraType.FIELD) {
            const config = preferences as FieldConfiguration
            this._fieldPreferences = JSON.parse(config.fieldPreferences)
        } else {
            const config = preferences as RobotConfiguration
            this._intakePreferences = JSON.parse(config.intakePreferences)
            this._ejectorPreferences = JSON.parse(config.ejectorPreferences)
            this._alliance = config.alliance
            this._station = config.station
        }
        this.updateScoringZones()
        this.updateProtectedZones()
        this.updateIntakeSensor()
    }

    public updateSimConfig(config: SimConfigData | undefined) {
        const robotPrefs = PreferencesSystem.getRobotPreferences(this.assemblyName)
        if (robotPrefs) {
            this._simConfigData = robotPrefs.simConfig = config
            PreferencesSystem.setRobotPreferences(this.assemblyName, robotPrefs)
            PreferencesSystem.savePreferences()
            ;(this._brain as WPILibBrain)?.loadSimConfig?.()
        }
    }

    public enablePhysics() {
        if (World.multiplayerSystem?.getOwnSceneObjectIDs().includes(this.id as LocalSceneObjectId)) {
            World.multiplayerSystem.broadcast({ type: "enableObjectPhysics", data: this.id as RemoteSceneObjectId })
        }

        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.physicsSystem.enablePhysicsForBody(this._mechanism.getBodyByNodeId(rn.id)!)
        })
        this._mechanism.ghostBodies.forEach(x => World.physicsSystem.enablePhysicsForBody(x))
    }

    public disablePhysics() {
        if (World.multiplayerSystem?.getOwnSceneObjectIDs().includes(this.id as LocalSceneObjectId)) {
            World.multiplayerSystem.broadcast({ type: "disableObjectPhysics", data: this.id as RemoteSceneObjectId })
        }

        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.physicsSystem.disablePhysicsForBody(this._mechanism.getBodyByNodeId(rn.id)!)
        })
        this._mechanism.ghostBodies.forEach(x => World.physicsSystem.disablePhysicsForBody(x))
    }

    public hasPhysics(): boolean {
        const rootBody = World.physicsSystem.getBody(this.getRootNodeId()!)
        return rootBody.IsActive() && !rootBody.IsSensor()
    }

    public getRootNodeId(): Jolt.BodyID | undefined {
        return this._mechanism.getBodyByNodeId(this._mechanism.rootBody)
    }

    public loadFocusTransform(mat: THREE.Matrix4) {
        const bounds = this.computeBoundingBox()
        const center = bounds.getCenter(new THREE.Vector3())
        mat.makeTranslation(center.x, center.y, center.z)
    }

    public getSupplierData(): ContextData {
        const data: ContextData = {
            title: this.miraType == MiraType.ROBOT ? "A Robot" : "A Field",
            items: [],
        }

        data.items.push(
            {
                name: "Move",
                customProps: {
                    configurationType: this.miraType === MiraType.ROBOT ? "ROBOTS" : "FIELDS",
                    configMode: ConfigMode.MOVE,
                    selectedAssembly: this,
                },
                screen: ConfigurePanel,
                type: "panel",
            },
            {
                name: "Configure",
                customProps: {
                    configurationType: this.miraType === MiraType.ROBOT ? "ROBOTS" : "FIELDS",
                    configMode: undefined,
                    selectedAssembly: this,
                },
                screen: ConfigurePanel,
                type: "panel",
            }
        )

        if (this.brain?.brainType == "wpilib") {
            data.items.push({
                name: "Auto Testing",
                screen: AutoTestPanel,
                type: "panel",
            })
        }

        if (World.sceneRenderer.currentCameraControls.controlsType == "Orbit") {
            const cameraControls = World.sceneRenderer.currentCameraControls as CustomOrbitControls
            if (cameraControls.focusProvider == this) {
                data.items.push({
                    name: "Camera: Unfocus",
                    func: () => {
                        cameraControls.unfocus()
                    },
                })

                if (cameraControls.locked) {
                    data.items.push({
                        name: "Camera: Unlock",
                        func: () => {
                            cameraControls.locked = false
                        },
                    })
                } else {
                    data.items.push({
                        name: "Camera: Lock",
                        func: () => {
                            cameraControls.locked = true
                        },
                    })
                }
            } else {
                data.items.push({
                    name: "Camera: Focus",
                    func: () => {
                        cameraControls.focusProvider = this
                    },
                })
            }
        }

        data.items.push({
            name: "Remove",
            func: () => {
                World.sceneRenderer.removeSceneObject(this.id)
            },
        })

        return data
    }

    public getUpdateData(): UpdateObjectData | undefined {
        const gamePiecesControlled: number[] = this.activeEjectables.map(bodyId => bodyId.GetIndexAndSequenceNumber())

        const bodies = this.getAllBodies()
            .map(body => {
                const linearVelocity = body.GetLinearVelocity()
                const angularVelocity = body.GetAngularVelocity()
                const position = body.GetPosition()
                const rotation = body.GetRotation()

                return {
                    bodyId: body.GetID().GetIndexAndSequenceNumber(),
                    linearVelocityStr: `{"x": ${linearVelocity.GetX()}, "y": ${linearVelocity.GetY()}, "z": ${linearVelocity.GetZ()}}`,
                    angularVelocityStr: `{"x": ${angularVelocity.GetX()}, "y": ${angularVelocity.GetY()}, "z": ${angularVelocity.GetZ()}}`,
                    positionStr: `{"x": ${position.GetX()}, "y": ${position.GetY()}, "z": ${position.GetZ()}}`,
                    rotationStr: `{"x": ${rotation.GetX()}, "y": ${rotation.GetY()}, "z": ${rotation.GetZ()}, "w": ${rotation.GetW()}}`,
                }
            })
            .filter(n => n != null)

        return {
            sceneObjectKey: this.id as RemoteSceneObjectId,
            gamePiecesControlled,
            bodies,
        }
    }
    public getAllBodyIds(): Jolt.BodyID[] {
        return [...this.mechanism.nodeToBody.values()]
    }

    public getAllBodies(): Jolt.Body[] {
        return [...this.mechanism.nodeToBody.values()]
            .map(bodyId => World.physicsSystem.getBody(bodyId))
            .filter(body => body != null)
    }

    private recordRobotCollision(collision: Jolt.BodyID) {
        const objectCollidedWith = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collision)
        if (objectCollidedWith && objectCollidedWith.isGamePiece) {
            objectCollidedWith.robotLastInContactWith = this
        }
    }
}

export async function createMirabuf(
    assembly: mirabuf.Assembly,
    progressHandle?: ProgressHandle,
    multiplayerOwnerId?: string
): Promise<MirabufSceneObject | null | undefined> {
    const parser = new MirabufParser(assembly, progressHandle)
    if (parser.maxErrorSeverity >= ParseErrorSeverity.UNIMPORTABLE) {
        console.error(`Assembly Parser produced significant errors for '${assembly.info!.name!}'`)
        return
    }

    return new MirabufSceneObject(new MirabufInstance(parser), assembly.info!.name!, progressHandle, multiplayerOwnerId)
}

/**
 * Body association to a rigid node with a given mirabuf scene object.
 */
export class RigidNodeAssociate extends BodyAssociate {
    public readonly sceneObject: MirabufSceneObject
    public robotLastInContactWith: MirabufSceneObject | null = null

    public readonly rigidNode: RigidNodeReadOnly

    public get rigidNodeId(): RigidNodeId {
        return this.rigidNode.id
    }

    public get isGamePiece(): boolean {
        return this.rigidNode.isGamePiece
    }

    public constructor(sceneObject: MirabufSceneObject, rigidNode: RigidNodeReadOnly, body: Jolt.BodyID) {
        super(body)
        this.sceneObject = sceneObject
        this.rigidNode = rigidNode
    }
}

export default MirabufSceneObject

export class MirabufObjectChangeEvent extends Event {
    private static _eventKey = "MirabufObjectChange"
    private _obj: MirabufSceneObject | null

    private constructor(obj: MirabufSceneObject | null) {
        super(MirabufObjectChangeEvent._eventKey)
        this._obj = obj
    }

    public static addEventListener(cb: (object: MirabufSceneObject | null) => void): () => void {
        const listener = (event: Event) => {
            if (event instanceof MirabufObjectChangeEvent) {
                cb(event._obj)
            } else {
                cb(null)
            }
        }
        window.addEventListener(this._eventKey, listener)
        return () => window.removeEventListener(this._eventKey, listener)
    }

    public static dispatch(obj: MirabufSceneObject | null) {
        window.dispatchEvent(new MirabufObjectChangeEvent(obj))
    }
}
