/**
 * This example will be used to showcase how Jolt physics works.
 */

import * as THREE from 'three';
import Stats from 'stats.js';
import JOLT from '../util/loading/JoltSyncLoader.ts';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

import { useEffect, useRef } from 'react';
import { random } from '../util/Random.ts';
import Jolt from '@barclah/jolt-physics';
import { mirabuf } from "../proto/mirabuf"
import { LoadMirabufRemote } from '../mirabuf/MirabufLoader.ts';
import MirabufParser from '../mirabuf/MirabufParser.ts';
import { MirabufTransform_ThreeMatrix4 } from '../util/conversions/MiraThreeConversions.ts';
import { JoltVec3_ThreeVector3, JoltQuat_ThreeQuaternion } from '../util/conversions/JoltThreeConversions';
import { COUNT_OBJECT_LAYERS, LAYER_MOVING, LAYER_NOT_MOVING, addToScene, removeFromScene } from '../util/threejs/MeshCreation.ts';

const clock = new THREE.Clock();
let time = 0;

let stats: Stats;

let renderer: THREE.WebGLRenderer;
let camera: THREE.PerspectiveCamera;
let scene: THREE.Scene;

let joltInterface: Jolt.JoltInterface;
let physicsSystem: Jolt.PhysicsSystem;
let bodyInterface: Jolt.BodyInterface;

const dynamicObjects: THREE.Mesh[] = [];

const MIRA_FILE = "test_mira/Team_2471_(2018)_v7.mira"
// const MIRA_FILE = "test_mira/Dozer_v2.mira"


let hacky: string = '';

let controls: OrbitControls;

function matToString(mat: THREE.Matrix4) {
    const arr = mat.toArray();
    return `[\n${arr[0].toFixed(4)}, ${arr[4].toFixed(4)}, ${arr[8].toFixed(4)}, ${arr[12].toFixed(4)},\n`
        + `${arr[1].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[13].toFixed(4)},\n`
        + `${arr[2].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[14].toFixed(4)},\n`
        + `${arr[3].toFixed(4)}, ${arr[7].toFixed(4)}, ${arr[11].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

function miraMatToString(mat: mirabuf.ITransform) {
    const arr = mat.spatialMatrix!;
    return `[\n${arr[0].toFixed(4)}, ${arr[1].toFixed(4)}, ${arr[2].toFixed(4)}, ${arr[3].toFixed(4)},\n`
        + `${arr[4].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[7].toFixed(4)},\n`
        + `${arr[8].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[11].toFixed(4)},\n`
        + `${arr[12].toFixed(4)}, ${arr[13].toFixed(4)}, ${arr[14].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

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

function createFloor(size = 50) {
    const shape = new JOLT.BoxShape(new JOLT.Vec3(size, 0.5, size), 0.05, undefined);
    const position = new JOLT.Vec3(0, -0.5, 0);
    const rotation = new JOLT.Quat(0, 0, 0, 1);
    const creationSettings = new JOLT.BodyCreationSettings(shape, position, rotation, JOLT.EMotionType_Static, LAYER_NOT_MOVING)
    const body = bodyInterface.CreateBody(creationSettings);
    JOLT.destroy(position);
    JOLT.destroy(rotation);
    JOLT.destroy(creationSettings);
    addToScene(scene, body, new THREE.Color(0xc7c7c7), bodyInterface, dynamicObjects);

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
        threeObj.position.copy(JoltVec3_ThreeVector3(body.GetPosition()));
        threeObj.quaternion.copy(JoltQuat_ThreeQuaternion(body.GetRotation()));

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


// Swap the onTestUpdate function to run the performance test with the random cubes.
// const onTestUpdate = (time, deltaTime) => spawnRandomCubes(time, deltaTime);
const onTestUpdate = (time: number, deltaTime: number) => {};

function MyThree() {
    console.log("Running...");

    const refContainer = useRef<HTMLDivElement>(null);


    useEffect(() => {
        LoadMirabufRemote(MIRA_FILE).then((assembly: mirabuf.Assembly | undefined) => {
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
                    hacky = partInstance.info!.name!;
                    
                    if (transforms.has(child.value!)) continue;
                    const mat = MirabufTransform_ThreeMatrix4(partInstance.transform!)!;

                    console.log(`[${partInstance.info!.name!}] -> ${matToString(mat)}`);

                    transforms.set(child.value!, mat.premultiply(parent));
                    getTransforms(child, mat);
                }
            }

            for (const child of root.children!) {
                const partInstance = partInstances.get(child.value!)!;
                let mat;
                hacky = partInstance.info!.name!;
                if (!partInstance.transform) {
                    const def = parts.partDefinitions![partInstances.get(child.value!)!.partDefinitionReference!];
                    if (!def.baseTransform) {
                        mat = new THREE.Matrix4().identity();
                    } else {
                        mat = MirabufTransform_ThreeMatrix4(def.baseTransform);
                    }
                } else {
                    mat = MirabufTransform_ThreeMatrix4(partInstance.transform);
                }

                console.log(`[${partInstance.info!.name!}] -> ${matToString(mat!)}`);

                transforms.set(partInstance.info!.GUID!, mat!);
                getTransforms(child, mat!);
            }

            let i = 0;

            const materials = [
                new THREE.MeshToonMaterial({
                    color: 0xe32b50
                }),
                new THREE.MeshToonMaterial({
                    color: 0x4ccf57
                }),
                new THREE.MeshToonMaterial({
                    color: 0xcf4cca
                })
            ]

            const instances = parts.partInstances;
            if (!instances) return;
            for (const instance of Object.values(instances)/* .filter(x => x.info!.name!.startsWith('EyeBall')) */) {
                const definition = assembly.data!.parts!.partDefinitions![instance.partDefinitionReference!]!;
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
                            const normLength = Math.sqrt(mesh.mesh.normals.at(i)! * mesh.mesh.normals.at(i)! +
                                mesh.mesh.normals.at(i + 1)! * mesh.mesh.normals.at(i + 1)! +
                                mesh.mesh.normals.at(i + 2)! * mesh.mesh.normals.at(i + 2)!
                            );

                            newNorms[i] = mesh.mesh.normals.at(i)! / normLength;
                            newNorms[i + 1] = mesh.mesh.normals.at(i + 1)! / normLength;
                            newNorms[i + 2] = mesh.mesh.normals.at(i + 2)! / normLength;
                        }

                        geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(newVerts), 3));
                        geometry.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(newNorms), 3));
                        geometry.setAttribute('uv', new THREE.BufferAttribute(new Float32Array(mesh.mesh.uv), 2));
                        geometry.setIndex(mesh.mesh.indices);

                        const appearanceOverride = body.appearanceOverride;
                        const material = materials[i++ % materials.length];
                        let appearances;

                        // if (appearanceOverride && (appearances = data.materials?.appearances) && appearances[appearanceOverride]) {
                        //     const miraMaterial = data.materials.appearances[appearanceOverride];
                        //     let hex = 0xe32b50;
                        //     if (miraMaterial.albedo) {
                        //         const {A, B, G, R} = miraMaterial.albedo;
                        //         if (A && B && G && R)
                        //             hex = A << 24 | R << 16 | G << 8  | B;
                        //     }

                        //     material = new THREE.MeshPhongMaterial({
                        //         color: hex,
                        //         shininess: 0.5,
                        //     });
                        // }

                        const threeMesh = new THREE.Mesh( geometry, material );
                        // threeMesh.receiveShadow = true;
                        // threeMesh.castShadow = true;
                        scene.add(threeMesh);
                        
                        const mat = transforms.get(instance.info!.GUID!)!;
                        
                        console.log(`RENDER [${instance.info!.name!}] -> ${matToString(mat)}`);

                        threeMesh.position.setFromMatrixPosition(mat);
                        threeMesh.rotation.setFromRotationMatrix(mat);
                    }
                }
            }
        })
        initGraphics();
        
        if (refContainer.current) {
            refContainer.current.innerHTML = "";
            refContainer.current.appendChild(renderer.domElement)

            stats = new Stats();
            stats.dom.style.position = 'absolute';
            stats.dom.style.top = '0px';
            refContainer.current.appendChild(stats.dom);
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
