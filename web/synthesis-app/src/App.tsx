import './App.css';
import MyThree from './graphics/ThreeExample.tsx';
import React, { useEffect, useState } from 'react';
import { PhysicsManager } from './physics/PhysicsManager.tsx';

import RAPIER from "@dimforge/rapier3d-compat";

function App() {
	console.log("App executed");

	const [rapier, setRapier] = useState<boolean>(false);

	useEffect(() => {
		RAPIER.init().then(() => { setRapier(true); });
	}, []);

	if (rapier) {
		PhysicsManager.killInstance();
		PhysicsManager.getInstance();

		var ball = PhysicsManager.getInstance().makeBall(new RAPIER.Vector3(0.0, 3.0, 0.0), 0.5);
		var col = ball.collider(0);
		col.setRestitution(0.7);
		col.setRestitutionCombineRule(RAPIER.CoefficientCombineRule.Max);

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
