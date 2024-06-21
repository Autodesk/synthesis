import JOLT from "../loading/JoltSyncLoader.ts"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import {
    JoltVec3_ThreeVector3,
    JoltQuat_ThreeQuaternion,
} from "../TypeConversions.ts"

export const LAYER_NOT_MOVING = 0
export const LAYER_MOVING = 1
export const COUNT_OBJECT_LAYERS = 2

export function createMeshForShape(shape: Jolt.Shape) {
    const scale = new JOLT.Vec3(1, 1, 1)
    const triangleContext = new JOLT.ShapeGetTriangles(
        shape,
        JOLT.AABox.prototype.sBiggest(),
        shape.GetCenterOfMass(),
        JOLT.Quat.prototype.sIdentity(),
        scale
    )
    JOLT.destroy(scale)

    const vertices = new Float32Array(
        JOLT.HEAP32.buffer,
        triangleContext.GetVerticesData(),
        triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT
    )
    const buffer = new THREE.BufferAttribute(vertices, 3).clone()
    JOLT.destroy(triangleContext)

    const geometry = new THREE.BufferGeometry()
    geometry.setAttribute("position", buffer)
    geometry.computeVertexNormals()

    return geometry
}

export function getThreeObjForBody(body: Jolt.Body, color: THREE.Color) {
    const material = new THREE.MeshPhongMaterial({
        color: color,
        shininess: 0.1,
    })
    let threeObj
    const shape = body.GetShape()

    switch (shape.GetSubType()) {
        case JOLT.EShapeSubType_Box: {
            const boxShape = JOLT.castObject(shape, JOLT.BoxShape)
            const extent = JoltVec3_ThreeVector3(
                boxShape.GetHalfExtent()
            ).multiplyScalar(2)
            threeObj = new THREE.Mesh(
                new THREE.BoxGeometry(extent.x, extent.y, extent.z, 1, 1, 1),
                material
            )
            threeObj.receiveShadow = true
            threeObj.castShadow = true
            break
        }
        case JOLT.EShapeSubType_Capsule:
            // TODO
            break
        case JOLT.EShapeSubType_Cylinder:
            // TODO
            break
        case JOLT.EShapeSubType_Sphere:
            // TODO
            break
        default:
            threeObj = new THREE.Mesh(createMeshForShape(shape), material)
            threeObj.receiveShadow = true
            threeObj.castShadow = true
            break
    }

    if (!threeObj) return undefined

    threeObj.position.copy(JoltVec3_ThreeVector3(body.GetPosition()))
    threeObj.quaternion.copy(JoltQuat_ThreeQuaternion(body.GetRotation()))

    return threeObj
}

export function addToThreeScene(
    scene: THREE.Scene,
    body: Jolt.Body,
    color: THREE.Color,
    dynamicObjects: THREE.Mesh[]
) {
    const threeObj = getThreeObjForBody(body, color)
    if (!threeObj) return
    threeObj.userData.body = body
    scene.add(threeObj)
    dynamicObjects.push(threeObj)
}

export function addToScene(
    scene: THREE.Scene,
    body: Jolt.Body,
    color: THREE.Color,
    bodyInterface: Jolt.BodyInterface,
    dynamicObjects: THREE.Mesh[]
) {
    bodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)
    addToThreeScene(scene, body, color, dynamicObjects)
}

export function removeFromScene(
    scene: THREE.Scene,
    threeObject: THREE.Mesh,
    bodyInterface: Jolt.BodyInterface,
    dynamicObjects: THREE.Mesh[]
) {
    const id = threeObject.userData.body.GetID()
    bodyInterface.RemoveBody(id)
    bodyInterface.DestroyBody(id)
    delete threeObject.userData.body

    scene.remove(threeObject)
    const idx = dynamicObjects.indexOf(threeObject)
    dynamicObjects.splice(idx, 1)
}
