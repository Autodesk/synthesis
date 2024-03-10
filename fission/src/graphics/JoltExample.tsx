/**
 * This example will be used to showcase how Jolt physics works.
 */

/* eslint-disable  @typescript-eslint/no-explicit-any */

import * as THREE from 'three';
import Stats from 'stats.js';
import JOLT from '../util/loading/JoltSyncLoader.ts';

import { useEffect, useRef } from 'react';
import React from 'react';
import { Random } from '../util/Random.ts';

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

const LAYER_NOT_MOVING = 0;
const LAYER_MOVING = 1;
const COUNT_OBJECT_LAYERS = 2;

const wrapVec3 = (v) => new THREE.Vector3(v.GetX(), v.GetY(), v.GetZ());
const wrapQuat = (q) => new THREE.Quaternion(q.GetX(), q.GetY(), q.GetZ(), q.GetW());


// vvv Below are the functions required to initalize everything and draw a basic floor with collisions. vvv

function setupCollisionFiltering(settings) {
    let objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS);
    objectFilter.EnableCollision(LAYER_NOT_MOVING, LAYER_MOVING);
    objectFilter.EnableCollision(LAYER_MOVING, LAYER_MOVING);

    const BP_LAYER_NOT_MOVING = new JOLT.BroadPhaseLayer(LAYER_NOT_MOVING);
    const BP_LAYER_MOVING = new JOLT.BroadPhaseLayer(LAYER_MOVING);
    const COUNT_BROAD_PHASE_LAYERS = 2;

    let bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_NOT_MOVING, BP_LAYER_NOT_MOVING);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_MOVING, BP_LAYER_MOVING);

    settings.mObjectLayerPairFilter = objectFilter;
    settings.mBroadPhaseLayerInterface = bpInterface;
    settings.mObjectVsBroadPhaseLayerFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(settings.mBroadPhaseLayerInterface, COUNT_BROAD_PHASE_LAYERS, settings.mObjectLayerPairFilter, COUNT_OBJECT_LAYERS);
}

function initPhysics() {
    let settings = new JOLT.JoltSettings();
    setupCollisionFiltering(settings);
    joltInterface = new JOLT.JoltInterface(settings);
    JOLT.destroy(settings);

    physicsSystem = joltInterface.GetPhysicsSystem();
    bodyInterface = physicsSystem.GetBodyInterface();
}

function initGraphics() {
    renderer = new THREE.WebGLRenderer();
    renderer.setClearColor(0xbfd1e5);
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);

    camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.2, 2000);
    camera.position.set(-5, 4, 5);
    camera.lookAt(new THREE.Vector3(0, 0.5, 0));

    scene = new THREE.Scene();

    let directionalLight = new THREE.DirectionalLight(0xffffff, 2);
    directionalLight.position.set(10, 10, 5);
    scene.add(directionalLight);

    let ambientLight = new THREE.AmbientLight(0xffffff, 0.1);
    scene.add(ambientLight);

    // TODO: Add controls.

    // TODO: Add resize event
}

function createMeshForShape(shape) {
    let scale = new JOLT.Vec3(1, 1, 1);
    let triangleContext = new JOLT.ShapeGetTriangles(shape, JOLT.AABox.prototype.sBiggest(), shape.GetCenterOfMass(), JOLT.Quat.prototype.sIdentity(), scale);
    JOLT.destroy(scale);

    let vertices = new Float32Array(JOLT.HEAP32.buffer, triangleContext.GetVerticesData(), triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT);
    let buffer = new THREE.BufferAttribute(vertices, 3).clone();
    JOLT.destroy(triangleContext);

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
        case JOLT.EShapeSubType_Box:
            let boxShape = JOLT.castObject(shape, JOLT.BoxShape);
            let extent = wrapVec3(boxShape.GetHalfExtent()).multiplyScalar(2);
            threeObj = new THREE.Mesh(new THREE.BoxGeometry(extent.x, extent.y, extent.z, 1, 1, 1), material);
            break;
        case JOLT.EShapeSubType_Capsule:
            // TODO
        case JOLT.EShapeSubType_Cylinder:
            // TODO
        case JOLT.EShapeSubType_Sphere:
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
    bodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate);
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
    let shape = new JOLT.BoxShape(new JOLT.Vec3(size, 0.5, size), 0.05, undefined);
    let position = new JOLT.Vec3(0, -0.5, 0);
    let rotation = new JOLT.Quat(0, 0, 0, 1);
    let creationSettings = new JOLT.BodyCreationSettings(shape, position, rotation, JOLT.EMotionType_Static, LAYER_NOT_MOVING)
    let body = bodyInterface.CreateBody(creationSettings);
    JOLT.destroy(position);
    JOLT.destroy(rotation);
    JOLT.destroy(creationSettings);
    addToScene(body, 0xc7c7c7);

    return body;
}

function updatePhysics(deltaTime) {
    // If below 55hz run 2 steps. Otherwise things run very slow.
    let numSteps = deltaTime > 1.0 / 55.0 ? 2 : 1;
    joltInterface.Step(deltaTime, numSteps);
}

function render() {
    stats.update();
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
const onTestUpdate = (time, deltaTime) => {};

function spikeTestScene() {
    let boxShape = new JOLT.BoxShape(new JOLT.Vec3(0.5, 0.5, 0.5), 0.1, undefined);
    let boxCreationSettings = new JOLT.BodyCreationSettings(boxShape, new JOLT.Vec3(0, 0.5, 0), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Static, LAYER_NOT_MOVING);
    boxCreationSettings.mCollisionGroup.SetSubGroupID(0);
    let squareBodyBase = bodyInterface.CreateBody(boxCreationSettings);
    addToScene(squareBodyBase, 0x00ff00);

    let shape = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape.GetMassProperties().mMass = 1;
    let creationSettings = new JOLT.BodyCreationSettings(shape, new JOLT.Vec3(-0.25, 2, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);

    // RECTANGLE BODY 1 (Red)
    creationSettings.mCollisionGroup.SetSubGroupID(1);
    let rectangleBody1 = bodyInterface.CreateBody(creationSettings);
    addToScene(rectangleBody1, 0xff0000);

    // RECTANGLE BODY 2 (Blue)
    let shape2 = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape2.GetMassProperties().mMass = 1;
    let creationSettings2 = new JOLT.BodyCreationSettings(shape2, new JOLT.Vec3(-0.75, 4, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);
    let rectangleBody2 = bodyInterface.CreateBody(creationSettings2);
    addToScene(rectangleBody2, 0x3394e8);

    // RECTANGLE BODY 3 (Yellow)
    let shape3 = new JOLT.BoxShape(new JOLT.Vec3(0.25, 1, 0.25), 0.1, undefined);
    shape3.GetMassProperties().mMass = 10000;
    let creationSettings3 = new JOLT.BodyCreationSettings(shape3, new JOLT.Vec3(0.25, 4, 0.75), JOLT.Quat.prototype.sIdentity(), JOLT.EMotionType_Dynamic, LAYER_MOVING);
    let rectangleBody3 = bodyInterface.CreateBody(creationSettings3);
    addToScene(rectangleBody3, 0xffff00);

    // Left here for future reference.
    // GROUP FITLER
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
    let hingeConstraintSettings = new JOLT.HingeConstraintSettings();
    let anchorPoint = new JOLT.Vec3(creationSettings.mPosition.GetX(), creationSettings.mPosition.GetY() - 1.0, creationSettings.mPosition.GetZ() -0.25);
    hingeConstraintSettings.mPoint1 = hingeConstraintSettings.mPoint2 = anchorPoint;
    let axis = new JOLT.Vec3(1, 0, 0)
    let normAxis = new JOLT.Vec3(0, -1, 0);
    hingeConstraintSettings.mHingeAxis1 = hingeConstraintSettings.mHingeAxis2 = axis;
    hingeConstraintSettings.mNormalAxis1 = hingeConstraintSettings.mNormalAxis2 = normAxis;
    physicsSystem.AddConstraint(hingeConstraintSettings.Create(squareBodyBase, rectangleBody1));

    // HINGE CONSTRAINT 2
    let hingeConstraintSettings2 = new JOLT.HingeConstraintSettings();
    let anchorPoint2 = new JOLT.Vec3(creationSettings.mPosition.GetX() - 0.25, creationSettings.mPosition.GetY() + 1.0, creationSettings.mPosition.GetZ());
    hingeConstraintSettings2.mPoint1 = hingeConstraintSettings2.mPoint2 = anchorPoint2;
    let axis2 = new JOLT.Vec3(0, 0, 1)
    let normAxis2 = new JOLT.Vec3(-1, 0, 0);
    hingeConstraintSettings2.mHingeAxis1 = hingeConstraintSettings2.mHingeAxis2 = axis2;
    hingeConstraintSettings2.mNormalAxis1 = hingeConstraintSettings2.mNormalAxis2 = normAxis2;
    physicsSystem.AddConstraint(hingeConstraintSettings2.Create(rectangleBody1, rectangleBody2));

    // HINGE CONSTRAINT 3
    let hingeConstraintSettings3 = new JOLT.HingeConstraintSettings();
    let anchorPoint3 = new JOLT.Vec3(creationSettings.mPosition.GetX() + 0.25, creationSettings.mPosition.GetY() + 1.0, creationSettings.mPosition.GetZ());
    hingeConstraintSettings3.mPoint1 = hingeConstraintSettings3.mPoint2 = anchorPoint3;
    let axis3 = new JOLT.Vec3(0, 0, 1)
    let normAxis3 = new JOLT.Vec3(1, 0, 0);
    hingeConstraintSettings3.mHingeAxis1 = hingeConstraintSettings3.mHingeAxis2 = axis3;
    hingeConstraintSettings3.mNormalAxis1 = hingeConstraintSettings3.mNormalAxis2 = normAxis3;
    physicsSystem.AddConstraint(hingeConstraintSettings3.Create(rectangleBody1, rectangleBody3));
}

function spawnRandomCubes(time, deltaTime) {
    if (time > timeNextSpawn) {
        makeRandomBox();
        timeNextSpawn = time + timePerObject;
    }

    if (dynamicObjects.length > 500) {
        removeFromScene(dynamicObjects[2]); // 0 &&|| 1 is the floor, don't want to remove that.
    }
}

function getRandomQuat() {
	let vec = new JOLT.Vec3(0.001 + Random(), Random(), Random());
	let quat = JOLT.Quat.prototype.sRotation(vec.Normalized(), 2 * Math.PI * Random());
	JOLT.destroy(vec);
	return quat;
}

function makeRandomBox() {
    let pos = new JOLT.Vec3((Random() - 0.5) * 25, 15, (Random() - 0.5) * 25);
    let rot = getRandomQuat();

    let x = Random();
    let y = Random();
    let z = Random();
    let size = new JOLT.Vec3(x, y, z);
    let shape = new JOLT.BoxShape(size, 0.05, undefined);
    let creationSettings = new JOLT.BodyCreationSettings(shape, pos, rot, JOLT.EMotionType_Dynamic, LAYER_MOVING);
    creationSettings.mRestitution = 0.5;
    let body = bodyInterface.CreateBody(creationSettings);

    JOLT.destroy(pos);
    JOLT.destroy(rot);
    JOLT.destroy(size);

    // I feel as though this object should be freed at this point but doing so will cause a crash at runtime.
    // This is the only object where this happens. I'm not sure why. Seems problematic.
    // Jolt.destroy(shape);

    JOLT.destroy(creationSettings);

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

            stats = new Stats();
            stats.domElement.style.position = 'absolute';
            stats.domElement.style.top = '0px';
            refContainer.current.appendChild(stats.domElement);
        }

        initPhysics();
        render();

        createFloor();

        // Spawn the y-cube of blocks as specified in the spike document.
        spikeTestScene();
    }, []);

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    );
}

export default MyThree;