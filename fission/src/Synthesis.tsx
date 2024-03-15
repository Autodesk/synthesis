import Scene from './components/Scene.tsx';
import { useEffect } from 'react';
import GetSceneRenderer from './systems/scene/SceneRenderer.ts';
import MirabufSceneObject from './mirabuf/MirabufSceneObject.ts';
import { LoadMirabufRemote } from './mirabuf/MirabufLoader.ts';
import { mirabuf } from './proto/mirabuf';
import MirabufParser, { ParseErrorSeverity } from './mirabuf/MirabufParser.ts';
import MirabufInstance from './mirabuf/MirabufInstance.ts';

const DEFAULT_MIRA_PATH = 'test_mira/Team_2471_(2018)_v7.mira';

function Synthesis() {

	let mira_path = DEFAULT_MIRA_PATH;

	const urlParams = new URLSearchParams(document.location.search);

	if (urlParams.has("mira")) {
        mira_path = `test_mira/${urlParams.get("mira")!}`;
        console.debug(`Selected Mirabuf File: ${mira_path}`);
    }
    console.log(urlParams)

	useEffect(() => {

		const setup = async () => {
			const miraAssembly = await LoadMirabufRemote(mira_path)
				.catch(
					_ => LoadMirabufRemote(DEFAULT_MIRA_PATH)
				).catch(console.error);

			if (!miraAssembly || !(miraAssembly instanceof mirabuf.Assembly)) {
				return;
			}
	
			const parser = new MirabufParser(miraAssembly);
			if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
				console.error(`Assembly Parser produced significant errors for '${miraAssembly.info!.name!}'`);
				return;
			}
			
			const mirabufSceneObject = new MirabufSceneObject(new MirabufInstance(parser));
			GetSceneRenderer().RegisterSceneObject(mirabufSceneObject);
		};
		setup();

        let mainLoopHandle = 0;
		const mainLoop = () => {
			mainLoopHandle = requestAnimationFrame(mainLoop);
	
			GetSceneRenderer().Update();
		};
		mainLoop();

        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle);
            GetSceneRenderer().RemoveAllSceneObjects();
        };
	}, []);

	return <Scene useStats={true} />

	// return <MyThree />;
}

export default Synthesis;