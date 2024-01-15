// SOURCE: https://dev.to/omher/how-to-start-using-react-and-threejs-in-a-few-minutes-2h6g
import * as THREE from "three";

import { useEffect, useRef } from "react";
// import { wasmWrapper } from '../WasmWrapper.mjs';
import React from "react";
import { PhysicsManager } from "../physics/PhysicsManager.tsx";
import * as AppTest from "../App.tsx";
import { Translations } from "../util/Translations.tsx";

var cube: THREE.Mesh;

function MyThree() {
    const refContainer = useRef<HTMLDivElement>(null);
    useEffect(() => {
        // === THREE.JS CODE START ===
        var scene = new THREE.Scene();
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

        var groundGeo = new THREE.BoxGeometry(10.0, 0.5, 10.0);
        var groundMat = new THREE.MeshPhongMaterial({
            color: 0x287aed,
            shininess: 0.1,
        });
        var ground = new THREE.Mesh(groundGeo, groundMat);
        ground.receiveShadow = true;
        ground.position.set(0.0, -2.0, 0.0);

        var geometry = new THREE.BoxGeometry(1.0, 1.0, 1.0);
        var material = new THREE.MeshPhongMaterial({
            color: 0xe32b50,
            shininess: 0.1,
        });
        cube = new THREE.Mesh(geometry, material);
        cube.receiveShadow = true;
        cube.castShadow = true;
        scene.add(cube, ground);

        // var pointLight = new THREE.PointLight(0xffffff, 3, 9.0);
        // pointLight.translateY(2.0);
        // pointLight.translateZ(1.0);
        // pointLight.translateX(1.0);

        var directionalLight = new THREE.DirectionalLight(0xffffff, 3.0);
        directionalLight.position.set(-1.0, 3.0, 2.0);
        directionalLight.castShadow = true;

        var ambientLight = new THREE.AmbientLight(0xffffff, 0.2);
        scene.add(directionalLight, ambientLight);

        camera.position.set(5.0, 0.0, 5.0);
        camera.rotateY(3.14159 * 0.25);
        camera.rotateX(-0.2);

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
                frameReq = requestAnimationFrame(update);

                PhysicsManager.getInstance().step();

                var body = PhysicsManager.getInstance().getBody(0);

                if (body) {
                    cube.matrixAutoUpdate = false;
                    cube.matrixWorld.compose(new THREE.Vector3(1.0, -3.0, 1.0), new THREE.Quaternion(), new THREE.Vector3(1.0, 1.0, 1.0));
                    cube.matrixWorldNeedsUpdate = true;
                    // Translations.loadMeshWithRigidbody(body, cube);
                } else {
                    // ObjTransform = new THREE.Matrix4();
                }

                // console.log(pos);
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

    return <div ref={refContainer}></div>;
}

export default MyThree;
