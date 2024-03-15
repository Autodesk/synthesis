import './App.css';
import Scene from './components/Scene.tsx';
import { useEffect } from 'react';
import GetSceneRenderer from './systems/scene/SceneRenderer.ts';
import MyThree from './graphics/JoltExample.tsx';

function App() {

	// useEffect(() => {

	// 	const mainLoop = () => {
	// 		requestAnimationFrame(mainLoop);
	
	// 		GetSceneRenderer().Update();
	// 	};
	// 	mainLoop();
	// }, []);

	// return <Scene useStats={true} />

	return <MyThree />;
}

export default App;
