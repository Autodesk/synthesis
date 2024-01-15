import './App.css';
import MyThree from './graphics/ThreeExample.tsx';
import React, { useEffect, useState } from 'react';

function App() {
	console.log("App executed");

	// const [rapier, setRapier] = useState<boolean>(false);

	useEffect(() => {
		// Example of setting up a state variable to track when RAPIER is ready for use
		// RAPIER.init().then(() => { setRapier(true); });
	}, []);

	if (true /* Check for is `rapier` has been set to true before loading ThreeJS and running Physics */) {
		
		// Initialize Physics

		return (
			< MyThree />
		);
	} else {
		return (
			<div>{"Loading Physics Engine"}</div>
		)
	}
}

export default App;
