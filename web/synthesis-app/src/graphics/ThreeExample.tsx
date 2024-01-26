// SOURCE: https://dev.to/omher/how-to-start-using-react-and-threejs-in-a-few-minutes-2h6g
import * as THREE from "three";

import { useEffect, useRef, useState } from "react";
// import { wasmWrapper } from '../WasmWrapper.mjs';
import React from "react";
import { PhysicsManager } from "../physics/PhysicsManager.tsx";
import * as AppTest from "../App.tsx";
import { Translations } from "../util/Translations.tsx";
import RAPIER from "@dimforge/rapier3d-compat";
import DetailsPanel from "../components/Details.tsx";

var staticCube: THREE.Mesh;
var mainBar: THREE.Mesh;
var lightBar: THREE.Mesh;
var heavyBar: THREE.Mesh;

var scene: THREE.Scene;

var lastBallSpawned: number = Date.now();
var lastFrameCheck: number = Date.now();
var numFrames: number = 0;

var balls: Array<[THREE.Mesh, number]> = new Array();

var materials = [
    new THREE.MeshPhongMaterial({
        color: 0xe32b50,
        shininess: 0.5,
    }),
    new THREE.MeshPhongMaterial({
        color: 0x4ccf57,
        shininess: 0.5,
    }),
    new THREE.MeshPhongMaterial({
        color: 0xcf4cca,
        shininess: 0.5,
    }),
    new THREE.MeshPhongMaterial({
        color: 0x8c37db,
        shininess: 0.5,
    }),
    new THREE.MeshPhongMaterial({
        color: 0xbddb37,
        shininess: 0.5,
    })
];

var matIndex = 0;

var physicsInit = false;

var spawnBalls = true;

function createBall(
    radius: number,
    position: RAPIER.Vector3,
    velocity: RAPIER.Vector3,
    restitution: number
) {
    matIndex++;
    var ballGeo = new THREE.SphereGeometry(radius);
    var mesh = new THREE.Mesh(ballGeo, materials[matIndex % materials.length]);
    mesh.receiveShadow = true;
    mesh.castShadow = true;
    // scene.add(mesh);
    var phys = PhysicsManager.getInstance().makeBall(position, radius);
    phys.setLinvel(velocity, true);
    phys.collider(0).setRestitution(restitution);
    balls.push([mesh, phys.handle]);
}

function createBox(
    halfExtents: RAPIER.Vector3,
    position: RAPIER.Vector3,
    velocity: RAPIER.Vector3,
    restitution: number
) {
    matIndex++;
    var ballGeo = new THREE.BoxGeometry(halfExtents.x * 2.0, halfExtents.y * 2.0, halfExtents.z * 2.0);
    var mesh = new THREE.Mesh(ballGeo, materials[matIndex % materials.length]);
    mesh.receiveShadow = true;
    mesh.castShadow = true;
    // scene.add(mesh);
    var phys = PhysicsManager.getInstance().makeBox(position, halfExtents);
    phys.setLinvel(velocity, true);
    phys.collider(0).setRestitution(restitution);
    balls.push([mesh, phys.handle]);
}

function makeStaticBox(position: THREE.Vector3, extents: THREE.Vector3) {
    var groundGeo = new THREE.BoxGeometry(extents.x, extents.y, extents.z);
    var groundMat = new THREE.MeshPhongMaterial({
        color: 0xeeeeee,
        shininess: 0.1,
    });
    var ground = new THREE.Mesh(groundGeo, groundMat);
    ground.receiveShadow = true;
    ground.position.set(position.x, position.y, position.z);

    scene.add(ground);
}

function MyThree() {

    const [ballCount, setBallCount] = useState<number>(0);
    const [fps, setFps] = useState<number>(0.0);

    const refContainer = useRef<HTMLDivElement>(null);
    useEffect(() => {

        console.log('MyThree effect');

        // === THREE.JS CODE START ===
        scene = new THREE.Scene();
        var camera = new THREE.PerspectiveCamera(
            75,
            window.innerWidth / window.innerHeight,
            0.1,
            1000
        );
        var renderer = new THREE.WebGLRenderer();
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        renderer.setClearColor(0x121212);

        // document.body.appendChild( renderer.domElement );
        // use ref as a mount point of the Three.js scene instead of the document.body
        if (refContainer.current) {
            refContainer.current.innerHTML = "";
            refContainer.current.appendChild(renderer.domElement);
            console.log("Added dom element");
        }

        // var geometry = new THREE.BoxGeometry(1.0, 1.0, 1.0);
        // var material = new THREE.MeshPhongMaterial({
        //     color: 0xe32b50,
        //     shininess: 0.1,
        // });

        var [X, Y] = [30.0, 30.0];

        makeStaticBox(new THREE.Vector3(0.0, -0.25, 0.0), new THREE.Vector3(X, 0.5, Y));
        // makeStaticBox(new THREE.Vector3((-X / 2.0) + 0.25, -1.0, 0.0), new THREE.Vector3(0.5, 2.0, Y));
        // makeStaticBox(new THREE.Vector3((X / 2.0) - 0.25, -1.0, 0.0), new THREE.Vector3(0.5, 2.0, Y));
        // makeStaticBox(new THREE.Vector3(0.0, -1.0, (-Y / 2.0) + 0.25), new THREE.Vector3(X, 2.0, 0.5));
        // makeStaticBox(new THREE.Vector3(0.0, -1.0, (Y / 2.0) - 0.25), new THREE.Vector3(X, 2.0, 0.5));

        var directionalLight = new THREE.DirectionalLight(0xffffff, 3.0);
        directionalLight.position.set(-1.0, 3.0, 2.0);
        directionalLight.castShadow = true;

        var ambientLight = new THREE.AmbientLight(0xffffff, 0.2);
        scene.add(directionalLight, ambientLight);

        camera.position.set(6.0, 5.0, 6.0);
        camera.rotateY(3.14159 * 0.25);
        camera.rotateX(-0.5);

        var shadowMapSize = Math.min(1024, renderer.capabilities.maxTextureSize);
        var shadowCamSize = 15;

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
        directionalLight.shadow.normalBias = -0.05;

        // directionalLight.shadow.camera = new THREE.OrthographicCamera()

        // directionalLight.shadow.camera.position.copy(camera.position);
        // directionalLight.shadow.camera.matrix.copy(camera.matrix);

        var animate = function () {
            requestAnimationFrame(animate);

            // var pos: THREE.Vector3 = new THREE.Vector3();
            // var rot: THREE.Quaternion = new THREE.Quaternion();
            // var scale: THREE.Vector3 = new THREE.Vector3();

            // ObjTransform.decompose(pos, rot, scale);

            // cube.position.set(pos.x, pos.y, pos.z);
            // cube.rotation.setFromQuaternion(rot);

            renderer.render(scene, camera);
        };
        animate();
    }, []);

    useEffect(() => {
        var frameReq: number | undefined = undefined;

        async function physicsStuff() {
            var update = function () {

                var delta = Date.now() - lastFrameCheck;
                lastFrameCheck = Date.now();
                setFps(1000.0 / delta);

                frameReq = requestAnimationFrame(update);
                
                if (spawnBalls && Date.now() - lastBallSpawned > 10) {
                    lastBallSpawned = Date.now();

                    var randPos = {
                        x: Math.random() * 8.0 - 4.0,
                        y: 10.0,
                        z: Math.random() * 8.0 - 4.0
                    }

                    if (Math.random() > 0.3) {
                        createBall(
                            Math.random() * 0.3 + 0.1,
                            randPos,
                            new RAPIER.Vector3(
                                Math.random() * 0.5 - 0.25,
                                0.0,
                                Math.random() * 0.5 - 0.25
                            ),
                            Math.random() * 0.3
                        );
                    } else {
                        createBox(
                            { x: 0.25, y: 0.25, z: 0.25 },
                            randPos,
                            { x: 0.0, y: 0.0, z: 0.0 },
                            Math.random() * 0.3
                        );
                    }
                    setBallCount(balls.length);

                    if (balls.length % 200 == 0) {
                        spawnBalls = false;
                    }
                }

                PhysicsManager.getInstance().step(delta / 1000.0);

                // for (var i = 0; i < balls.length; i++) {
                //     var [mesh, handle] = balls[i];
                //     var body = PhysicsManager.getInstance().getBody(handle);

                //     (body) && Translations.loadMeshWithRigidbody(body, mesh);
                // }
            };

            frameReq = requestAnimationFrame(update);
        }

        physicsStuff();

        return () => {
            if (frameReq) {
                cancelAnimationFrame(frameReq);
                console.log("Canceling animation");
            }
            // wasmWrapper.coreDestroy();
        };
    }, []);

    console.log('MyThree Component');

    return (
        <div>
            <div ref={refContainer}></div>
            < DetailsPanel ballCount={ballCount} fps={fps} toggleSpawning={() => { spawnBalls = !spawnBalls; }} />
        </div>
    );
}

export default MyThree;
