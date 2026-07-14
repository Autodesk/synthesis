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
import EventSystem from "@/systems/EventSystem.ts"
import type Mechanism from "@/systems/physics/Mechanism"
import type { LayerReserve } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import {
    type Alliance,
    defaultFieldPreferences,
    defaultFieldSpawnLocation,
    defaultRobotPreferences,
    type EjectorPreferences,
    type FieldPreferences,
    type IntakePreferences,
    type ProtectedZonePreferences,
    type RobotPreferences,
    type ScoringZonePreferences,
    type SpawnLocation,
    type Station,
} from "@/systems/preferences/PreferenceTypes"
import {
    CameraMode,
    type CustomFieldViewControls,
    type CustomTargetControls,
    getTargetControls,
} from "@/systems/scene/CameraControls"
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
    convertJoltVec3ToThreeVector3,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import { createMeshForShape } from "@/util/threejs/MeshCreation.ts"
import SceneObject from "../systems/scene/SceneObject"
import EjectableSceneObject from "./EjectableSceneObject"
import FieldMiraEditor from "./FieldMiraEditor"
import IntakeSensorSceneObject from "./IntakeSensorSceneObject"
import MirabufInstance from "./MirabufInstance"
import MirabufCachingService, { MiraType } from "./MirabufLoader"
import MirabufParser, { ParseErrorSeverity, type RigidNodeId, type RigidNodeReadOnly } from "./MirabufParser"
import ProtectedZoneSceneObject from "./ProtectedZoneSceneObject"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"
import InputSystem from "@/systems/input/InputSystem.ts"
import { v4 as uuidV4 } from "uuid"
import { hexStringToUint8Array } from "@/util/Utility.ts"

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

type MinMax = { min: number; max: number }
type AxisVertices = {
    x: MinMax
    y: MinMax
    z: MinMax
}

class MirabufSceneObject extends SceneObject implements ContextSupplier {
    public readonly mirabufInstance: MirabufInstance
    public readonly mechanism: Mechanism

    private _brain: Brain | undefined
    public alliance: Alliance | undefined
    public station: Station | undefined

    private _debugBodies: Map<string, RnDebugMeshes> | null
    private _physicsLayerReserve: LayerReserve | undefined

    private _fieldPreferences: FieldPreferences | undefined
    private _robotPreferences: RobotPreferences | undefined

    private _ejectables: EjectableSceneObject[] = []
    private _intakeSensor?: IntakeSensorSceneObject
    private _scoringZones: ScoringZoneSceneObject[] = []
    private _protectedZones: ProtectedZoneSceneObject[] = []

    public nameOverride?: string
    private _nameTag: SceneOverlayTag | undefined
    private _centerOfMassIndicator: THREE.Mesh | undefined
    private _basePositionTransform: THREE.Vector3 | undefined
    public intakeActive = false
    public ejectorActive = false

    private multiplayerOwningClientId?: string

    private _lastEjectableToastTime = 0
    private static readonly EJECTABLE_TOAST_COOLDOWN_MS = 500

    private _collisionUnsubscriber?: () => void

    private _furthestVertices?: AxisVertices = undefined
    private _centerPositionOffsetFromRootNodeTransform?: Jolt.Vec3 = undefined

    public get scoringZones(): Readonly<ScoringZoneSceneObject[]> {
        return this._scoringZones
    }

    public get intakePreferences(): IntakePreferences {
        return this.robotPreferences.intake
    }

    public set intakePreferences(val: IntakePreferences) {
        this.robotPreferences.intake = val
    }

    public get robotPreferences(): RobotPreferences {
        this._robotPreferences ??= defaultRobotPreferences()
        return this._robotPreferences
    }

    public get ejectorPreferences(): EjectorPreferences {
        return this.robotPreferences.ejector
    }

    public set ejectorPreferences(val: EjectorPreferences) {
        this.robotPreferences.ejector = val
    }

    public get multiplayerOwnerName(): string | undefined {
        if (this.multiplayerOwningClientId == null) return undefined
        return World.multiplayerSystem?._clientToInfoMap?.get(this.multiplayerOwningClientId)?.displayName
    }

    get simConfigData() {
        return this._robotPreferences?.simConfig
    }

    get fieldPreferences() {
        return this._fieldPreferences
    }

    get nameTag() {
        return this._nameTag
    }

    get isOwnObject() {
        return this.multiplayerOwningClientId == undefined
    }

    public get activeEjectables(): Jolt.BodyID[] {
        return this._ejectables.map(e => e.gamePieceBodyId!).filter(x => x !== undefined)
    }

    public get miraType(): MiraType {
        return this.mirabufInstance.parser.assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD
    }

    public get rootNodeId(): string {
        return this.mirabufInstance.parser.rootNode
    }

    public get brain() {
        return this._brain
    }

    public set brain(brain: Brain | undefined) {
        this._brain = brain
        const simLayer = World.simulationSystem.getSimulationLayer(this.mechanism)!
        simLayer.setBrain(brain)
    }

    public get descriptiveName(): string {
        return `${this.miraType === MiraType.ROBOT ? `[${this.multiplayerOwnerName ?? InputSystem.brainIndexSchemeMap.get((this.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}] ` : ""}${this.assemblyName}`
    }

    public get assemblyName() {
        return this.mirabufInstance.parser.assembly.info?.name ?? "Unknown"
    }

    public get assemblyId() {
        return this.mirabufInstance.parser.assemblyId
    }

    public constructor(mirabufInstance: MirabufInstance, progressHandle?: ProgressHandle, multiplayerOwnerId?: string) {
        super()
        this.mirabufInstance = mirabufInstance
        this.multiplayerOwningClientId = multiplayerOwnerId
        this.loadPreferences()

        progressHandle?.update("Creating mechanism...", 0.9)

        this.mechanism = World.physicsSystem.createMechanismFromParser(this.mirabufInstance.parser)
        if (this.mechanism.layerReserve) this._physicsLayerReserve = this.mechanism.layerReserve

        this._debugBodies = null

        if (this.miraType === MiraType.ROBOT) {
            // creating nametag for robots
            this._nameTag = new SceneOverlayTag(() => {
                const name =
                    this.nameOverride ??
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
            this._collisionUnsubscriber = EventSystem.listen("OnContactAddedEvent", data => {
                const { body1, body2 } = data

                if (body1.GetIndexAndSequenceNumber() === this.getRootNodeId()?.GetIndexAndSequenceNumber()) {
                    this.recordRobotCollision(body2)
                } else if (body2.GetIndexAndSequenceNumber() === this.getRootNodeId()?.GetIndexAndSequenceNumber()) {
                    this.recordRobotCollision(body1)
                }
            })

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
        this.mirabufInstance.addToScene(World.sceneRenderer.scene)

        if (DEBUG_BODIES) {
            this._debugBodies = new Map()
            this.mechanism.nodeToBody.forEach((bodyId, rnName) => {
                const body = World.physicsSystem.getBody(bodyId)!

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

        const rigidNodes = this.mirabufInstance.parser.rigidNodes
        this.mechanism.nodeToBody.forEach((bodyId, rigidNodeId) => {
            const rigidNode = rigidNodes.get(rigidNodeId)
            if (!rigidNode) {
                console.warn("Found a RigidNodeId with no related RigidNode. Skipping for now...")
                return
            }
            World.physicsSystem.setBodyAssociation(new RigidNodeAssociate(this, rigidNode, bodyId))
        })

        // Simulation
        if (this.miraType === MiraType.ROBOT) {
            World.simulationSystem.registerMechanism(this.mechanism)
            const simLayer = World.simulationSystem.getSimulationLayer(this.mechanism)!

            this._brain = new SynthesisBrain(this)
            simLayer.setBrain(this._brain)
        }

        if (this.isOwnObject) {
            setSpotlightAssembly(this)
        }

        this.updateBatches()

        this._basePositionTransform = this.getXZPositionTransform()

        this.moveToSpawnLocation()

        this.updateIntakeSensor()
        this.updateScoringZones()
        this.updateProtectedZones()

        const targetControls = getTargetControls()
        if (targetControls && this.isOwnObject && (this.miraType === MiraType.ROBOT || !targetControls.focusProvider)) {
            targetControls.focusProvider = this
        }

        EventSystem.dispatch("MirabufObjectChangeEvent", this)
    }

    // Centered in x-z plane, bottom surface of object
    public getXZPositionTransform(vec: THREE.Vector3 = new THREE.Vector3()): THREE.Vector3 {
        const box = this.computeBoundingBox()

        const transform = box.getCenter(vec)
        transform.setY(box.min.y)

        return transform
    }

    public getPositionTransform(vec: THREE.Vector3 = new THREE.Vector3()): THREE.Vector3 {
        const box = this.computeBoundingBox()
        return box.getCenter(vec)
    }

    public moveToSpawnLocation() {
        const referencePos = new THREE.Vector3()
        const pos =
            this.miraType == MiraType.FIELD
                ? defaultFieldSpawnLocation()
                : (this.robotSpawnPosition(referencePos) ?? defaultFieldSpawnLocation())

        this.setObjectPosition(pos)
    }

    private robotSpawnPosition(referencePos: THREE.Vector3): SpawnLocation | undefined {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        const fieldLocations = field?.fieldPreferences?.spawnLocations

        const pos =
            this.alliance != null && this.station != null && fieldLocations != null
                ? fieldLocations[this.alliance][this.station]
                : fieldLocations?.default

        // TODO
        // Why are we calling this?
        field?.getXZPositionTransform(referencePos)

        return pos
    }

    private setObjectPosition(initialPos: SpawnLocation, referencePosition: THREE.Vector3 = new THREE.Vector3()) {
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
        const initialTranslation = new JOLT.RVec3(
            initialPos.pos[0] - rotatedBasePositionTransform.x + referencePosition.x,
            initialPos.pos[1] - rotatedBasePositionTransform.y + referencePosition.y,
            initialPos.pos[2] - rotatedBasePositionTransform.z + referencePosition.z
        )

        const yUnitVec = new JOLT.Vec3(0, 1, 0)
        const initialRotation = JOLT.Quat.prototype.sRotation(yUnitVec, initialPos.yaw)

        const blankVec = new JOLT.Vec3()
        this.mirabufInstance.parser.rigidNodes.forEach(rn => {
            const jBodyId = this.mechanism.getBodyByNodeId(rn.id)
            if (!jBodyId) return

            const position = World.physicsSystem.getBody(jBodyId)!.GetPosition()
            const offset = convertJoltRVec3ToJoltVec3(position.Sub(bodyCenter))

            World.physicsSystem.setBodyPositionRotationAndVelocity(
                jBodyId,
                initialTranslation,
                initialRotation,
                blankVec,
                blankVec,
                false
            )

            JOLT.destroy(position)
            JOLT.destroy(offset)
        })

        this.updateMeshTransforms()

        JOLT.destroy(bodyCenter)
        JOLT.destroy(initialTranslation)
        JOLT.destroy(initialRotation)
        JOLT.destroy(yUnitVec)
        JOLT.destroy(blankVec)
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
        this.mirabufInstance.dispose(World.sceneRenderer.scene)

        if (this._brain && this._brain instanceof SynthesisBrain) {
            this._brain.clearControls()
        }

        this._debugBodies?.forEach(x => {
            World.sceneRenderer.scene.remove(x.colliderMesh, x.comMesh)
            x.colliderMesh.geometry.dispose()
            x.comMesh.geometry.dispose()

            ;(x.colliderMesh.material as THREE.Material).dispose()
            ;(x.comMesh.material as THREE.Material).dispose()
        })
        this._debugBodies?.clear()

        this._physicsLayerReserve?.release()
        this._ejectables.forEach(e => World.sceneRenderer.removeSceneObject(e.id))

        if (this._intakeSensor) {
            World.sceneRenderer.removeSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        this._scoringZones.forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._scoringZones.length = 0

        this._protectedZones.forEach(zone => World.sceneRenderer.removeSceneObject(zone.id))
        this._protectedZones.length = 0

        this.mechanism.nodeToBody.forEach(bodyId => {
            World.physicsSystem.removeBodyAssociation(bodyId)
        })

        this._nameTag?.dispose()

        World.simulationSystem.unregisterMechanism(this.mechanism)
        World.physicsSystem.destroyMechanism(this.mechanism)

        this._collisionUnsubscriber?.()

        if (this._centerOfMassIndicator) {
            World.sceneRenderer.scene.remove(this._centerOfMassIndicator)
            this._centerOfMassIndicator = undefined
        }

        EventSystem.dispatch("MirabufObjectChangeEvent", null)
    }

    public eject() {
        if (this._ejectables.length === 0) return

        const order = this.ejectorPreferences?.ejectOrder
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

        // If this.dispose() has been ran then return
        if (this.mirabufInstance.meshes.size) {
            this.mirabufInstance.parser.rigidNodes.forEach(rn => {
                const bodyId = this.mechanism.getBodyByNodeId(rn.id)!
                const body = World.physicsSystem.getBody(bodyId)
                if (!body) return

                const transform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())
                this.updateNodeParts(rn, transform)

                const position = body.GetPosition()
                if (Number.isNaN(position.GetX())) {
                    const vel = body.GetLinearVelocity()
                    console.warn(
                        `Invalid Position.\nPosition => ${position.GetX()}, ${position.GetY()}, ${position.GetZ()}\nVelocity => ${vel.GetX()}, ${vel.GetY()}, ${vel.GetZ()}`
                    )

                    JOLT.destroy(vel)
                    JOLT.destroy(position)
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
                        const oldWeighedCOM = weightedCOM

                        const mass = 1 / inverseMass
                        const com = body.GetCenterOfMassPosition().Mul(mass)

                        weightedCOM = weightedCOM.AddRVec3(com)
                        totalMass += mass

                        JOLT.destroy(oldWeighedCOM)
                        JOLT.destroy(com)
                    }
                }
            })
        }

        if (this._centerOfMassIndicator) {
            const setPositionAndVisibility = (netCoM: Jolt.RVec3) => {
                this._centerOfMassIndicator!.position.set(netCoM.GetX(), netCoM.GetY(), netCoM.GetZ())
                this._centerOfMassIndicator!.visible = PreferencesSystem.getUserPreference("ShowCenterOfMassIndicators")
            }

            const com = totalMass > 0 ? weightedCOM.Div(totalMass) : weightedCOM
            setPositionAndVisibility(com)
        }

        JOLT.destroy(weightedCOM)
    }

    public updateNodeParts(rn: RigidNodeReadOnly, transform: THREE.Matrix4) {
        rn.parts.forEach(part => {
            const partTransform = this.mirabufInstance.parser.globalTransforms.get(part)!.clone().premultiply(transform)
            const meshes = this.mirabufInstance.meshes.get(part) ?? []
            meshes.forEach(([batch, id]) => batch.setMatrixAt(id, partTransform))

            // JOLT.destroy(partTransform)
        })
    }

    /** Updates the batch computations */
    private updateBatches() {
        this.mirabufInstance.batches.forEach(x => {
            x.computeBoundingBox()
            x.computeBoundingSphere()
        })
    }

    /** Updates the position of the nametag relative to the robots position */
    private updateNameTag() {
        if (!this._nameTag || !PreferencesSystem.getUserPreference("RenderSceneTags")) return

        this._nameTag.color = this.alliance
        const boundingBox = this.computeBoundingBox()

        const rootNodeId = this.getRootNodeId()
        if (rootNodeId) {
            const body = World.physicsSystem.getBody(rootNodeId)
            if (body) {
                const centerOfMass = body.GetCenterOfMassPosition()
                this._nameTag.position = World.sceneRenderer.worldToPixelSpace(
                    new THREE.Vector3(centerOfMass.GetX(), boundingBox.max.y + 0.1, centerOfMass.GetZ())
                )
            }
        }
    }

    /*
     * I think it's fine that we create a new `IntakeSensorSceneObject`
     *  since this function only gets called occasionally by user input (and on `setup`), rather than in a loop.
     */
    public updateIntakeSensor() {
        if (this._intakeSensor) {
            World.sceneRenderer.removeSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        // Do we have an intake, and is it something other than the default. Config will default to root node at least.
        if (this.intakePreferences && this.intakePreferences.parentNode) {
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

        if (!this.ejectorPreferences.parentNode) {
            const now = Date.now()
            if (
                (!World.multiplayerSystem || World.multiplayerSystem?.getOwnRobots().includes(this)) &&
                now - this._lastEjectableToastTime > MirabufSceneObject.EJECTABLE_TOAST_COOLDOWN_MS
            ) {
                console.warn(`Configure an ejector first.`)
                globalAddToast("info", "Configure Ejector", "Configure an ejector first.")
                this._lastEjectableToastTime = now
            }

            return false
        }

        // 2) don’t exceed your configured maxPieces
        const max = this.intakePreferences?.maxPieces ?? 1
        if (this._ejectables.length >= max) return false

        // 3) avoid duplicates
        const key = bodyId.GetIndexAndSequenceNumber()
        if (this._ejectables.some(e => e.gamePieceBodyId!.GetIndexAndSequenceNumber() === key)) return false

        const ejectable = new EjectableSceneObject(this, bodyId)
        this._ejectables.push(ejectable)
        World.sceneRenderer.registerSceneObject(ejectable)

        return true
    }

    public updateScoringZones() {
        this.removeSceneObjects(this._scoringZones)

        if (!this._fieldPreferences || !this._fieldPreferences.scoringZones) return

        for (let i = 0; i < this._fieldPreferences.scoringZones.length; i++) {
            const newZone = new ScoringZoneSceneObject(this, i)
            this._scoringZones.push(newZone)
            World.sceneRenderer.registerSceneObject(newZone)
        }
    }

    public updateProtectedZones() {
        this.removeSceneObjects(this._protectedZones)

        if (!this._fieldPreferences || !this._fieldPreferences.protectedZones) return

        for (let i = 0; i < this._fieldPreferences.protectedZones.length; i++) {
            const newZone = new ProtectedZoneSceneObject(this, i)

            this._protectedZones.push(newZone)
            World.sceneRenderer.registerSceneObject(newZone)
        }
    }

    private removeSceneObjects(objs: SceneObject[]) {
        objs.filter(obj => obj.id != -1).forEach(obj => World.sceneRenderer.removeSceneObject(obj.id))
        objs.length = 0
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
        this.mirabufInstance.batches.forEach(batch => {
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
     * @returns The transformation matrix which corresponds to the reverse of the current spacial rotation of the root node of this scene object, relative to the
     */
    private getInverseRotationOfBody(): Jolt.Mat44 {
        const rootBody = World.physicsSystem.getBody(this.getRootNodeId()!)!
        return rootBody.GetWorldTransform().GetRotation().Inversed()
    }

    /**
     * Computes the six furthest vertices along the x, y, and z axes respectively. Stores its result in `this._furthestVertices`
     *
     * `this._furthestVertices` is guaranteed to be defined after calling this function
     *
     * The vertices calculated by this function should remain valid as the robot moves through the world
     * However, if the robot modifies its dimensionality in some way(e.g. by extending an arm), this function should be called again to have accurate results.
     */
    private computeFurthestVertices(): void {
        this._furthestVertices = {
            x: { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
            y: { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
            z: { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
        }

        const inverseRotation = this.getInverseRotationOfBody()

        const biggest = JOLT.AABox.prototype.sBiggest()
        const scale = new JOLT.Vec3(1, 1, 1)
        const identity = JOLT.Quat.prototype.sIdentity()

        this.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
            const bodyId = this.mechanism.getBodyByNodeId(rigidNode.id)
            if (!bodyId) return

            const body = World.physicsSystem.getBody(bodyId)!
            const bodyTransform = body.GetWorldTransform()

            const vertexTransform = bodyTransform.MulMat44(inverseRotation)

            const shape = body.GetShape()
            const triangleContext = new JOLT.ShapeGetTriangles(shape, biggest, shape.GetCenterOfMass(), identity, scale)
            const vertices = new Float32Array(
                JOLT.HEAP32.buffer,
                triangleContext.GetVerticesData(),
                triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT
            )

            for (let i = 0; i < vertices.length; i += 3) {
                // Transform the vertex into the position it would occupy if the robot were axis aligned
                const vertex = new JOLT.Vec3(vertices[i], vertices[i + 1], vertices[i + 2])
                const transformedVertex = vertexTransform.MulVec3(vertex)

                const transX = transformedVertex.GetX()
                const transY = transformedVertex.GetY()
                const transZ = transformedVertex.GetZ()

                // Compute maximum vertex along each axis
                const oldX = this._furthestVertices!.x
                const oldY = this._furthestVertices!.y
                const oldZ = this._furthestVertices!.z

                oldX.min = Math.min(oldX.min, transX)
                oldY.min = Math.min(oldY.min, transY)
                oldZ.min = Math.min(oldZ.min, transZ)

                oldX.max = Math.max(oldX.max, transX)
                oldY.max = Math.max(oldY.max, transY)
                oldZ.max = Math.max(oldZ.max, transZ)

                JOLT.destroy(vertex)
                JOLT.destroy(transformedVertex)
            }

            JOLT.destroy(vertexTransform)
            JOLT.destroy(triangleContext)
        })

        JOLT.destroy(inverseRotation)

        JOLT.destroy(scale)
        JOLT.destroy(biggest)
        JOLT.destroy(identity)

        const mins = [this._furthestVertices.x.min, this._furthestVertices.y.min, this._furthestVertices.z.min]
        const maxes = [this._furthestVertices.x.max, this._furthestVertices.y.max, this._furthestVertices.z.max]
        if (mins.some(m => m === Number.POSITIVE_INFINITY) || maxes.some(m => m === Number.NEGATIVE_INFINITY)) {
            console.warn("Failed to compute furthest vertices")
        }
    }

    /**
     * Gets the tightest fitting oriented bounding box around the robot, centered at the robot's origin.
     *
     * @returns The aforementioned bounding box
     */
    public getOrientedBoundingBox(): Jolt.OrientedBox {
        if (!this._furthestVertices) this.computeFurthestVertices()

        // Get dimensions of scene object along each axis
        const axesVertices = [this._furthestVertices!.x, this._furthestVertices!.y, this._furthestVertices!.z]
        const axisHalfExtents = axesVertices.map(({ min, max }) => Math.abs(max - min) / 2) as [number, number, number]

        const halfExtent = new JOLT.Vec3(...axisHalfExtents)

        // Get root body transformation
        const rootBody = World.physicsSystem.getBody(this.getRootNodeId()!)!
        // NOTE Do not destroy
        const rotation = rootBody.GetRotation()

        // Here, we calculate the vector between the center of the robot when axis-aligned (which is should be initially) and the root node transform
        // WARNING This requires the robot to be axis-aligned initially. This may not always be true.
        if (!this._centerPositionOffsetFromRootNodeTransform) {
            const rootNodeTransform = convertJoltRVec3ToJoltVec3(rootBody.GetPosition())
            const alignedPosition = convertThreeVector3ToJoltVec3(this.getPositionTransform())

            this._centerPositionOffsetFromRootNodeTransform = alignedPosition.SubVec3(rootNodeTransform)
        }

        // Then, we rotate our vector by the rotation of the root body, otherwise any rotation will mess with the translation
        const offset = rotation.MulVec3(this._centerPositionOffsetFromRootNodeTransform)

        // Finally, we just offset the root node to get the true center
        const position = convertJoltRVec3ToJoltVec3(rootBody.GetPosition().Add(offset))
        const transform = JOLT.Mat44.prototype.sRotationTranslation(rotation, position)

        const orientedBoundingBox = new JOLT.OrientedBox(transform, halfExtent)

        JOLT.destroy(transform)
        JOLT.destroy(halfExtent)

        return orientedBoundingBox
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

        const rootBody = World.physicsSystem.getBody(rootNodeId)!
        const rootTransform = convertJoltMat44ToThreeMatrix4(rootBody.GetWorldTransform())

        const rootPosition = new THREE.Vector3()
        const rootRotation = new THREE.Quaternion()
        const rootScale = new THREE.Vector3()
        rootTransform.decompose(rootPosition, rootRotation, rootScale)

        // Create inverse rotation matrix to "undo" the robot's rotation
        const inverseRotation = new THREE.Matrix4().makeRotationFromQuaternion(rootRotation.clone().invert())

        const unrotatedBox = new THREE.Box3()

        this.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
            const bodyId = this.mechanism.getBodyByNodeId(rigidNode.id)
            if (!bodyId) return

            const body = World.physicsSystem.getBody(bodyId)!
            const bodyTransform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())

            const shape = body.GetShape()
            const scale = new JOLT.Vec3(1, 1, 1)
            const biggest = JOLT.AABox.prototype.sBiggest()

            const identity = JOLT.Quat.prototype.sIdentity()
            const triangleContext = new JOLT.ShapeGetTriangles(shape, biggest, shape.GetCenterOfMass(), identity, scale)

            try {
                const vertices = new Float32Array(
                    // I don't think anything needs to be freed here
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
                JOLT.destroy(biggest)
                JOLT.destroy(identity)
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

        const jBody = World.physicsSystem.getBody(jRootId)!
        if (jBody.IsStatic()) {
            const aaBox = jBody.GetWorldSpaceBounds()
            const mat = new THREE.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
            const centerVec = convertJoltVec3ToThreeVector3(aaBox.mMin.Add(aaBox.mMax).Div(2.0))

            mat.compose(centerVec, new THREE.Quaternion(0, 0, 0, 1), new THREE.Vector3(1, 1, 1))
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

    public resetPreferences(): void {
        this._fieldPreferences = defaultFieldPreferences()
        this._robotPreferences = defaultRobotPreferences()
        const parts = this.mirabufInstance.parser.assembly.data?.parts
        if (parts) {
            const editor = new FieldMiraEditor(parts)
            editor.migrateDevtoolFieldData(this._fieldPreferences)
            editor.migrateDevtoolRobotData(this._robotPreferences)
            this._fieldPreferences = { ...this._fieldPreferences, ...editor.getUserData("synthesis:field_preferences") }
            this._robotPreferences = { ...this._robotPreferences, ...editor.getUserData("synthesis:robot_preferences") }
        }
        this.savePreferences()
        this.updateScoringZones()
        this.updateIntakeSensor()
        this.updateProtectedZones()
    }

    public savePreferences(): void {
        if (this.miraType == MiraType.FIELD && this._fieldPreferences) {
            PreferencesSystem.setFieldPreferences(this.assemblyId, this._fieldPreferences)
        } else if (this._robotPreferences) {
            PreferencesSystem.setRobotPreferences(this.assemblyId, this._robotPreferences)
        }
        PreferencesSystem.savePreferences()
        setTimeout(() => this.sendPreferences())
    }

    public loadPreferences(checkMira: boolean = true): void {
        const parts = this.mirabufInstance.parser.assembly.data?.parts

        if (parts && checkMira) {
            const editor = new FieldMiraEditor(parts)
            if (this.miraType === MiraType.FIELD && !PreferencesSystem.hasFieldPreferences(this.assemblyId)) {
                this._fieldPreferences = defaultFieldPreferences()
                editor.migrateDevtoolFieldData(this._fieldPreferences)
                this._fieldPreferences = {
                    ...this._fieldPreferences,
                    ...editor.getUserData("synthesis:field_preferences"),
                }
            } else if (this.miraType === MiraType.ROBOT && !PreferencesSystem.hasRobotPreferences(this.assemblyId)) {
                this._robotPreferences = defaultRobotPreferences()
                editor.migrateDevtoolRobotData(this._robotPreferences)
                this._robotPreferences = {
                    ...this._robotPreferences,
                    ...editor.getUserData("synthesis:robot_preferences"),
                }
            }
            this.savePreferences()
        }
        this._fieldPreferences = PreferencesSystem.getFieldPreferences(this.assemblyId)
        this._robotPreferences = PreferencesSystem.getRobotPreferences(this.assemblyId)
        setTimeout(() => this.sendPreferences())
    }

    public savePreferencesToMirabuf(): void {
        const parts = this.mirabufInstance.parser.assembly.data?.parts
        if (parts) {
            const editor = new FieldMiraEditor(parts)
            if (this.miraType === MiraType.FIELD) {
                if (this._fieldPreferences !== undefined) {
                    editor.setUserData("synthesis:field_preferences", this._fieldPreferences)
                }
            } else {
                if (this._robotPreferences !== undefined) {
                    editor.setUserData("synthesis:robot_preferences", this._robotPreferences)
                }
            }
        }
    }

    public getPreferenceData(): FieldConfiguration | RobotConfiguration {
        return this.miraType == MiraType.FIELD
            ? {
                  fieldPreferences: JSON.stringify(this._fieldPreferences),
              }
            : {
                  intakePreferences: JSON.stringify(this.intakePreferences),
                  ejectorPreferences: JSON.stringify(this.ejectorPreferences),
                  alliance: this.alliance,
                  station: this.station,
              }
    }

    public setPreferenceData(preferences: FieldConfiguration | RobotConfiguration) {
        if (this.miraType === MiraType.FIELD) {
            const config = preferences as FieldConfiguration
            this._fieldPreferences = JSON.parse(config.fieldPreferences)
        } else {
            const config = preferences as RobotConfiguration
            this.intakePreferences = JSON.parse(config.intakePreferences)
            this.ejectorPreferences = JSON.parse(config.ejectorPreferences)
            this.alliance = config.alliance
            this.station = config.station
        }
        this.updateScoringZones()
        this.updateProtectedZones()
        this.updateIntakeSensor()
    }

    public updateSimConfig(config: SimConfigData | undefined) {
        this.robotPreferences.simConfig = config
        this.savePreferences()
        ;(this._brain as WPILibBrain)?.loadSimConfig?.()
    }

    public enablePhysics() {
        if (World.multiplayerSystem?.getOwnSceneObjectIDs().includes(this.id as LocalSceneObjectId)) {
            World.multiplayerSystem.broadcast({ type: "enableObjectPhysics", data: this.id as RemoteSceneObjectId })
        }

        this.mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.physicsSystem.enablePhysicsForBody(this.mechanism.getBodyByNodeId(rn.id)!)
        })
        this.mechanism.ghostBodies.forEach(x => World.physicsSystem.enablePhysicsForBody(x))
    }

    public disablePhysics() {
        if (World.multiplayerSystem?.getOwnSceneObjectIDs().includes(this.id as LocalSceneObjectId)) {
            World.multiplayerSystem.broadcast({ type: "disableObjectPhysics", data: this.id as RemoteSceneObjectId })
        }

        this.mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.physicsSystem.disablePhysicsForBody(this.mechanism.getBodyByNodeId(rn.id)!)
        })
        this.mechanism.ghostBodies.forEach(x => World.physicsSystem.disablePhysicsForBody(x))
    }

    public hasPhysics(): boolean {
        const rootBody = World.physicsSystem.getBody(this.getRootNodeId()!)!
        return rootBody.IsActive() && !rootBody.IsSensor()
    }

    public getRootNodeId(): Jolt.BodyID | undefined {
        return this.mechanism.getBodyByNodeId(this.mechanism.rootBody)!
    }

    public loadFocusTransform(mat: THREE.Matrix4) {
        const bounds = this.computeBoundingBox()
        let center = bounds.getCenter(new THREE.Vector3())
        const rotation = new THREE.Quaternion()

        const rootNodeId = this.getRootNodeId()
        if (rootNodeId) {
            const rootBody = World.physicsSystem.getBody(rootNodeId)
            if (rootBody) {
                const rootTransform = convertJoltMat44ToThreeMatrix4(rootBody.GetWorldTransform())
                rootTransform.decompose(new THREE.Vector3(), rotation, new THREE.Vector3())

                // Prioritize center of mass for smooth rotations
                if (!rootBody.IsStatic()) {
                    const pos = rootBody.GetCenterOfMassPosition()
                    center = new THREE.Vector3(pos.GetX(), pos.GetY(), pos.GetZ())
                }
            }
        }

        mat.makeRotationFromQuaternion(rotation)
        mat.setPosition(center)
    }

    private addRobotCameraMenuItems(data: ContextData, cameraControls: CustomTargetControls) {
        const modes = [
            { mode: CameraMode.Follow, name: "Camera: Follow Robot" },
            { mode: CameraMode.Locked, name: "Camera: Lock to Robot" },
            { mode: CameraMode.Face, name: "Camera: Face Robot" },
        ]

        modes
            .filter(({ mode }) => cameraControls.mode !== mode)
            .forEach(({ name, mode }) => {
                data.items.push({
                    name,
                    func: () => {
                        cameraControls.mode = mode
                    },
                })
            })
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

        if (World.sceneRenderer.currentCameraControls.controlsType == "Target") {
            const cameraControls = World.sceneRenderer.currentCameraControls as CustomTargetControls
            if (cameraControls.focusProvider == this) {
                data.items.push({
                    name: "Camera: Unfocus",
                    func: () => {
                        cameraControls.unfocus()
                    },
                })

                if (this.miraType === MiraType.ROBOT) {
                    this.addRobotCameraMenuItems(data, cameraControls)
                }
            } else {
                data.items.push({
                    name: "Camera: Focus",
                    func: () => {
                        cameraControls.focusProvider = this
                    },
                })
            }
        } else if (
            World.sceneRenderer.currentCameraControls.controlsType == "FieldView" &&
            this.miraType === MiraType.ROBOT
        ) {
            const fieldViewControls = World.sceneRenderer.currentCameraControls as CustomFieldViewControls
            if (fieldViewControls.focusedRobot === this) {
                data.items.push({
                    name: "Field Camera: Unfocus Robot",
                    func: () => {
                        fieldViewControls.focusRobot(undefined)
                    },
                })
            } else {
                data.items.push({
                    name: "Field Camera: Focus Robot",
                    func: () => {
                        fieldViewControls.focusRobot(this)
                    },
                })
            }

            data.items.push({
                name: "Robot Camera: Focus",
                func: () => {
                    World.sceneRenderer.setCameraControls("Target")
                    const targetControls = World.sceneRenderer.currentCameraControls as CustomTargetControls
                    targetControls.focusProvider = this
                },
            })
        }

        if ((this.brain as SynthesisBrain | undefined)?.driveType === DriveType.SWERVE) {
            data.items.push({
                name: "Reset Orientation",
                func: () => {
                    ;(this.brain as SynthesisBrain).resetSwerveOrientation()
                },
            })
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
            .map(bodyId => World.physicsSystem.getBody(bodyId)!)
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
    hash: string,
    assembly: mirabuf.Assembly,
    progressHandle?: ProgressHandle,
    multiplayerOwnerId?: string
): Promise<MirabufSceneObject | undefined> {
    const parser = new MirabufParser(assembly, progressHandle)

    if (!parser.assembly.info?.GUID?.match(/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/)) {
        await migrateUUID(parser, hash)
    }
    if (parser.maxErrorSeverity >= ParseErrorSeverity.UNIMPORTABLE) {
        console.error(`Assembly Parser produced significant errors for '${assembly.info!.name!}'`)
        return
    }

    return new MirabufSceneObject(new MirabufInstance(parser), progressHandle, multiplayerOwnerId)
}

async function migrateUUID(parser: MirabufParser, hash: string) {
    parser.assembly.info ??= {}
    const newGUID = uuidV4({ random: hexStringToUint8Array(hash).slice(0, 16) }) // using deterministic random to prevent the same model from being assigned different uuids after being imported multiple times. Once initially set, uuid will be persistent across hash changes
    console.warn("Migrating UUID", parser.assembly.info.GUID, "->", newGUID)
    parser.assembly.info.GUID = newGUID

    if ((await MirabufCachingService.get(hash)) != null) {
        await MirabufCachingService.remove(hash)
    }

    const cacheInfo = await MirabufCachingService.storeAssemblyInCache(parser.assembly, {
        miraType: parser.assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD,
        name: parser.assembly.info?.name ?? "Unknown",
    })

    if (cacheInfo == null) {
        globalAddToast("warning", "Migration Error", "Importing failed to save")
    }
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
