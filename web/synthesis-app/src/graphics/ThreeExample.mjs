// SOURCE: https://dev.to/omher/how-to-start-using-react-and-threejs-in-a-few-minutes-2h6g
import * as THREE from 'three';

import { useEffect, useRef, useState } from "react";
import { wasmWrapper } from '../WasmWrapper.mjs';

export var Position = new THREE.Vector3(0.0, 0.0, 0.0);

function MyThree() {

  /** @type {React.MutableRefObject<HTMLCanvasElement>} */
  const refContainer = useRef(null);
  useEffect(() => {
    // === THREE.JS CODE START ===
    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    var renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);

    // document.body.appendChild( renderer.domElement );
    // use ref as a mount point of the Three.js scene instead of the document.body
    if (refContainer.current) {
      refContainer.current.replaceChildren([]);
      refContainer.current.appendChild( renderer.domElement )
    }
    console.log("Added dom element");

    var geometry = new THREE.BoxGeometry(1, 1, 1);
    var material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    var cube = new THREE.Mesh(geometry, material);
    scene.add(cube);
    
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

    async function physicsStuff() {
      await wasmWrapper.wrapperPromise;

      wasmWrapper.coreInit();

      var ball = wasmWrapper.physicsCreateBall();

      var update = function () {
        requestAnimationFrame(update);

        wasmWrapper.physicsStep(1.0 / 60.0, 1);
        var pos = wasmWrapper.physicsGetPosition(ball);
        Position.set(pos[0], pos[1], pos[2]);
      }
      update();
    }

    physicsStuff();

    return () => {
      wasmWrapper.coreDestroy();
    }
  }, []);

  return (
    <div ref={refContainer}></div>
  );
}

export default MyThree
