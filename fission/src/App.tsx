import './App.css';
// import MyThree from './graphics/ThreeExample.tsx';
import MyThree from './graphics/JoltExample.tsx'
import { useEffect, useState } from 'react';

// import { mirabuf as Mirabuf } from './proto/mirabuf.js';
// import DetailsPanel from './components/Details.tsx';

function App() {
	const [joltLoaded, setJoltLoaded] = useState<boolean>(false);

	useEffect(() => { (async () => {
		setJoltLoaded(true);
	})()}, []);

	if (joltLoaded) {
		return (
			< MyThree />
		);
	} else {
		return (
			<div>{joltLoaded ? "Jolt has loaded!" : "Jolt is loading..."}</div>
		)
	}
}

export default App;