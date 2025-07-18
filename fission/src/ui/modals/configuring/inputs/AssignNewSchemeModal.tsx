import { TextField } from "@mui/material";
import type React from "react";
import { useContext, useState } from "react";
import InputSchemeManager from "@/systems/input/InputSchemeManager";
import InputSystem from "@/systems/input/InputSystem";
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain";
import type { ModalImplProps } from "@/ui/components/Modal";
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel";
import { StateContext, useStateContext } from "@/ui/StateProvider";
import { UIContext, useUIContext } from "@/ui/UIProvider";

const AssignNewSchemeModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
	const { openPanel } = useUIContext()
	const { setSelectedScheme, setConfigurationType } = useStateContext()

	const [name, setName] = useState<string>(
		InputSchemeManager.randomAvailableName,
	);

	modal!.props.onAccept = () => {
		const scheme = InputSystem.brainIndexSchemeMap.get(
			SynthesisBrain.brainIndexMap.size - 1,
		);

		if (scheme === undefined) return;

		scheme.schemeName = name;

		InputSchemeManager.addCustomScheme(scheme);
		InputSchemeManager.saveSchemes();

		setConfigurationType("INPUTS");
		setSelectedScheme(scheme);
		openPanel(<ConfigurePanel />, modal);
	};
	modal!.props.hideCancel = true;

	return (
		<TextField
			label="Name"
			placeholder=""
			defaultValue={name}
			onChange={(e) => setName(e.target.value)}
		/>
	);
};

export default AssignNewSchemeModal;
