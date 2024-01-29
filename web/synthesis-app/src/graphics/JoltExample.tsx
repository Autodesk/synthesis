import * as THREE from 'three';
import Jolt from '../JoltPkg/Jolt.ts'

import { useEffect, useRef } from 'react';
import React from 'react';

let clock = new THREE.Clock();
let time = 0;

let renderer;
let camera;
let scene;

let jolt;
let physicsSystem;
let bodyInterface;

let dynamicObjects: any = [];

const LAYER_NOT_MOVING = 0;
const LAYER_MOVING = 1;
const COUNT_OBJECT_LAYERS = 2;

const wrapVec3 = (v) => new THREE.Vector3(v.GetX(), v.GetY(), v.GetZ());
const wrapQuat = (q) => new THREE.Quaternion(q.GetX(), q.GetY(), q.GetZ(), q.GetW());


// vvv Below are the functions required to initalize everything and draw a basic floor with collisions. vvv

function setupCollisionFiltering(settings) {
    let objectFilter = new Jolt.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS);
    objectFilter.EnableCollision(LAYER_NOT_MOVING, LAYER_MOVING);
    objectFilter.EnableCollision(LAYER_MOVING, LAYER_MOVING);

    const BP_LAYER_NOT_MOVING = new Jolt.BroadPhaseLayer(LAYER_NOT_MOVING);
    const BP_LAYER_MOVING = new Jolt.BroadPhaseLayer(LAYER_MOVING);
    const COUNT_BROAD_PHASE_LAYERS = 2;

    let bpInterface = new Jolt.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_NOT_MOVING, BP_LAYER_NOT_MOVING);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_MOVING, BP_LAYER_MOVING);

    settings.mObjectLayerPairFilter = objectFilter;
    settings.mBroadPhaseLayerInterface = bpInterface;
    settings.mObjectVsBroadPhaseLayerFilter = new Jolt.ObjectVsBroadPhaseLayerFilterTable(settings.mBroadPhaseLayerInterface, COUNT_BROAD_PHASE_LAYERS, settings.mObjectLayerPairFilter, COUNT_OBJECT_LAYERS);
}

function initPhysics() {
    let settings = new Jolt.JoltSettings();
    setupCollisionFiltering(settings);
    jolt = new Jolt.JoltInterface(settings);
    Jolt.destroy(settings);

    physicsSystem = jolt.GetPhysicsSystem();
    bodyInterface = physicsSystem.GetBodyInterface();
}

function initGraphics() {
    renderer = new THREE.WebGLRenderer();
    renderer.setClearColor(0xbfd1e5);
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);

    camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.2, 2000);
    camera.position.set(0, 15, 30);
    camera.lookAt(new THREE.Vector3(0, 0, 0));

    scene = new THREE.Scene();

    let directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(10, 10, 5);
    scene.add(directionalLight);

    // TODO: Add controls.

    // TODO: Add resize event
}

function createMeshForShape(shape) {
    let scale = new Jolt.Vec3(1, 1, 1);
    let triangleContext = new Jolt.ShapeGetTriangles(shape, Jolt.AABox.prototype.sBiggest(), shape.GetCenterOfMass(), Jolt.Quat.prototype.sIdentity(), scale);
    Jolt.destroy(scale);

    let vertices = new Float32Array(Jolt.HEAP32.buffer, triangleContext.GetVerticesData(), triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT);
    let buffer = new THREE.BufferAttribute(vertices, 3).clone();
    Jolt.destroy(triangleContext);

    let geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', buffer);
    geometry.computeVertexNormals();

    return geometry;
}

function getThreeObjForBody(body, color) {
    let material = new THREE.MeshPhongMaterial({ color: color });
    let threeObj;
    let shape = body.GetShape();

    switch (shape.GetSubType()) {
        case Jolt.EShapeSubType_Box:
            let boxShape = Jolt.castObject(shape, Jolt.BoxShape);
            let extent = wrapVec3(boxShape.GetHalfExtent()).multiplyScalar(2);
            threeObj = new THREE.Mesh(new THREE.BoxGeometry(extent.x, extent.y, extent.z, 1, 1, 1), material);
            break;
        case Jolt.EShapeSubType_Capsule:
            // TODO
        case Jolt.EShapeSubType_Cylinder:
            // TODO
        case Jolt.EShapeSubType_Sphere:
            // TODO
        default:
            threeObj = new THREE.Mesh(createMeshForShape(shape), material);
            break;
    }

    threeObj.position.copy(wrapVec3(body.GetPosition()));
    threeObj.quaternion.copy(wrapQuat(body.GetRotation()));

    return threeObj;
}

function addToThreeScene(body, color) {
    let threeObj = getThreeObjForBody(body, color);
    threeObj.userData.body = body;
    scene.add(threeObj);
    dynamicObjects.push(threeObj);
}

function addToScene(body, color) {
    bodyInterface.AddBody(body.GetID(), Jolt.EActivation_Activate);
    addToThreeScene(body, color);
}

function removeFromScene(threeObject) {
	let id = threeObject.userData.body.GetID();
	bodyInterface.RemoveBody(id);
	bodyInterface.DestroyBody(id);
	delete threeObject.userData.body;

	scene.remove(threeObject);
	let idx = dynamicObjects.indexOf(threeObject);
	dynamicObjects.splice(idx, 1);
}

function createFloor(size = 50) {
    let shape = new Jolt.BoxShape(new Jolt.Vec3(size, 0.5, size), 0.05, undefined);
    let position = new Jolt.Vec3(0, -0.5, 0);
    let rotation = new Jolt.Quat(0, 0, 0, 1);
    let creationSettings = new Jolt.BodyCreationSettings(shape, position, rotation, Jolt.EBodyType_Static, LAYER_NOT_MOVING)
    let body = bodyInterface.CreateBody(creationSettings);
    Jolt.destroy(position);
    Jolt.destroy(rotation);
    Jolt.destroy(creationSettings);
    addToScene(body, 0xc7c7c7);

    return body;
}

function updatePhysics(deltaTime) {
    // If below 55hz run 2 steps. Otherwise things run very slow.
    let numSteps = deltaTime > 1.0 / 55.0 ? 2 : 1;
    jolt.Step(deltaTime, numSteps);
}

function render() {
    requestAnimationFrame(render);

    // Prevents a problem when rendering at 30hz. Referred to as the spiral of death.
    let deltaTime = clock.getDelta();
    deltaTime = Math.min(deltaTime, 1.0 / 30.0);

    // Update transforms.
    for (let i = 0, j = dynamicObjects.length; i < j; i++) {
        let threeObj = dynamicObjects[i];
        let body = threeObj.userData.body;
        threeObj.position.copy(wrapVec3(body.GetPosition()));
        threeObj.quaternion.copy(wrapQuat(body.GetRotation()));

        if (body.GetBodyType() === Jolt.EBodyType_SoftBody) {
            // TODO: Special soft body handle.
        }
    }

    onTestUpdate(time, deltaTime);

    time += deltaTime;
    updatePhysics(deltaTime);
    // controls.update(deltaTime); // TODO: Add controls?
    renderer.render(scene, camera);
}

// vvv The following are test functions used to do various basic things. vvv
const timePerObject = 0.1;
let timeNextSpawn = time + timePerObject;

function onTestUpdate(time, deltaTime) {
    if (time > timeNextSpawn) {
        makeBox();
        timeNextSpawn = time + timePerObject;
    }

    if (dynamicObjects.length > 100) {
        removeFromScene(dynamicObjects[2]); // 0 is the floor, don't want to remove that.
    }
}

function getRandomQuat() {
	let vec = new Jolt.Vec3(0.001 + Math.random(), Math.random(), Math.random());
	let quat = Jolt.Quat.prototype.sRotation(vec.Normalized(), 2 * Math.PI * Math.random());
	Jolt.destroy(vec);
	return quat;
}

function makeBox() {
    let pos = new Jolt.Vec3((Math.random() - 0.5) * 25, 15, (Math.random() - 0.5) * 25);
    let rot = getRandomQuat();

    let size = new Jolt.Vec3(0.5, 0.5, 0.5);
    let shape = new Jolt.BoxShape(size, 0.05, undefined);
    let creationSettings = new Jolt.BodyCreationSettings(shape, pos, rot, Jolt.EMotionType_Dynamic, LAYER_MOVING);
    creationSettings.mRestitution = 0.5;
    let body = bodyInterface.CreateBody(creationSettings);

    Jolt.destroy(pos);
    Jolt.destroy(rot);
    Jolt.destroy(size);

    // I feel as though this object should be freed at this point but doing so will cause a crash at runtime.
    // This is the only object where this happens. I'm not sure why. Seems problematic.
    // Jolt.destroy(shape);
    Jolt.destroy(creationSettings);

    addToScene(body, 0xff0000);
}

function MyThree() {
    console.log("Running...");

    const refContainer = useRef<HTMLDivElement>(null);

    useEffect(() => {
        initGraphics();
        
        if (refContainer.current) {
            refContainer.current.innerHTML = "";
            refContainer.current.appendChild(renderer.domElement)
        }

        initPhysics();
        render();

        createFloor();
    }, []);

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    );
}

export default MyThree;
