import './App.css';
// import MyThree from './graphics/ThreeExample.tsx';
import MyThree from './graphics/JoltExample.tsx'
import React, { useEffect, useState } from 'react';

import { mirabuf as Mirabuf } from './proto/mirabuf.js';
import DetailsPanel from './components/Details.tsx';

function App() {
	console.log("App executed");

	const [rapier, setRapier] = useState<boolean>(false);

	useEffect(() => {
		setRapier(true);
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

export default App;
