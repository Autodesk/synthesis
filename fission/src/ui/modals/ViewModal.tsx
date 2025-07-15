import { MenuItem, Select } from "@mui/material";
import type React from "react";
import { useEffect, useState } from "react";
import type { ModalImplProps } from "../components/Modal";

type ViewType = "Orbit" | "Freecam" | "Overview" | "Driver Station";

const ViewModal: React.FC<ModalImplProps> = ({ modal }) => {
	// TODO:
	// const { showTooltip } = useTooltipControlContext()
	const [view, setView] = useState<ViewType>("Orbit");

	// const controls: { [key in ViewType]: TooltipControl[] } = {
	//     "Orbit": [
	//         { control: "LMB + Drag", description: "Orbit Camera" },
	//         { control: "Scroll", description: "Zoom Camera" },
	//     ],
	//     "Freecam": [
	//         { control: "RMB + Drag", description: "Rotate Camera" },
	//         { control: "RMB + WASD", description: "Move Camera" },
	//         { control: "Scroll", description: "Zoom Camera" },
	//     ],
	//     "Overview": [{ control: "None", description: "Cannot Move Camera" }],
	//     "Driver Station": [
	//         { control: "RMB + Drag", description: "Rotate Camera" },
	//         { control: "RMB + WASD", description: "Move Camera" },
	//         { control: "Scroll", description: "Zoom Camera" },
	//     ],
	// }

	useEffect(() => {
		modal!.props.onAccept = () => {
            console.log("Selected view:", view)
            // TODO: show tooltip for controls
        }
	}, [modal, view]);

	return (
		<Select
			value={view}
			onChange={(e) => {
				setView(e.target.value as ViewType);
			}}
			label={"Camera View"}
		>
        {["Orbit", "Freecam", "Overview", "Driver Station"].map(opt => <MenuItem key={opt} value={opt}>{opt}</MenuItem>)}
        </Select>
	);
};

export default ViewModal;
