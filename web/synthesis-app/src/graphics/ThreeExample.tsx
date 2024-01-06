// SOURCE: https://dev.to/omher/how-to-start-using-react-and-threejs-in-a-few-minutes-2h6g
import * as THREE from 'three';

import { useEffect, useRef } from "react";
// import { wasmWrapper } from '../WasmWrapper.mjs';
import React from 'react';

export var Position = new THREE.Vector3(0.0, 0.0, 0.0);

function MyThree() {

  /** @type {React.MutableRefObject<HTMLCanvasElement>} */
  const refContainer = useRef<HTMLDivElement>(null);
  useEffect(() => {
    // === THREE.JS CODE START ===
    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    var renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);

    // document.body.appendChild( renderer.domElement );
    // use ref as a mount point of the Three.js scene instead of the document.body
    if (refContainer.current) {
      refContainer.current.innerHTML = '';
      refContainer.current.appendChild( renderer.domElement )
    }
    console.log("Added dom element");

    var geometry = new THREE.TorusGeometry(1.0);
    var material = new THREE.MeshPhongMaterial({ color: 0x59f081, shininess: 0.1 });
    var cube = new THREE.Mesh(geometry, material);
    scene.add(cube);

    var pointLight = new THREE.PointLight(0xffffff, 3, 9.0);
    pointLight.translateY(2.0);
    pointLight.translateZ(1.0);
    pointLight.translateX(1.0);
    var ambientLight = new THREE.AmbientLight(0xffffff, 0.05);
    scene.add(pointLight, ambientLight);
    
    camera.position.z = 5;
    
    var animate = function () {
      requestAnimationFrame(animate);
      cube.rotation.x += 0.01;
      cube.rotation.y += 0.01;

      cube.position.set(Position.x, Position.y, Position.z);
      renderer.render(scene, camera);
    };
    animate();

  }, []);

  useEffect(() => {

    var frameReq: number | undefined = undefined;

    async function physicsStuff() {
      // await wasmWrapper.wrapperPromise;

      // wasmWrapper.coreInit();

      // var ball = wasmWrapper.physicsCreateBall();

      var update = function () {
        frameReq = requestAnimationFrame(update);

        // wasmWrapper.physicsStep(1.0 / 30.0, 2);
        // var pos = wasmWrapper.physicsGetPosition(ball);
        Position.set(0.0, 0.0, 0.0);

        // console.log(pos);
      }
      frameReq = requestAnimationFrame(update);
    }

    physicsStuff();

    return () => {
      if (frameReq) {
        cancelAnimationFrame(frameReq);
        console.log("Canceling animation");
      }
      // wasmWrapper.coreDestroy();
    }
  }, []);

  return (
    <div ref={refContainer}></div>
  );
}

export default MyThree
