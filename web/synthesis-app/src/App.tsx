// import './App.css';
// // import React, { useEffect, useState } from 'react';
// import { RunJolt } from './jolt/jolt.ts'

// function App() {
// 	RunJolt();
// }

// export default App;

import './App.css';
// import MyThree from './graphics/ThreeExample.tsx';
import MyThree from './graphics/JoltExample.tsx'
import React, { useEffect, useState } from 'react';

// function App() {
// 	console.log("App executed");

// 	// const [rapier, setRapier] = useState<boolean>(false);

// 	useEffect(() => {
// 		// Example of setting up a state variable to track when RAPIER is ready for use
// 		// RAPIER.init().then(() => { setRapier(true); });
// 	}, []);

// 	if (true /* Check for is `rapier` has been set to true before loading ThreeJS and running Physics */) {
		
// 		// Initialize Physics

// 		return (
// 			< MyThree />
// 		);
// 	} else {
// 		return (
// 			<div>{"Loading Physics Engine"}</div>
// 		)
// 	}
// }


function App() {
	return (
		< MyThree />
	);
}

export default App;
