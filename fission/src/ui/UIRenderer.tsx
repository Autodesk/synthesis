import { useContext, useEffect } from "react";
import { CloseType, UIContext } from "./UIProvider";
import type React from "react";
import { Drawer } from "@mui/material";
import { Modal } from "@/components/Modal";
import { Panel } from "@/components/Panel";

export type UIRendererProps = object; // TODO: add actual props or delete

export const UIRenderer: React.FC<UIRendererProps> = () => {
	const { modal, openModal, closeModal, panels, openPanel, closePanel } =
		useContext(UIContext);
	useEffect(() => {
		console.log(
			`opening test panel ${openPanel(<p>test</p>, "top-right", {
				onClose: () => console.log("closed test panel"),
				onAccept: () => console.log("ACCEPTED!"),
				onCancel: () => console.log("CANCELED!"),
			})}`,
		);
	}, []);
	console.log(panels);
	return (
		<>
			<div id="panel-container" className="relative pointer-events-none w-[100vw] h-[100vh]">
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
