import type React from "react";
import { useContext } from "react";
import { Panel } from "@/components/Panel";
import { Modal } from "./components/Modal";
import { UIContext, useUIContext } from "./UIProvider";

export const UIRenderer: React.FC = () => {
	const { modal, panels } = useUIContext()

	return (
		<>
			<div
				id="panel-container"
				className="relative pointer-events-none w-[100vw] h-[100vh]"
			>
				{panels.map((p, _i) => (
					<Panel key={`panel-${p.id}`} panel={p} props={p.props}>
						{p.content}
					</Panel>
				))}
			</div>
			{modal && <Modal modal={modal} props={modal.props}>{modal.content}</Modal>}
		</>
	);
};
