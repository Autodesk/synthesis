import React, { useEffect, useState } from "react";
import { SynthesisIcons } from "@/ui/components/StyledComponents";
import { MenuItem, Select } from "@mui/material";
import { ModalImplProps } from "@/ui/components/Modal";

type DrivetrainType = "None" | "Tank" | "Arcade" | "Swerve";

const DrivetrainModal: React.FC<ModalImplProps> = ({ modal, parent }) => {
	// const { showTooltip } = useTooltipControlContext();
	const [drivetrain, setDrivetrain] = useState<DrivetrainType>("None");

	// const controls: { [key in DrivetrainType]: TooltipControl[] } = {
	//     None: [{ control: "None", description: "Cannot Move" }],
	//     Tank: [
	//         { control: "WS", description: "Drivetrain Left" },
	//         { control: "IK", description: "Drivetrain Right" },
	//         { control: "E", description: "Intake" },
	//         { control: "Q", description: "Dispense" },
	//     ],
	//     Arcade: [
	//         { control: "WASD", description: "Drive" },
	//         { control: "E", description: "Intake" },
	//         { control: "Q", description: "Dispense" },
	//     ],
	//     Swerve: [
	//         { control: "WASD", description: "Drive" },
	//         { control: "< >", description: "Turn" },
	//         { control: "E", description: "Intake" },
	//         { control: "Q", description: "Dispense" },
	//     ],
	// }

	useEffect(() => {
		// TODO: show tooltip `controls[drivetrain]`
	}, []);

	return (
		<Select
			label="Type"
			value={drivetrain}
			onChange={(e) => setDrivetrain(e.target.value as DrivetrainType)}
		>
			{["None", "Tank", "Arcade", "Swerve"].map((opt) => (
				<MenuItem value={opt}>{opt}</MenuItem>
			))}
		</Select>
	);
};

export default DrivetrainModal;
