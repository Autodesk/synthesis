import type React from "react";
import { useContext, type ReactNode } from "react";
import {
	UIContext,
	CloseType,
	type Panel as PanelType,
	PanelPosition,
} from "../UIProvider";
import { Button, Card, CardContent, CardActions } from "@mui/material";
import Draggable from "react-draggable";

interface PanelProps {
	children?: ReactNode;
	panel: PanelType;
}

// TODO: I don't like this
const HALF_W = "calc(50vw - 50%)";
const HALF_H = "calc(50vh - 50%)";
const FULL_W = "calc(100vw - 100%)";
const FULL_H = "calc(100vh - 100%)";

// TODO: optimize?
const getPositionOffset = (position: PanelPosition) => {
	switch (position) {
		case "top-left":
			return { x: 0, y: 0 };
		case "top":
			return { x: HALF_W, y: 0 };
		case "top-right":
			return { x: FULL_W, y: 0 };
		case "left":
			return { x: 0, y: HALF_H };
		case "right":
			return { x: FULL_W, y: HALF_H };
		case "bottom-left":
			return { x: 0, y: FULL_H };
		case "bottom":
			return { x: HALF_W, y: FULL_H };
		case "bottom-right":
			return { x: FULL_W, y: FULL_H };
		default:
			return { x: HALF_W, y: HALF_H };
	}
};

export const Panel: React.FC<PanelProps> = ({ children, panel }) => {
	const { closePanel } = useContext(UIContext);
	return (
		<Draggable
			positionOffset={getPositionOffset(panel.position)}
		>
			<Card
				sx={{
					position: "absolute",
					maxWidth: 400,
					pointerEvents: "auto",
					p: 4,
				}}
			>
				<CardContent>
					<div className="modal-contents">{children}</div>
				</CardContent>
				<CardActions>
					<Button
						onClick={() => closePanel(panel.id, CloseType.Cancel)}
						variant="outlined"
						color="secondary"
					>
						Close
					</Button>
					<Button
						onClick={() => closePanel(panel.id, CloseType.Accept)}
						variant="contained"
						color="primary"
					>
						Accept
					</Button>
				</CardActions>
			</Card>
		</Draggable>
	);
};
