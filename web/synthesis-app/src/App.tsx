import './App.css';
import MyThree from './graphics/ThreeExample.tsx';
import React, { useEffect, useState } from 'react';
import { PhysicsManager } from './physics/PhysicsManager.tsx';

import RAPIER from "@dimforge/rapier3d-compat";

function initPhysicsScene() {

	// PhysicsManager.killInstance();
	PhysicsManager.getInstance();

	var staticBody = PhysicsManager.getInstance().makeBox(new RAPIER.Vector3(0.0, 2.0, 0.0), new RAPIER.Vector3(0.5, 0.5, 0.5));
	staticBody.setBodyType(RAPIER.RigidBodyType.Fixed, true);

	var mainBody = PhysicsManager.getInstance().makeBox(new RAPIER.Vector3(0.25, 2.75, 1.5), new RAPIER.Vector3(0.25, 0.25, 1.0));
	mainBody.collider(0).setMass(1.0);
	var lightBody = PhysicsManager.getInstance().makeBox(new RAPIER.Vector3(0.75, 2.75, 3.5), new RAPIER.Vector3(0.25, 0.25, 1.0));
	lightBody.collider(0).setMass(0.1);
	var heavyBody = PhysicsManager.getInstance().makeBox(new RAPIER.Vector3(-0.25, 2.75, 3.5), new RAPIER.Vector3(0.25, 0.25, 1.0));
	heavyBody.collider(0).setMass(100.0);

	var mainJointData = RAPIER.JointData.revolute({ x: 0.25, y: -0.25, z: -1.0 }, { x: 0.5, y: 0.5, z: 0.5 }, { x: 1.0, y: 0.0, z: 0.0 });
	var mainJoint = PhysicsManager.getInstance().createJoint(mainJointData, mainBody, staticBody) as RAPIER.RevoluteMultibodyJoint;
	// mainJoint.setLimits(-3.14159 * 0.5, 3.14159 * 0.5);
	// DmainJoint.setContactsEnabled(false);

	var lightJointData = RAPIER.JointData.revolute({ x: -0.25, y: 0.0, z: -1.0 }, { x: 0.25, y: 0.0, z: 1.0 }, { x: 0.0, y: 1.0, z: 0.0 });
	// var lightJoint = PhysicsManager.getInstance().createJoint(lightJointData, lightBody, mainBody);
	var heavyJointData = RAPIER.JointData.revolute({ x: 0.25, y: 0.0, z: -1.0 }, { x: -0.25, y: 0.0, z: 1.0 }, { x: 0.0, y: 1.0, z: 0.0 });
	// var heavyJoint = PhysicsManager.getInstance().createJoint(heavyJointData, heavyBody, mainBody);
}

var STOP = false;

function App() {
	console.log("App executed");

	const [rapier, setRapier] = useState<boolean>(false);

	useEffect(() => {
		if (!STOP) {
			STOP = true;
			RAPIER.init().then(() => { console.log("Rapier Loaded"); initPhysicsScene(); setRapier(true); });
		}
	}, []);

	if (rapier) {

		return (
			< MyThree />
		);
	} else {
		return (
			<div>{rapier ? "True" : "False"}</div>
		)
	}
}

export var getPosition: () => /*Vector | */undefined = () => undefined;

export default App;
