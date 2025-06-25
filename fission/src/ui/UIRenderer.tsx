import { useContext, useEffect } from "react";
import { CloseType, UIContext } from "./UIProvider";
import type React from "react";
import type { ReactElement, ReactNode } from "react";
import { Box, Drawer, Modal as MUIModal, Typography } from "@mui/material";
import { Modal } from "@/components/Modal";

export type UIRendererProps = {};

export const UIRenderer: React.FC<UIRendererProps> = () => {
	const { modal, openModal, closeModal, panels, openPanel, closePanel } =
		useContext(UIContext);
	useEffect(() => {
		console.log(
			`opening test modal ${openModal(<p>test</p>, {
				onClose: () => {
					console.log("closed test modal");
				},
			})}`,
		);
	}, []);
	console.log(modal);
	return (
		<>
			{panels.map((p, i) => (
				<Drawer
					key={p.id}
					anchor="right"
					open={true}
					onClose={() => closePanel(p.id, CloseType.Cancel)}
					variant="persistent"
					sx={{
						width: 320,
						flexShrink: 0,
						"& .MuiDrawer-paper": {
							width: 320,
							boxSizing: "border-box",
							right: i * 320,
						},
					}}
				>
					{p.content}
				</Drawer>
			))}
			<Modal modal={modal}>{modal?.content}</Modal>
		</>
	);
};
