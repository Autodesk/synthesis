/**
 * This example will be used to showcase how Jolt physics works.
 */

/* eslint-disable  @typescript-eslint/no-explicit-any */

import * as THREE from 'three';
import Stats from 'stats.js';
import JOLT from '../util/loading/JoltSyncLoader.ts';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

import { useEffect, useRef } from 'react';
import { random } from '../util/Random.ts';
import Jolt from '@barclah/jolt-physics';
import { mirabuf } from "../proto/mirabuf"
import loadMirabufRemote from '../mirabuf/MirabufLoader.ts';
import MirabufParser from '../mirabuf/MirabufParser.ts';

const clock = new THREE.Clock();
let time = 0;

let stats: any;

let renderer: any;
let camera: any;
let scene: any;

let joltInterface: any;
let physicsSystem: any;
let bodyInterface: any;

const dynamicObjects: any[] = [];

// const MIRA_FILE = "Team 2471 (2018)_v7.mira"
const MIRA_FILE = "Dozer_v2.mira"

const LAYER_NOT_MOVING = 0;
const LAYER_MOVING = 1;
const COUNT_OBJECT_LAYERS = 2;

const wrapVec3 = (v: Jolt.Vec3) => new THREE.Vector3(v.GetX(), v.GetY(), v.GetZ());
const wrapQuat = (q: Jolt.Quat) => new THREE.Quaternion(q.GetX(), q.GetY(), q.GetZ(), q.GetW());
const wrapMat4 = (m: mirabuf.ITransform) => {
    const arr: number[] | null | undefined = m.spatialMatrix;
    if (!arr) return undefined;
    const pos = new THREE.Vector3(arr[3] * 0.01, arr[7] * 0.01, arr[11] * 0.01);
    const mat = new THREE.Matrix4().fromArray(arr);
    const onlyRotation = new THREE.Matrix4().extractRotation(mat);
    const quat = new THREE.Quaternion().setFromRotationMatrix(onlyRotation);

    return new THREE.Matrix4().compose(pos, quat, new THREE.Vector3(1, 1, 1));
}
let controls: OrbitControls;

// vvv Below are the functions required to initialize everything and draw a basic floor with collisions. vvv

function setupCollisionFiltering(settings: Jolt.JoltSettings) {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS);
    objectFilter.EnableCollision(LAYER_NOT_MOVING, LAYER_MOVING);
    objectFilter.EnableCollision(LAYER_MOVING, LAYER_MOVING);

    const BP_LAYER_NOT_MOVING = new JOLT.BroadPhaseLayer(LAYER_NOT_MOVING);
    const BP_LAYER_MOVING = new JOLT.BroadPhaseLayer(LAYER_MOVING);
    const COUNT_BROAD_PHASE_LAYERS = 2;

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_NOT_MOVING, BP_LAYER_NOT_MOVING);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_MOVING, BP_LAYER_MOVING);

    settings.mObjectLayerPairFilter = objectFilter;
    settings.mBroadPhaseLayerInterface = bpInterface;
    settings.mObjectVsBroadPhaseLayerFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(settings.mBroadPhaseLayerInterface, COUNT_BROAD_PHASE_LAYERS, settings.mObjectLayerPairFilter, COUNT_OBJECT_LAYERS);
}

function initPhysics() {
    const settings = new JOLT.JoltSettings();
    setupCollisionFiltering(settings);
    joltInterface = new JOLT.JoltInterface(settings);
    JOLT.destroy(settings);

    physicsSystem = joltInterface.GetPhysicsSystem();
    bodyInterface = physicsSystem.GetBodyInterface();
}

function initGraphics() {
    camera = new THREE.PerspectiveCamera(
        75,
        window.innerWidth / window.innerHeight,
        0.1,
        1000
    );
    
    camera.position.set(-5, 4, 5);

    scene = new THREE.Scene();

    renderer = new THREE.WebGLRenderer();
    renderer.setClearColor(0x121212);
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.setSize(window.innerWidth, window.innerHeight);

    controls = new OrbitControls(camera, renderer.domElement);
    controls.update();

    const directionalLight = new THREE.DirectionalLight(0xffffff, 3.0);
    directionalLight.position.set(-1.0, 3.0, 2.0);
    directionalLight.castShadow = true;
    scene.add(directionalLight);

    const shadowMapSize = Math.min(4096, renderer.capabilities.maxTextureSize);
    const shadowCamSize = 15;
    console.debug(`Shadow Map Size: ${shadowMapSize}`);

    // console.log(`Cam Top: ${directionalLight.shadow.camera.top}`);
    // console.log(`Cam Bottom: ${directionalLight.shadow.camera.bottom}`);
    // console.log(`Cam Left: ${directionalLight.shadow.camera.left}`);
    // console.log(`Cam Right: ${directionalLight.shadow.camera.right}`);

    directionalLight.shadow.camera.top = shadowCamSize;
    directionalLight.shadow.camera.bottom = -shadowCamSize;
    directionalLight.shadow.camera.left = -shadowCamSize;
    directionalLight.shadow.camera.right = shadowCamSize;
    directionalLight.shadow.mapSize = new THREE.Vector2(shadowMapSize, shadowMapSize);
    directionalLight.shadow.blurSamples = 16;
    directionalLight.shadow.normalBias = -0.02;
    // directionalLight.shadow.bias = -0.01;

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.1);
    scene.add(ambientLight);

    // TODO: Add controls.

    // TODO: Add resize event
}

function createMeshForShape(shape: Jolt.Shape) {
    const scale = new JOLT.Vec3(1, 1, 1);
    const triangleContext = new JOLT.ShapeGetTriangles(shape, JOLT.AABox.prototype.sBiggest(), shape.GetCenterOfMass(), JOLT.Quat.prototype.sIdentity(), scale);
    JOLT.destroy(scale);

    const vertices = new Float32Array(JOLT.HEAP32.buffer, triangleContext.GetVerticesData(), triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT);
    const buffer = new THREE.BufferAttribute(vertices, 3).clone();
    JOLT.destroy(triangleContext);

    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', buffer);
    geometry.computeVertexNormals();

    return geometry;
}

function getThreeObjForBody(body: Jolt.Body, color: THREE.Color) {
    const material = new THREE.MeshPhongMaterial({ color: color, shininess: 0.1 });
    let threeObj;
    const shape = body.GetShape();

    switch (shape.GetSubType()) {
        case JOLT.EShapeSubType_Box: {
            const boxShape = JOLT.castObject(shape, JOLT.BoxShape);
            const extent = wrapVec3(boxShape.GetHalfExtent()).multiplyScalar(2);
            threeObj = new THREE.Mesh(new THREE.BoxGeometry(extent.x, extent.y, extent.z, 1, 1, 1), material);
            threeObj.receiveShadow = true;
            threeObj.castShadow = true;
            break;
        }
        case JOLT.EShapeSubType_Capsule:
            // TODO
            break;
        case JOLT.EShapeSubType_Cylinder:
            // TODO
            break;
        case JOLT.EShapeSubType_Sphere:
            // TODO
            break;
        default:
            threeObj = new THREE.Mesh(createMeshForShape(shape), material);
            threeObj.receiveShadow = true;
            threeObj.castShadow = true;
            break;
    }

    if (!threeObj) return undefined;

    threeObj.position.copy(wrapVec3(body.GetPosition()));
    threeObj.quaternion.copy(wrapQuat(body.GetRotation()));

    return threeObj;
}

function addToThreeScene(body: Jolt.Body, color: THREE.Color) {
    const threeObj = getThreeObjForBody(body, color);
    if (!threeObj) return;
    threeObj.userData.body = body;
    scene.add(threeObj);
    dynamicObjects.push(threeObj);
}

function addToScene(body: Jolt.Body, color: THREE.Color) {
    bodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate);
    addToThreeScene(body, color);
}

function removeFromScene(threeObject: THREE.Mesh) {
	const id = threeObject.userData.body.GetID();
	bodyInterface.RemoveBody(id);
	bodyInterface.DestroyBody(id);
	delete threeObject.userData.body;

	scene.remove(threeObject);
	const idx = dynamicObjects.indexOf(threeObject);
	dynamicObjects.splice(idx, 1);
}

function createFloor(size = 50) {
    const shape = new JOLT.BoxShape(new JOLT.Vec3(size, 0.5, size), 0.05, undefined);
    const position = new JOLT.Vec3(0, -0.5, 0);
    const rotation = new JOLT.Quat(0, 0, 0, 1);
    const creationSettings = new JOLT.BodyCreationSettings(shape, position, rotation, JOLT.EMotionType_Static, LAYER_NOT_MOVING)
    const body = bodyInterface.CreateBody(creationSettings);
    JOLT.destroy(position);
    JOLT.destroy(rotation);
    JOLT.destroy(creationSettings);
    addToScene(body, new THREE.Color(0xc7c7c7));

    return body;
}

function updatePhysics(deltaTime: number) {
    // If below 55hz run 2 steps. Otherwise things run very slow.
    const numSteps = deltaTime > 1.0 / 55.0 ? 2 : 1;
    joltInterface.Step(deltaTime, numSteps);
}

function render() {
    stats.update();
    requestAnimationFrame(render);
    controls.update();

    // Prevents a problem when rendering at 30hz. Referred to as the spiral of death.
    let deltaTime = clock.getDelta();
    deltaTime = Math.min(deltaTime, 1.0 / 30.0);

    // Update transforms.
    for (let i = 0, j = dynamicObjects.length; i < j; i++) {
        const threeObj = dynamicObjects[i];
        const body = threeObj.userData.body;
        threeObj.position.copy(wrapVec3(body.GetPosition()));
        threeObj.quaternion.copy(wrapQuat(body.GetRotation()));

        if (body.GetBodyType() === JOLT.EBodyType_SoftBody) {
            // TODO: Special soft body handle.
        }
    }

    onTestUpdate(time, deltaTime);

    time += deltaTime;
    updatePhysics(1.0 / 60.0);
    // controls.update(deltaTime); // TODO: Add controls?
    renderer.render(scene, camera);
}

// vvv The following are test functions used to do various basic things. vvv

const timePerObject = 0.05;
let timeNextSpawn = time + timePerObject;

// Swap the onTestUpdate function to run the performance test with the random cubes.
// const onTestUpdate = (time, deltaTime) => spawnRandomCubes(time, deltaTime);
const onTestUpdate = (time: number, deltaTime: number) => {};

function spikeTestScene() {
    const boxShape = new JOLT.BoxShape(new JOLT.Vec3(0.5, 0.5, 0.5), 0.1, undefined);
    const boxCreationSettings = new JOLT.BodyCreationSettings(boxShape, new JOLT.Vec3(0, 0.5, 0), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Static, LAYER_NOT_MOVING);
    boxCreationSettings.mCollisionGroup.SetSubGroupID(0);
    const squareBodyBase = bodyInterface.CreateBody(boxCreationSettings);
    addToScene(squareBodyBase, new THREE.Color(0x00ff00));

    const shape = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape.GetMassProperties().mMass = 1;
    const creationSettings = new JOLT.BodyCreationSettings(shape, new JOLT.Vec3(-0.25, 2, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);

    // RECTANGLE BODY 1 (Red)
    creationSettings.mCollisionGroup.SetSubGroupID(1);
    const rectangleBody1 = bodyInterface.CreateBody(creationSettings);
    addToScene(rectangleBody1, new THREE.Color(0xff0000));

    // RECTANGLE BODY 2 (Blue)
    const shape2 = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape2.GetMassProperties().mMass = 1;
    const creationSettings2 = new JOLT.BodyCreationSettings(shape2, new JOLT.Vec3(-0.75, 4, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);
    const rectangleBody2 = bodyInterface.CreateBody(creationSettings2);
    addToScene(rectangleBody2, new THREE.Color(0x3394e8));

    // RECTANGLE BODY 3 (Yellow)
    const shape3 = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape3.GetMassProperties().mMass = 10000;
    const creationSettings3 = new JOLT.BodyCreationSettings(shape3, new JOLT.Vec3(0.25, 4, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);
    const rectangleBody3 = bodyInterface.CreateBody(creationSettings3);
    addToScene(rectangleBody3, new THREE.Color(0xffff00));

    // Left here for future reference.
    // GROUP FILTER
    // let a = squareBodyBase.GetCollisionGroup();
    // a.SetGroupID(0);
    // a.SetSubGroupID(0);
    // let b = rectangleBody1.GetCollisionGroup();
    // b.SetGroupID(0);
    // b.SetSubGroupID(0);
    // let c = rectangleBody2.GetCollisionGroup();
    // c.SetGroupID(0);
    // c.SetSubGroupID(0);
    // let filterTable = new Jolt.GroupFilterTable(3);
    // filterTable.DisableCollision(0, 0);
    // a.SetGroupFilter(filterTable);
    // b.SetGroupFilter(filterTable);
    // c.SetGroupFilter(filterTable);

    // HINGE CONSTRAINT
    const hingeConstraintSettings = new JOLT.HingeConstraintSettings();
    const anchorPoint = new JOLT.Vec3(creationSettings.mPosition.GetX(), creationSettings.mPosition.GetY() - 1.0, creationSettings.mPosition.GetZ() -0.25);
    hingeConstraintSettings.mPoint1 = hingeConstraintSettings.mPoint2 = anchorPoint;
    const axis = new JOLT.Vec3(1, 0, 0)
    const normAxis = new JOLT.Vec3(0, -1, 0);
    hingeConstraintSettings.mHingeAxis1 = hingeConstraintSettings.mHingeAxis2 = axis;
    hingeConstraintSettings.mNormalAxis1 = hingeConstraintSettings.mNormalAxis2 = normAxis;
    physicsSystem.AddConstraint(hingeConstraintSettings.Create(squareBodyBase, rectangleBody1));

    // HINGE CONSTRAINT 2
    const hingeConstraintSettings2 = new JOLT.HingeConstraintSettings();
    const anchorPoint2 = new JOLT.Vec3(creationSettings.mPosition.GetX() - 0.25, creationSettings.mPosition.GetY() + 1.0, creationSettings.mPosition.GetZ());
    hingeConstraintSettings2.mPoint1 = hingeConstraintSettings2.mPoint2 = anchorPoint2;
    const axis2 = new JOLT.Vec3(0, 0, 1)
    const normAxis2 = new JOLT.Vec3(-1, 0, 0);
    hingeConstraintSettings2.mHingeAxis1 = hingeConstraintSettings2.mHingeAxis2 = axis2;
    hingeConstraintSettings2.mNormalAxis1 = hingeConstraintSettings2.mNormalAxis2 = normAxis2;
    physicsSystem.AddConstraint(hingeConstraintSettings2.Create(rectangleBody1, rectangleBody2));

    // HINGE CONSTRAINT 3
    const hingeConstraintSettings3 = new JOLT.HingeConstraintSettings();
    const anchorPoint3 = new JOLT.Vec3(creationSettings.mPosition.GetX() + 0.25, creationSettings.mPosition.GetY() + 1.0, creationSettings.mPosition.GetZ());
    hingeConstraintSettings3.mPoint1 = hingeConstraintSettings3.mPoint2 = anchorPoint3;
    const axis3 = new JOLT.Vec3(0, 0, 1)
    const normAxis3 = new JOLT.Vec3(1, 0, 0);
    hingeConstraintSettings3.mHingeAxis1 = hingeConstraintSettings3.mHingeAxis2 = axis3;
    hingeConstraintSettings3.mNormalAxis1 = hingeConstraintSettings3.mNormalAxis2 = normAxis3;
    physicsSystem.AddConstraint(hingeConstraintSettings3.Create(rectangleBody1, rectangleBody3));
}

function spawnRandomCubes(time: number, deltaTime: number) {
    if (time > timeNextSpawn) {
        makeRandomBox();
        timeNextSpawn = time + timePerObject;
    }

    if (dynamicObjects.length > 500) {
        removeFromScene(dynamicObjects[2]); // 0 &&|| 1 is the floor, don't want to remove that.
    }
}

function getRandomQuat() {
	const vec = new JOLT.Vec3(0.001 + random(), random(), random());
	const quat = JOLT.Quat.prototype.sRotation(vec.Normalized(), 2 * Math.PI * random());
	JOLT.destroy(vec);
	return quat;
}

function makeRandomBox() {
    const pos = new JOLT.Vec3((random() - 0.5) * 25, 15, (random() - 0.5) * 25);
    const rot = getRandomQuat();

    const x = random();
    const y = random();
    const z = random();
    const size = new JOLT.Vec3(x, y, z);
    const shape = new JOLT.BoxShape(size, 0.05, undefined);
    const creationSettings = new JOLT.BodyCreationSettings(shape, pos, rot, JOLT.EMotionType_Dynamic, LAYER_MOVING);
    creationSettings.mRestitution = 0.5;
    const body = bodyInterface.CreateBody(creationSettings);

    JOLT.destroy(pos);
    JOLT.destroy(rot);
    JOLT.destroy(size);

    // I feel as though this object should be freed at this point but doing so will cause a crash at runtime.
    // This is the only object where this happens. I'm not sure why. Seems problematic.
    // Jolt.destroy(shape);

    JOLT.destroy(creationSettings);

    addToScene(body, new THREE.Color(0xff0000));
}

function MyThree() {
    console.log("Running...");

    const refContainer = useRef<HTMLDivElement>(null);


    useEffect(() => {
        loadMirabufRemote(MIRA_FILE).then((assembly: mirabuf.Assembly | undefined) => {
            if (!assembly) return;
            const data = assembly.data;
            console.log(assembly.toJSON())
            if (!data) return;
            const parts = data.parts;
            if (!parts) return;
            const partInstances = new Map<string, mirabuf.IPartInstance>();
            for (const partInstance of Object.values(parts.partInstances!)) {
                partInstances.set(partInstance.info!.GUID!, partInstance);
            }

            // const 
            const parser = new MirabufParser(assembly);
            const root = parser.designHierarchyRoot;
            
            const transforms = new Map<string, THREE.Matrix4>();
            const getTransforms = (node: mirabuf.INode, parent: THREE.Matrix4) => {
                for (const child of node.children!) {
                    if (!partInstances.has(child.value!)) {
                        continue;
                    }
                    const partInstance = partInstances.get(child.value!)!;
                    if (transforms.has(child.value!)) continue;
                    const mat = wrapMat4(partInstance.transform!)!;
                    transforms.set(child.value!, mat.multiply(parent));
                    getTransforms(child, mat);
                }
            }

            for (const child of root.children!) {
                const partInstance = partInstances.get(child.value!)!;
                let mat;
                if (!partInstance.transform) {
                    const def = parts.partDefinitions![partInstances.get(child.value!)!.partDefinitionReference!];
                    if (!def.baseTransform) {
                        mat = new THREE.Matrix4().identity();
                    } else {
                        mat = wrapMat4(def.baseTransform);
                    }
                } else {
                    mat = wrapMat4(partInstance.transform);
                }
                transforms.set(partInstance.info!.GUID!, mat!);
                getTransforms(child, mat!);
            }

            const definitions = parts.partDefinitions;
            if (!definitions) return;
            for (const definition of Object.values(definitions)) {
                const bodies = definition.bodies;
                if (!bodies) continue;
                for (const body of bodies) {
                    if (!body) continue;
                    const mesh = body.triangleMesh;
                    const geometry = new THREE.BufferGeometry();
                    if (mesh && mesh.mesh && mesh.mesh.verts && mesh.mesh.normals && mesh.mesh.uv && mesh.mesh.indices) {

                        const newVerts = new Float32Array(mesh.mesh.verts.length);
                        for (let i = 0; i < mesh.mesh.verts.length; i += 3) {
                            newVerts[i] = mesh.mesh.verts.at(i)! / 100.0;
                            newVerts[i + 1] = mesh.mesh.verts.at(i + 1)! / 100.0;
                            newVerts[i + 2] = mesh.mesh.verts.at(i + 2)! / 100.0;
                        }

                        const newNorms = new Float32Array(mesh.mesh.normals.length);
                        for (let i = 0; i < mesh.mesh.normals.length; i += 3) {
                            newNorms[i] = mesh.mesh.normals.at(i)! / 100.0;
                            newNorms[i + 1] = mesh.mesh.normals.at(i + 1)! / 100.0;
                            newNorms[i + 2] = mesh.mesh.normals.at(i + 2)! / 100.0;
                        }

                        geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(newVerts), 3));
                        geometry.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(newNorms), 3));
                        geometry.setAttribute('uv', new THREE.BufferAttribute(new Float32Array(mesh.mesh.uv), 2));
                        geometry.setIndex(mesh.mesh.indices);

                        const appearanceOverride = body.appearanceOverride;
                        let material;
                        let appearances;
                        if (appearanceOverride && (appearances = data.materials?.appearances) && appearances[appearanceOverride]) {
                            const miraMaterial = data.materials.appearances[appearanceOverride];
                            let hex = 0xe32b50;
                            if (miraMaterial.albedo) {
                                const {A, B, G, R} = miraMaterial.albedo;
                                if (A && B && G && R)
                                    hex = A << 24 | R << 16 | G << 8  | B;
                            }

                            material = new THREE.MeshPhongMaterial({
                                color: hex,
                                shininess: 0.5,
                            });
                        }
                        for (const entry of transforms.entries()) {
                            if (partInstances.get(entry[0])!.partDefinitionReference! != definition.info!.GUID!) continue;
                            geometry.applyMatrix4(entry[1]);
                        }
                        const threeMesh = new THREE.Mesh( geometry, material );
                        threeMesh.receiveShadow = true;
                        threeMesh.castShadow = true;
                        scene.add(threeMesh);
                    }
                }
            }
        })
        initGraphics();
        
        if (refContainer.current) {
            refContainer.current.innerHTML = "";
            refContainer.current.appendChild(renderer.domElement)

            stats = new Stats();
            stats.domElement.style.position = 'absolute';
            stats.domElement.style.top = '0px';
            refContainer.current.appendChild(stats.domElement);
        }

        initPhysics();
        render();

        createFloor();

        // Spawn the y-cube of blocks as specified in the spike document.
        // spikeTestScene();
    }, []);

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    );
}

export default MyThree;
