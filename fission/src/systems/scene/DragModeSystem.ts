import type Jolt from "@synthesis.adsk/jolt-physics"
import { type Message } from "@/systems/multiplayer/MultiplayerTypes"
import * as THREE from "three"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import InputSystem from "@/systems/input/InputSystem.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import { rayCastForRigidBody } from "@/util/RaycastUtils"
import { convertThreeVector3ToJoltVec3 } from "@/util/TypeConversions"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { CameraMode, getTargetControls } from "./CameraControls"
import {
    type InteractionEnd,
    type InteractionMove,
    type InteractionStart,
    PRIMARY_MOUSE_INTERACTION,
} from "./ScreenInteractionHandler"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { SceneObjectId } from "./SceneRenderer"

interface DragTarget {
    bodyId: Jolt.BodyID
    initialPosition: THREE.Vector3
    localOffset: THREE.Vector3 // Offset in body's local coordinate system
    mass: number
    dragDepth: number
    physicsDisabled: boolean
    isGamePiece: boolean
    rn: RigidNodeId
    sceneObjectId: SceneObjectId
}

class DragModeSystem extends WorldSystem {
    // Drag force constants - tune these to reduce wobble and improve stability
    private static readonly DRAG_FORCE_CONSTANTS = {
        // Linear motion control
        MAX_DRAG_SPEED: 15.0, // Maximum speed when dragging (lower = more stable, higher = more responsive)
        DAMPING_ZONE: 2, // Distance where speed starts to ramp down (larger = smoother approach)
        FORCE_MULTIPLIER_BASE: 15.0, // Base force multiplier per unit mass (lower = less aggressive)
        FORCE_MULTIPLIER_MAX: 500.0, // Maximum force regardless of mass (lower = more stable)

        // Angular damping control
        ANGULAR_DAMPING_BASE: 3.0, // Base angular damping per unit mass (higher = less rotation wobble)
        ANGULAR_DAMPING_MAX: 500.0, // Maximum angular damping (higher = more rotation stability)

        // Braking when stationary
        LINEAR_BRAKING_BASE: 5.0, // Linear braking force per unit mass (higher = stops faster)
        LINEAR_BRAKING_MAX: 200.0, // Maximum linear braking force
        ANGULAR_BRAKING_BASE: 2.0, // Angular braking force per unit mass (higher = stops rotation faster)
        ANGULAR_BRAKING_MAX: 5.0, // Maximum angular braking force

        // Gravity compensation
        GRAVITY_MAGNITUDE: 11, // Gravity acceleration (m/s/s)
        GRAVITY_COMPENSATION: true, // Whether to compensate for gravity during drag

        // Precision and sensitivity
        MINIMUM_DISTANCE_THRESHOLD: 0.02, // Minimum distance to apply forces (smaller = more precision)
        WHEEL_SCROLL_SENSITIVITY: -0.01, // Mouse wheel scroll sensitivity for Z-axis
        ROTATION_SPEED: 1500.0, // speed of arrow key rotation. lower = more precise, higher = more
    } as const

    private _enabled: boolean = false
    private _dragTarget: DragTarget | undefined
    private _isDragging: boolean = false
    private _lastMousePosition: [number, number] = [0, 0]
    private _dragModeStartTime: number | undefined

    // Debug visualization
    private _debugSphere: THREE.Mesh | undefined

    // Wheel event handling for Z-axis dragging
    private _wheelEventHandler: ((event: WheelEvent) => void) | undefined

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _originalInteractionMove: ((i: InteractionMove) => void) | undefined
    private _originalInteractionEnd: ((i: InteractionEnd) => void) | undefined

    private static readonly CAMERA_SETTLE_DURATION = 1.0

    private readonly _unsubscriber: () => void

    public constructor() {
        super()

        // Create wheel event handler for Z-axis dragging
        this._wheelEventHandler = (event: WheelEvent) => {
            if (this._isDragging && this._dragTarget) {
                event.preventDefault()
                this.handleWheelDuringDrag(event)
            }
        }
        this._unsubscriber = EventSystem.listen("DragModeToggled", ({ enabled }) => {
            this.enabled = enabled
        })
    }

    public get enabled(): boolean {
        return this._enabled
    }

    public set enabled(enabled: boolean) {
        if (this._enabled === enabled) return

        this._enabled = enabled

        if (enabled) {
            this._dragModeStartTime = Date.now()
            World.analyticsSystem?.event("Drag Mode Enabled")
            this.hookInteractionHandlers()
        } else {
            this.unhookInteractionHandlers()
            this.stopDragging()

            // Re-enable camera controls in case drag mode was disabled mid-drag.
            World.sceneRenderer.currentCameraControls.enabled = true

            if (this._dragModeStartTime !== undefined) {
                const durationSeconds = (Date.now() - this._dragModeStartTime) / 1000
                World.analyticsSystem?.event("Drag Mode Disabled", { durationSeconds })
                this._dragModeStartTime = undefined
            }
        }

        EventSystem.dispatch("DragModeToggled", { enabled })
    }

    public update(_deltaT: number): void {
        if (!this._enabled) return

        if (this._isDragging && this._dragTarget) {
            this.updateDragForce()
        }
    }

    public destroy(): void {
        this.enabled = false

        // Clean up debug sphere
        this.removeDebugSphere()

        this._unsubscriber?.()
    }

    private createDebugSphere(position: THREE.Vector3): void {
        // Remove existing debug sphere if any
        this.removeDebugSphere()

        // Create a small red sphere to visualize the raycast hit point
        const geometry = new THREE.SphereGeometry(0.05, 16, 16)
        const material = new THREE.MeshBasicMaterial({
            color: 0xff0000,
            transparent: true,
            opacity: 0.8,
        })
        this._debugSphere = new THREE.Mesh(geometry, material)
        this._debugSphere.position.copy(position)

        // Add to the scene
        World.sceneRenderer.scene.add(this._debugSphere)
    }

    private updateDebugSphere(position: THREE.Vector3): void {
        if (this._debugSphere) {
            this._debugSphere.position.copy(position)
        }
    }

    private removeDebugSphere(): void {
        if (this._debugSphere) {
            World.sceneRenderer.scene.remove(this._debugSphere)
            this._debugSphere.geometry.dispose()
            if (this._debugSphere.material instanceof THREE.Material) {
                this._debugSphere.material.dispose()
            }
            this._debugSphere = undefined
        }
    }

    private hookInteractionHandlers(): void {
        const handler = World.sceneRenderer.renderer.domElement.parentElement?.querySelector("canvas")
        if (!handler) return

        const screenHandler = World.sceneRenderer.screenInteractionHandler
        this._originalInteractionStart = screenHandler.interactionStart
        this._originalInteractionMove = screenHandler.interactionMove
        this._originalInteractionEnd = screenHandler.interactionEnd

        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)
        screenHandler.interactionMove = (interaction: InteractionMove) => this.onInteractionMove(interaction)
        screenHandler.interactionEnd = (interaction: InteractionEnd) => this.onInteractionEnd(interaction)

        // Add wheel event listener for Z-axis dragging
        if (this._wheelEventHandler) {
            handler.addEventListener("wheel", this._wheelEventHandler, { passive: false })
        }
    }

    private unhookInteractionHandlers(): void {
        const handler = World.sceneRenderer.renderer.domElement.parentElement?.querySelector("canvas")
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        if (!screenHandler) return

        if (this._originalInteractionStart) screenHandler.interactionStart = this._originalInteractionStart
        if (this._originalInteractionMove) screenHandler.interactionMove = this._originalInteractionMove
        if (this._originalInteractionEnd) screenHandler.interactionEnd = this._originalInteractionEnd

        // Remove wheel event listener
        if (handler && this._wheelEventHandler) {
            handler.removeEventListener("wheel", this._wheelEventHandler)
        }
    }

    private onInteractionStart(interaction: InteractionStart): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionStart?.(interaction)
            return
        }

        this._lastMousePosition = interaction.position

        const target = this.findDragTarget(interaction.position)
        if (target) {
            this.startDragging(
                target.bodyId,
                interaction.position,
                target.hitPoint,
                target.isGamePiece,
                target.rn,
                target.sceneObjectId
            )
        } else {
            this._originalInteractionStart?.(interaction)
        }
    }

    private findDragTarget(mousePos: [number, number]):
        | {
              bodyId: Jolt.BodyID
              hitPoint: THREE.Vector3
              isGamePiece: boolean
              rn: RigidNodeId
              sceneObjectId: SceneObjectId
          }
        | undefined {
        const result = rayCastForRigidBody(mousePos)
        if (!result || !this.isDraggable(result.association)) return undefined
        const isGamePiece = result.association.isGamePiece
        const rn = result.association.rigidNodeId
        return {
            bodyId: result.bodyId,
            hitPoint: result.hitPoint,
            isGamePiece,
            rn,
            sceneObjectId: result.association.sceneObject.id,
        }
    }

    private isDraggable(association: RigidNodeAssociate): boolean {
        // I think this is the fastest way of doing this, since we only do the linear search if there's a multiplayer system
        const isRobot = association.sceneObject.miraType == MiraType.ROBOT
        const isOwnRobot = association.sceneObject.multiplayerOwnerName == undefined

        return association.isGamePiece || (isRobot && isOwnRobot)
    }

    private onInteractionMove(interaction: InteractionMove): void {
        if (this._isDragging && interaction.movement) {
            // Use absolute position instead of accumulating movement to prevent drift
            this._lastMousePosition[0] += interaction.movement[0]
            this._lastMousePosition[1] += interaction.movement[1]

            // Clamp to screen bounds to prevent issues with cursor going off-screen
            this._lastMousePosition[0] = Math.max(0, Math.min(window.innerWidth, this._lastMousePosition[0]))
            this._lastMousePosition[1] = Math.max(0, Math.min(window.innerHeight, this._lastMousePosition[1]))
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

    private startDragging(
        bodyId: Jolt.BodyID,
        mousePos: [number, number],
        hitPoint: THREE.Vector3,
        isGamePiece: boolean,
        rn: RigidNodeId,
        sceneObjectId: SceneObjectId
    ): void {
        const body = World.physicsSystem.getBody(bodyId)
        if (!body) return

        const bodyPos = body.GetPosition()
        const bodyPosition = new THREE.Vector3(bodyPos.GetX(), bodyPos.GetY(), bodyPos.GetZ())
        const bodyRotation = body.GetRotation()
        const bodyQuaternion = new THREE.Quaternion(
            bodyRotation.GetX(),
            bodyRotation.GetY(),
            bodyRotation.GetZ(),
            bodyRotation.GetW()
        )

        const motionProperties = body.GetMotionProperties()
        const mass = 1.0 / motionProperties.GetInverseMass()

        const camera = World.sceneRenderer.mainCamera
        const cameraToHit = hitPoint.clone().sub(camera.position)
        const cameraDirection = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
        const dragDepth = cameraToHit.dot(cameraDirection)

        // Convert the hit point offset to the body's local coordinate system
        const worldOffset = hitPoint.clone().sub(bodyPosition)
        const localOffset = worldOffset.clone().applyQuaternion(bodyQuaternion.clone().invert())

        const association = World.physicsSystem.getBodyAssociation(bodyId) as RigidNodeAssociate
        const isRobot = association?.sceneObject?.miraType === MiraType.ROBOT

        this._dragTarget = {
            bodyId: bodyId,
            initialPosition: bodyPosition.clone(),
            localOffset: localOffset,
            mass: mass,
            dragDepth: dragDepth,
            physicsDisabled: isRobot,
            isGamePiece,
            rn,
            sceneObjectId,
        }

        this._isDragging = true
        this._lastMousePosition = mousePos

        // Create debug sphere at the exact hit point
        this.createDebugSphere(hitPoint)

        if (isRobot) {
            World.physicsSystem.disablePhysicsForBody(bodyId)
        }

        // Face mode should keep the camera enabled tracking the target
        const targetControls = getTargetControls()
        if (targetControls && targetControls.mode !== CameraMode.FACE) {
            targetControls.enabled = false
        }
    }

    private stopDragging(): void {
        if (!this._isDragging) return

        if (this._dragTarget?.physicsDisabled) {
            World.physicsSystem.enablePhysicsForBody(this._dragTarget.bodyId)
        } else if (this._dragTarget) {
            const body = World.physicsSystem.getBody(this._dragTarget.bodyId)
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

                JOLT.destroy(stopBrakingForce)
                JOLT.destroy(angularStopTorque)
            }
        }

        let targetSceneObject: MirabufSceneObject | undefined
        let shouldTransition = true

        if (this._dragTarget) {
            const association = World.physicsSystem.getBodyAssociation(this._dragTarget.bodyId) as RigidNodeAssociate
            targetSceneObject = association?.sceneObject
            if (association?.isGamePiece) {
                shouldTransition = false
            }
        }

        this._isDragging = false
        this._dragTarget = undefined

        // Remove debug sphere when dragging stops
        this.removeDebugSphere()

        const targetControls = getTargetControls()
        if (targetControls) {
            targetControls.enabled = true
            if (shouldTransition) {
                targetControls.settleOntoFocus(targetSceneObject, DragModeSystem.CAMERA_SETTLE_DURATION)
            }
        }
    }

    private updateDragForce(): void {
        if (!this._dragTarget) return

        const body = World.physicsSystem.getBody(this._dragTarget.bodyId)
        if (!body) {
            this.stopDragging()
            return
        }

        // keeping the dragged body awake so drag forces take effect even if body is sleeping
        if (!this._dragTarget.physicsDisabled && !body.IsActive()) {
            World.physicsSystem.activateBody(this._dragTarget.bodyId)
        }

        const currentPos = body.GetPosition()
        const currentPosition = new THREE.Vector3(currentPos.GetX(), currentPos.GetY(), currentPos.GetZ())
        const currentRotation = body.GetRotation()
        const currentQuaternion = new THREE.Quaternion(
            currentRotation.GetX(),
            currentRotation.GetY(),
            currentRotation.GetZ(),
            currentRotation.GetW()
        )

        // Convert the local offset back to world coordinates based on current body rotation
        const currentWorldOffset = this._dragTarget.localOffset.clone().applyQuaternion(currentQuaternion)
        const currentDragPointWorld = currentPosition.clone().add(currentWorldOffset)

        const camera = World.sceneRenderer.mainCamera

        // Create a ray from the camera through the current mouse position
        const mouseNDC = new THREE.Vector2(
            (this._lastMousePosition[0] / window.innerWidth) * 2 - 1,
            -(this._lastMousePosition[1] / window.innerHeight) * 2 + 1
        )

        const raycaster = new THREE.Raycaster()
        raycaster.setFromCamera(mouseNDC, camera)

        // Create a dynamic drag plane perpendicular to the camera at the original drag depth
        const cameraDirection = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
        const dragPlanePosition = camera.position
            .clone()
            .add(cameraDirection.clone().multiplyScalar(this._dragTarget.dragDepth))
        const dragPlane = new THREE.Plane()
        dragPlane.setFromNormalAndCoplanarPoint(cameraDirection, dragPlanePosition)

        const intersectionPoint = new THREE.Vector3()
        const intersected = raycaster.ray.intersectPlane(dragPlane, intersectionPoint)

        if (!intersected) {
            // Fallback: project mouse position onto a sphere around the object
            const fallbackDistance = Math.max(this._dragTarget.dragDepth * 0.5, 1.0)
            const direction = raycaster.ray.direction.clone().normalize()
            intersectionPoint.copy(camera.position).add(direction.multiplyScalar(fallbackDistance))
        }

        // The target is where we want the drag point (on the robot) to be
        const targetDragPointWorld = intersectionPoint

        // Update debug sphere to show where the drag point currently is on the robot
        this.updateDebugSphere(currentDragPointWorld)

        // Calculate the displacement needed to move the current drag point to the target
        const displacement = targetDragPointWorld.clone().sub(currentDragPointWorld)
        const distance = displacement.length()

        if (distance > DragModeSystem.DRAG_FORCE_CONSTANTS.MINIMUM_DISTANCE_THRESHOLD) {
            const maxSpeed = DragModeSystem.DRAG_FORCE_CONSTANTS.MAX_DRAG_SPEED
            const dampingZone = DragModeSystem.DRAG_FORCE_CONSTANTS.DAMPING_ZONE

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

            const forceMultiplier = Math.min(
                mass * DragModeSystem.DRAG_FORCE_CONSTANTS.FORCE_MULTIPLIER_BASE,
                DragModeSystem.DRAG_FORCE_CONSTANTS.FORCE_MULTIPLIER_MAX
            )
            const forceNeeded = velocityError.multiplyScalar(forceMultiplier)

            // Add gravity compensation to counteract downward pull
            if (DragModeSystem.DRAG_FORCE_CONSTANTS.GRAVITY_COMPENSATION) {
                const gravityCompensation = new THREE.Vector3(
                    0,
                    mass * DragModeSystem.DRAG_FORCE_CONSTANTS.GRAVITY_MAGNITUDE,
                    0
                )
                forceNeeded.add(gravityCompensation)
            }

            // Apply force at the center of mass and calculate the torque manually
            // to simulate applying force at the drag point
            const joltForce = convertThreeVector3ToJoltVec3(forceNeeded)
            body.AddForce(joltForce)

            const inertia = body.GetMotionProperties().GetInverseInertiaDiagonal()
            const moi = 1.0 / inertia.Length()
            const yawRotation = new JOLT.Vec3(
                0,
                moi *
                    DragModeSystem.DRAG_FORCE_CONSTANTS.ROTATION_SPEED *
                    (InputSystem.isKeyPressed("ArrowRight") ? 1 : 0 - (InputSystem.isKeyPressed("ArrowLeft") ? 1 : 0)),
                0
            )
            const cameraVector = World.sceneRenderer.mainCamera.getWorldDirection(new THREE.Vector3(0, 0, 0))
            const pitchRotation = new JOLT.Vec3(cameraVector.z, 0, -cameraVector.x).Mul(
                moi *
                    DragModeSystem.DRAG_FORCE_CONSTANTS.ROTATION_SPEED *
                    (InputSystem.isKeyPressed("ArrowUp") ? 1 : 0 - (InputSystem.isKeyPressed("ArrowDown") ? 1 : 0))
            )

            body.AddTorque(yawRotation)
            body.AddTorque(pitchRotation)
        } else {
            // When close to target, apply braking forces and gravity compensation
            const currentVel = body.GetLinearVelocity()
            const mass = this._dragTarget.mass
            const brakingStrength = Math.min(
                mass * DragModeSystem.DRAG_FORCE_CONSTANTS.LINEAR_BRAKING_BASE,
                DragModeSystem.DRAG_FORCE_CONSTANTS.LINEAR_BRAKING_MAX
            )
            const brakingForce = new JOLT.Vec3(
                -currentVel.GetX() * brakingStrength,
                -currentVel.GetY() * brakingStrength,
                -currentVel.GetZ() * brakingStrength
            )

            // Add gravity compensation to prevent falling when stationary
            if (DragModeSystem.DRAG_FORCE_CONSTANTS.GRAVITY_COMPENSATION) {
                const gravityCompensationY = mass * DragModeSystem.DRAG_FORCE_CONSTANTS.GRAVITY_MAGNITUDE
                brakingForce.SetY(brakingForce.GetY() + gravityCompensationY)
            }
            body.AddForce(brakingForce)
        }
        body.SetAngularVelocity(new JOLT.Vec3())

        if (World.multiplayerSystem && this._dragTarget.isGamePiece) {
            const message: Message = {
                type: "updatePhysicsBody",
                data: {
                    sceneObjectId: this._dragTarget.sceneObjectId,
                    ...World.physicsSystem.getBodyUpdateData(body),
                    rigidNodeId: this._dragTarget.rn,
                },
            }
            World.multiplayerSystem.broadcast(message)
        }
    }

    private handleWheelDuringDrag(event: WheelEvent): void {
        if (!this._dragTarget || !this._isDragging) return

        // Adjust drag depth based on wheel delta
        // Positive deltaY = wheel scroll down = move away from camera (increase depth)
        // Negative deltaY = wheel scroll up = move toward camera (decrease depth)
        const depthChange = event.deltaY * DragModeSystem.DRAG_FORCE_CONSTANTS.WHEEL_SCROLL_SENSITIVITY
        this._dragTarget.dragDepth += depthChange

        // Clamp drag depth to reasonable bounds
        this._dragTarget.dragDepth = Math.max(0.5, Math.min(100.0, this._dragTarget.dragDepth))
    }
}

export default DragModeSystem
