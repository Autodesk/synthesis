import * as THREE from "three"
import WorldSystem from "../WorldSystem"
import World from "../World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import {
    InteractionStart,
    InteractionMove,
    InteractionEnd,
    PRIMARY_MOUSE_INTERACTION,
} from "./ScreenInteractionHandler"
import { JoltVec3_ThreeVector3 } from "@/util/TypeConversions"
import Jolt from "@barclah/jolt-physics"
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

class DragModeSystem extends WorldSystem {
    private _enabled: boolean = false
    private _dragTarget: DragTarget | undefined
    private _isDragging: boolean = false
    private _lastMousePosition: [number, number] = [0, 0]

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _originalInteractionMove: ((i: InteractionMove) => void) | undefined
    private _originalInteractionEnd: ((i: InteractionEnd) => void) | undefined

    public constructor() {
        super()
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
        }

        window.dispatchEvent(new CustomEvent("dragModeToggled", { detail: { enabled } }))
    }

    public Update(_deltaT: number): void {
        if (!this._enabled || !this._isDragging || !this._dragTarget) return

        this.updateDragForce()
    }

    public Destroy(): void {
        this.enabled = false
    }

    private hookInteractionHandlers(): void {
        const handler = World.SceneRenderer.renderer.domElement.parentElement?.querySelector("canvas")
        if (!handler) return

        const screenHandler = (World.SceneRenderer as any)._screenInteractionHandler
        this._originalInteractionStart = screenHandler.interactionStart
        this._originalInteractionMove = screenHandler.interactionMove
        this._originalInteractionEnd = screenHandler.interactionEnd

        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)
        screenHandler.interactionMove = (interaction: InteractionMove) => this.onInteractionMove(interaction)
        screenHandler.interactionEnd = (interaction: InteractionEnd) => this.onInteractionEnd(interaction)
    }

    private unhookInteractionHandlers(): void {
        const screenHandler = (World.SceneRenderer as any)._screenInteractionHandler
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
        const isComplexObject = association?.sceneObject?.miraType === MiraType.ROBOT

        this._dragTarget = {
            bodyId: bodyId,
            initialPosition: bodyPosition.clone(),
            offset: hitPoint.clone().sub(bodyPosition),
            mass: mass,
            dragPlane: dragPlane,
            dragDepth: dragDepth,
            physicsDisabled: isComplexObject,
        }

        this._isDragging = true
        this._lastMousePosition = mousePos

        if (isComplexObject) {
            World.PhysicsSystem.DisablePhysicsForBody(bodyId)
        }

        World.SceneRenderer.currentCameraControls.enabled = false
    }

    private stopDragging(): void {
        if (!this._isDragging) return

        if (this._dragTarget?.physicsDisabled) {
            World.PhysicsSystem.EnablePhysicsForBody(this._dragTarget.bodyId)
        }

        this._isDragging = false
        this._dragTarget = undefined

        World.SceneRenderer.currentCameraControls.enabled = true
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
            const maxSpeed = 20.0
            const dampingZone = 0.3

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

            const velocityChange = desiredVelocity.sub(currentVelocity)

            const mass = this._dragTarget.mass
            const forceNeeded = velocityChange.multiplyScalar(mass * 60.0)

            const joltForce = ThreeVector3_JoltVec3(forceNeeded)
            body.AddForce(joltForce)

            const dampingStrength = mass * 8.0
            const dampingForce = new JOLT.Vec3(
                -currentVel.GetX() * dampingStrength,
                -currentVel.GetY() * dampingStrength,
                -currentVel.GetZ() * dampingStrength
            )
            body.AddForce(dampingForce)
        } else {
            const currentVel = body.GetLinearVelocity()
            const brakingStrength = this._dragTarget.mass * 7.0
            const brakingForce = new JOLT.Vec3(
                -currentVel.GetX() * brakingStrength,
                -currentVel.GetY() * brakingStrength,
                -currentVel.GetZ() * brakingStrength
            )
            body.AddForce(brakingForce)
        }
    }
}

export default DragModeSystem
