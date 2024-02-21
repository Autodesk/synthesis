import './App.css';
import MyThree from './graphics/ThreeExample.tsx';
import React, { useEffect, useState } from 'react';
import { PhysicsManager } from './physics/PhysicsManager.tsx';

import RAPIER from "@dimforge/rapier3d-compat";

import { mirabuf as Mirabuf } from './proto/mirabuf.js';
import DetailsPanel from './components/Details.tsx';

function initPhysicsScene() {
	
	PhysicsManager.killInstance();
	PhysicsManager.getInstance();

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
