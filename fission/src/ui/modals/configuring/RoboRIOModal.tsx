import { Button, FormControlLabel } from "@mui/material";
import type React from "react";
import { useContext } from "react";
import type { ModalImplProps } from "@/ui/components/Modal";
import { UIContext } from "@/ui/UIProvider";

const RoboRIOModal: React.FC<ModalImplProps> = ({ modal, parent }) => {
	const { openModal } = useContext(UIContext);
	return (
		<FormControlLabel
			label="cbdbcc,ds,vsdv"
			control={
				<Button
					value="Create Device"
					onClick={() => /* TODO: openModal("create-device") */ undefined}
				/>
			}
		/>
	);
};

export default RoboRIOModal;
