import { Panel } from "@/components/Panel";
import { useContext, useEffect } from "react";
import type React from "react";
import { ThemeContext } from "./ThemeProvider";
import { UIContext } from "./UIProvider";
import ConfigurePanel from "./panels/configuring/assembly-config/ConfigurePanel";
import MirabufCachingService, {
	MirabufCacheInfo,
	MiraType,
} from "@/mirabuf/MirabufLoader";
import World from "@/systems/World";
import { createMirabuf } from "@/mirabuf/MirabufSceneObject";
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem";
import { Modal } from "./components/Modal";

export type UIRendererProps = object; // TODO: add actual props or delete

export const UIRenderer: React.FC<UIRendererProps> = () => {
	const {
		modal,
		openModal: _openModal,
		panels,
		openPanel,
		addToast,
	} = useContext(UIContext);

	const {
		mode,
		toggleColorMode,
		primaryColor,
		secondaryColor,
		setPrimaryColor,
		setSecondaryColor,
	} = useContext(ThemeContext);

	// useEffect(() => {
	//     openModal(<MainMenuModal />)
	// }, [])

	// TODO: figure this out

	// TODO: remove default panel
	// biome-ignore lint/correctness/useExhaustiveDependencies: adding deps will trigger a refresh loop
	useEffect(() => {
		console.log(
			`opening test panel ${openPanel(
				<ConfigurePanel />,
				undefined,
				"top-right",
				{
					onClose: () => console.log("closed test panel"),
					onAccept: () => addToast("success", "ACCEPTED!"),
					onCancel: () => addToast("success", "CANCELED!"),
				},
			)}`,
		);
	}, []);
	return (
		<>
			<div
				id="panel-container"
				className="relative pointer-events-none w-[100vw] h-[100vh]"
			>
				{panels.map((p, _i) => (
					<Panel key={`panel-${p.id}`} panel={p}>
						{p.content}
					</Panel>
				))}
			</div>
			<Modal modal={modal}>{modal?.content}</Modal>
		</>
	);
};
