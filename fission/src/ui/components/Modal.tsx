import React, { ReactNode, useContext } from "react";
import {
	Button,
	Card,
	Modal as MUIModal,
	CardContent,
	CardActions,
} from "@mui/material";
import { UIContext, CloseType } from "../UIProvider";
import type { Modal as ModalType } from "../UIProvider";

interface ModalProps {
	children?: ReactNode;
	modal: ModalType;
}

export const Modal: React.FC<ModalProps> = ({ children, modal }) => {
	const { closeModal } = useContext(UIContext);
	return (
		<MUIModal open={modal !== undefined} onClose={closeModal}>
			<Card
				sx={{
					position: "absolute",
					top: "50%",
					left: "50%",
					transform: "translate(-50%, -50%)",
					maxWidth: 400,
					p: 4,
				}}
			>
				<CardContent>
					<div className="modal-contents">{children}</div>
				</CardContent>
				<CardActions>
					<Button
						onClick={() => closeModal(CloseType.Cancel)}
						variant="outlined"
						color="error"
					>
						Close
					</Button>
					<Button
						onClick={() => closeModal(CloseType.Accept)}
						variant="contained"
						color="success"
					>
						Accept
					</Button>
				</CardActions>
			</Card>
		</MUIModal>
	);
};
