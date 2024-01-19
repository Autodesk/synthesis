// SOURCE: https://dev.to/omher/how-to-start-using-react-and-threejs-in-a-few-minutes-2h6g
import * as THREE from "three";

import { useEffect, useRef } from "react";
// import { wasmWrapper } from '../WasmWrapper.mjs';
import React from "react";
import { PhysicsManager } from "../physics/PhysicsManager.tsx";
import * as AppTest from "../App.tsx";
import { Translations } from "../util/Translations.tsx";

var staticCube: THREE.Mesh;
var mainBar: THREE.Mesh;
var lightBar: THREE.Mesh;
var heavyBar: THREE.Mesh;

function createMeshes(scene: THREE.Scene) {

    var redMaterial = new THREE.MeshPhongMaterial({
        color: 0xe32b50,
        shininess: 0.1,
    });
    var greenMaterial = new THREE.MeshPhongMaterial({
        color: 0x4ccf57,
        shininess: 0.1,
    });
    var pinkMaterial = new THREE.MeshPhongMaterial({
        color: 0xcf4cca,
        shininess: 0.1,
    });

    var staticGeo = new THREE.BoxGeometry(1.0, 1.0, 1.0);
    var barGeo = new THREE.BoxGeometry(0.5, 0.5, 2.0);

    staticCube = new THREE.Mesh(staticGeo, redMaterial);
    staticCube.receiveShadow = true;
    staticCube.castShadow = true;

    mainBar = new THREE.Mesh(barGeo, redMaterial);
    mainBar.receiveShadow = true;
    mainBar.castShadow = true;
    lightBar = new THREE.Mesh(barGeo, greenMaterial);
    lightBar.receiveShadow = true;
    lightBar.castShadow = true;
    heavyBar = new THREE.Mesh(barGeo, pinkMaterial);
    heavyBar.receiveShadow = true;
    heavyBar.castShadow = true;

    scene.add(staticCube, mainBar, lightBar, heavyBar);
}

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
            color: 0xeeeeee,
            shininess: 0.1,
        });
        var ground = new THREE.Mesh(groundGeo, groundMat);
        ground.receiveShadow = true;
        ground.position.set(0.0, -2.0, 0.0);

        // var geometry = new THREE.BoxGeometry(1.0, 1.0, 1.0);
        // var material = new THREE.MeshPhongMaterial({
        //     color: 0xe32b50,
        //     shininess: 0.1,
        // });

        createMeshes(scene);

        scene.add(ground);

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

                var staticBody = PhysicsManager.getInstance().getBody(0);
                var mainBody = PhysicsManager.getInstance().getBody(1);
                var lightBody = PhysicsManager.getInstance().getBody(2);
                var heavyBody = PhysicsManager.getInstance().getBody(3);

                (staticBody) && Translations.loadMeshWithRigidbody(staticBody, staticCube);
                (mainBody) && Translations.loadMeshWithRigidbody(mainBody, mainBar);
                (lightBody) && Translations.loadMeshWithRigidbody(lightBody, lightBar);
                (heavyBody) && Translations.loadMeshWithRigidbody(heavyBody, heavyBar);
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
