import React, { useContext } from "react";
import { globalAddToast } from "@/components/GlobalUIControls.ts";
import { CloseType, UIContext } from "../UIProvider";
import { ModalImplProps } from "../components/Modal";
import { Button, Stack } from "@mui/material";

const MainMenuModal: React.FC<
	ModalImplProps & { startSingleplayerCallback: () => void }
> = ({ modal, parent, startSingleplayerCallback }) => {
	const { closeModal } = useContext(UIContext);
	// TODO: disallow clickaway
    // TODO: hide buttons

	return (
		<Stack>
			<Button
				value={"Singleplayer"}
				onClick={() => {
					closeModal(CloseType.Accept);
					startSingleplayerCallback();
				}}
                fullWidth={true}
				className="my-1"
			/>
			<Button
				value={"Multiplayer"}
				onClick={() => {
					globalAddToast(
						"error",
						"Not Supported\nMultiplayer is not yet supported. Come back soon!",
					);
				}}
                fullWidth={true}
				className="mt-1 mb-3"
			/>
		</Stack>
	);
};

export default MainMenuModal;
