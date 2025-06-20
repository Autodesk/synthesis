import * as THREE from "three"
import WorldSystem from "../WorldSystem"
import World from "../World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { ThreeVector3_JoltVec3, JoltVec3_ThreeVector3 } from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import {
    InteractionStart,
    InteractionMove,
    InteractionEnd,
    PRIMARY_MOUSE_INTERACTION,
} from "./ScreenInteractionHandler"
import { CustomOrbitControls, SphericalCoords } from "./CameraControls"
import Jolt from "@azaleacolburn/jolt-physics"
import { MiraType } from "@/mirabuf/MirabufLoader"

interface DragTarget {
    bodyId: Jolt.BodyID
    initialPosition: THREE.Vector3
    offset: THREE.Vector3
    mass: number
    dragPlane: THREE.Plane
    dragDepth: number
    physicsDisabled: boolean
}

interface CameraTransition {
    isTransitioning: boolean
    transitionProgress: number
    transitionDuration: number
    startCoords: SphericalCoords
    targetCoords: SphericalCoords
    startFocus: THREE.Matrix4
    targetSceneObject: MirabufSceneObject | undefined
}

class DragModeSystem extends WorldSystem {
    private _enabled: boolean = false
    private _dragTarget: DragTarget | undefined
    private _isDragging: boolean = false
    private _lastMousePosition: [number, number] = [0, 0]

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _originalInteractionMove: ((i: InteractionMove) => void) | undefined
    private _originalInteractionEnd: ((i: InteractionEnd) => void) | undefined

    private _cameraTransition: CameraTransition = {
        isTransitioning: false,
        transitionProgress: 0,
        transitionDuration: 1.0,
        startCoords: { theta: 0, phi: 0, r: 0 },
        targetCoords: { theta: 0, phi: 0, r: 0 },
        startFocus: new THREE.Matrix4(),
        targetSceneObject: undefined,
    }

    private _handleDisableDragMode: () => void

    public constructor() {
        super()

        this._handleDisableDragMode = () => {
            this.enabled = false
        }

        window.addEventListener("disableDragMode", this._handleDisableDragMode)
    }

    public get enabled(): boolean {
        return this._enabled
    }

    public set enabled(enabled: boolean) {
        if (this._enabled === enabled) return

        this._enabled = enabled

        if (enabled) {
            this.hookInteractionHandlers()
        } else {
            this.unhookInteractionHandlers()
            this.stopDragging()

            if (this._cameraTransition.isTransitioning) {
                this._cameraTransition.isTransitioning = false
                World.SceneRenderer.currentCameraControls.enabled = true
            }
        }

        window.dispatchEvent(new CustomEvent("dragModeToggled", { detail: { enabled } }))
    }

    public Update(deltaT: number): void {
        if (!this._enabled) return

        if (this._isDragging && this._dragTarget) {
            this.updateDragForce()
        }

        if (this._cameraTransition.isTransitioning) {
            this.updateCameraTransition(deltaT)
        }
    }

    public Destroy(): void {
        this.enabled = false

        if (this._cameraTransition.isTransitioning) {
            this._cameraTransition.isTransitioning = false
            World.SceneRenderer.currentCameraControls.enabled = true
        }

        window.removeEventListener("disableDragMode", this._handleDisableDragMode)
    }

    private hookInteractionHandlers(): void {
        const handler = World.SceneRenderer.renderer.domElement.parentElement?.querySelector("canvas")
        if (!handler) return

        const screenHandler = World.SceneRenderer.screenInteractionHandler
        this._originalInteractionStart = screenHandler.interactionStart
        this._originalInteractionMove = screenHandler.interactionMove
        this._originalInteractionEnd = screenHandler.interactionEnd

        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)
        screenHandler.interactionMove = (interaction: InteractionMove) => this.onInteractionMove(interaction)
        screenHandler.interactionEnd = (interaction: InteractionEnd) => this.onInteractionEnd(interaction)
    }

    private unhookInteractionHandlers(): void {
        const screenHandler = World.SceneRenderer.screenInteractionHandler
        if (!screenHandler) return

        if (this._originalInteractionStart) screenHandler.interactionStart = this._originalInteractionStart
        if (this._originalInteractionMove) screenHandler.interactionMove = this._originalInteractionMove
        if (this._originalInteractionEnd) screenHandler.interactionEnd = this._originalInteractionEnd
    }

    private onInteractionStart(interaction: InteractionStart): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionStart?.(interaction)
            return
        }

        this._lastMousePosition = interaction.position

        const hitResult = this.raycastFromMouse(interaction.position)
        if (hitResult) {
            const association = World.PhysicsSystem.GetBodyAssociation(hitResult.data.mBodyID) as RigidNodeAssociate
            if (association?.sceneObject && association.sceneObject instanceof MirabufSceneObject) {
                const body = World.PhysicsSystem.GetBody(hitResult.data.mBodyID)
                if (body) {
                    const isStatic = body.GetMotionType() === JOLT.EMotionType_Static
                    const isFieldStructure =
                        association.sceneObject.miraType === MiraType.FIELD && !association.isGamePiece

                    if (!isStatic && !isFieldStructure) {
                        const hitPointVec = JoltVec3_ThreeVector3(hitResult.point)
                        this.startDragging(hitResult.data.mBodyID, interaction.position, hitPointVec)
                        return
                    }
                }
            }
        }

        this._originalInteractionStart?.(interaction)
    }

    private onInteractionMove(interaction: InteractionMove): void {
        if (this._isDragging && interaction.movement) {
            this._lastMousePosition[0] += interaction.movement[0]
            this._lastMousePosition[1] += interaction.movement[1]
        } else {
            this._originalInteractionMove?.(interaction)
        }
    }

    private onInteractionEnd(interaction: InteractionEnd): void {
        if (interaction.interactionType === PRIMARY_MOUSE_INTERACTION && this._isDragging) {
            this.stopDragging()
        } else {
            this._originalInteractionEnd?.(interaction)
        }
    }

    private raycastFromMouse(mousePos: [number, number]) {
        const camera = World.SceneRenderer.mainCamera
        const origin = camera.position
        const worldSpace = World.SceneRenderer.PixelToWorldSpace(mousePos[0], mousePos[1])
        const direction = worldSpace.sub(origin).normalize().multiplyScalar(40.0)

        return World.PhysicsSystem.RayCast(ThreeVector3_JoltVec3(origin), ThreeVector3_JoltVec3(direction))
    }

    private startDragging(bodyId: Jolt.BodyID, mousePos: [number, number], hitPoint: THREE.Vector3): void {
        const body = World.PhysicsSystem.GetBody(bodyId)
        if (!body) return

        const bodyPos = body.GetPosition()
        const bodyPosition = new THREE.Vector3(bodyPos.GetX(), bodyPos.GetY(), bodyPos.GetZ())

        const motionProperties = body.GetMotionProperties()
        const mass = 1.0 / motionProperties.GetInverseMass()

        const camera = World.SceneRenderer.mainCamera
        const cameraDirection = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
        const dragPlane = new THREE.Plane()
        dragPlane.setFromNormalAndCoplanarPoint(cameraDirection, hitPoint)

        const cameraToHit = hitPoint.clone().sub(camera.position)
        const dragDepth = cameraToHit.dot(cameraDirection)

        const association = World.PhysicsSystem.GetBodyAssociation(bodyId) as RigidNodeAssociate
        const isRobot = association?.sceneObject?.miraType === MiraType.ROBOT
        const isGamePiece = association?.isGamePiece

        this._dragTarget = {
            bodyId: bodyId,
            initialPosition: bodyPosition.clone(),
            offset: hitPoint.clone().sub(bodyPosition),
            mass: mass,
            dragPlane: dragPlane,
            dragDepth: dragDepth,
            physicsDisabled: isRobot,
        }

        this._isDragging = true
        this._lastMousePosition = mousePos

        if (isRobot) {
            World.PhysicsSystem.DisablePhysicsForBody(bodyId)
        }

        World.SceneRenderer.currentCameraControls.enabled = false
    }

    private stopDragging(): void {
        if (!this._isDragging) return

        if (this._dragTarget?.physicsDisabled) {
            World.PhysicsSystem.EnablePhysicsForBody(this._dragTarget.bodyId)
        } else if (this._dragTarget) {
            const body = World.PhysicsSystem.GetBody(this._dragTarget.bodyId)
            if (body) {
                const currentVel = body.GetLinearVelocity()
                const mass = this._dragTarget.mass
                const stopBrakingStrength = Math.min(mass * 10.0, 300.0)
                const stopBrakingForce = new JOLT.Vec3(
                    -currentVel.GetX() * stopBrakingStrength,
                    -currentVel.GetY() * stopBrakingStrength,
                    -currentVel.GetZ() * stopBrakingStrength
                )
                body.AddForce(stopBrakingForce)

                const angularVel = body.GetAngularVelocity()
                const angularStopBraking = Math.min(mass * 8.0, 200.0)
                const angularStopTorque = new JOLT.Vec3(
                    -angularVel.GetX() * angularStopBraking,
                    -angularVel.GetY() * angularStopBraking,
                    -angularVel.GetZ() * angularStopBraking
                )
                body.AddTorque(angularStopTorque)
            }
        }

        let targetSceneObject: MirabufSceneObject | undefined
        let shouldTransition = true

        if (this._dragTarget) {
            const association = World.PhysicsSystem.GetBodyAssociation(this._dragTarget.bodyId) as RigidNodeAssociate
            targetSceneObject = association?.sceneObject
            if (association?.isGamePiece) {
                shouldTransition = false
            }
        }

        this._isDragging = false
        this._dragTarget = undefined

        if (shouldTransition) {
            this.startCameraTransition(targetSceneObject)
        } else {
            World.SceneRenderer.currentCameraControls.enabled = true
        }
    }

    private startCameraTransition(targetSceneObject: MirabufSceneObject | undefined): void {
        const cameraControls = World.SceneRenderer.currentCameraControls as CustomOrbitControls

        this._cameraTransition.startCoords = {
            theta: cameraControls.coords.theta,
            phi: cameraControls.coords.phi,
            r: cameraControls.coords.r,
        }
        this._cameraTransition.startFocus.copy(cameraControls.focus)

        this._cameraTransition.targetCoords = {
            theta: this._cameraTransition.startCoords.theta,
            phi: this._cameraTransition.startCoords.phi,
            r: this._cameraTransition.startCoords.r,
        }

        this._cameraTransition.targetSceneObject = targetSceneObject

        this._cameraTransition.isTransitioning = true
        this._cameraTransition.transitionProgress = 0

        cameraControls.enabled = true
        cameraControls.focusProvider = undefined
    }

    private updateCameraTransition(deltaT: number): void {
        if (!this._cameraTransition.isTransitioning) return

        this._cameraTransition.transitionProgress += deltaT / this._cameraTransition.transitionDuration

        if (this._cameraTransition.transitionProgress >= 1.0) {
            this._cameraTransition.isTransitioning = false
            this._cameraTransition.transitionProgress = 1.0

            const cameraControls = World.SceneRenderer.currentCameraControls as CustomOrbitControls

            if (this._cameraTransition.targetSceneObject) {
                cameraControls.focusProvider = this._cameraTransition.targetSceneObject
            }
            return
        }

        const t = this.easeInOutCubic(this._cameraTransition.transitionProgress)

        const cameraControls = World.SceneRenderer.currentCameraControls as CustomOrbitControls

        const currentFocus = new THREE.Matrix4()
        if (this._cameraTransition.targetSceneObject) {
            const targetFocus = new THREE.Matrix4()
            this._cameraTransition.targetSceneObject.LoadFocusTransform(targetFocus)

            const startPos = new THREE.Vector3().setFromMatrixPosition(this._cameraTransition.startFocus)
            const targetPos = new THREE.Vector3().setFromMatrixPosition(targetFocus)
            const currentPos = new THREE.Vector3().lerpVectors(startPos, targetPos, t)

            currentFocus.makeTranslation(currentPos.x, currentPos.y, currentPos.z)
        } else {
            currentFocus.copy(this._cameraTransition.startFocus)
        }

        cameraControls.focus = currentFocus
    }

    private easeInOutCubic(t: number): number {
        return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2
    }

    private updateDragForce(): void {
        if (!this._dragTarget) return

        const body = World.PhysicsSystem.GetBody(this._dragTarget.bodyId)
        if (!body) {
            this.stopDragging()
            return
        }

        const currentPos = body.GetPosition()
        const currentPosition = new THREE.Vector3(currentPos.GetX(), currentPos.GetY(), currentPos.GetZ())

        const camera = World.SceneRenderer.mainCamera
        const mouseNDC = new THREE.Vector2(
            (this._lastMousePosition[0] / window.innerWidth) * 2 - 1,
            -(this._lastMousePosition[1] / window.innerHeight) * 2 + 1
        )

        const raycaster = new THREE.Raycaster()
        raycaster.setFromCamera(mouseNDC, camera)

        const intersectionPoint = new THREE.Vector3()
        const intersected = raycaster.ray.intersectPlane(this._dragTarget.dragPlane, intersectionPoint)

        if (!intersected) return

        const targetWorldPos = intersectionPoint.sub(this._dragTarget.offset)

        const displacement = targetWorldPos.sub(currentPosition)
        const distance = displacement.length()

        if (distance > 0.001) {
            const maxSpeed = 15.0
            const dampingZone = 0.5

            let targetSpeed: number
            if (distance > dampingZone) {
                targetSpeed = maxSpeed
            } else {
                targetSpeed = maxSpeed * (distance / dampingZone)
            }

            const direction = displacement.normalize()
            const desiredVelocity = direction.multiplyScalar(targetSpeed)

            const currentVel = body.GetLinearVelocity()
            const currentVelocity = new THREE.Vector3(currentVel.GetX(), currentVel.GetY(), currentVel.GetZ())

            const velocityError = desiredVelocity.sub(currentVelocity)

            const mass = this._dragTarget.mass

            const forceMultiplier = Math.min(mass * 30.0, 500.0)
            const forceNeeded = velocityError.multiplyScalar(forceMultiplier)

            const joltForce = ThreeVector3_JoltVec3(forceNeeded)
            body.AddForce(joltForce)
            console.log(forceNeeded)

            const angularVel = body.GetAngularVelocity()
            const angularDampingStrength = Math.min(mass * 3.0, 100.0)
            const angularDampingTorque = new JOLT.Vec3(
                -angularVel.GetX() * angularDampingStrength,
                -angularVel.GetY() * angularDampingStrength,
                -angularVel.GetZ() * angularDampingStrength
            )
            body.AddTorque(angularDampingTorque)
        } else {
            const currentVel = body.GetLinearVelocity()
            const mass = this._dragTarget.mass
            const brakingStrength = Math.min(mass * 5.0, 200.0)
            const brakingForce = new JOLT.Vec3(
                -currentVel.GetX() * brakingStrength,
                -currentVel.GetY() * brakingStrength,
                -currentVel.GetZ() * brakingStrength
            )

            body.AddForce(brakingForce)

            const angularVel = body.GetAngularVelocity()
            const angularBrakingStrength = Math.min(mass * 5.0, 150.0)
            const angularBrakingTorque = new JOLT.Vec3(
                -angularVel.GetX() * angularBrakingStrength,
                -angularVel.GetY() * angularBrakingStrength,
                -angularVel.GetZ() * angularBrakingStrength
            )
            body.AddTorque(angularBrakingTorque)
        }
    }
}

export default DragModeSystem
