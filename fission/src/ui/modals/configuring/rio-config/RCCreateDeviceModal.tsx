import { MenuItem, Select } from "@mui/material";
import type React from "react";
import { useContext, useEffect, useState } from "react";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain";
import World from "@/systems/World";
import type { ModalImplProps } from "@/ui/components/Modal";
import { UIContext, useUIContext } from "@/ui/UIProvider";
import RoboRIOModal from "../RoboRIOModal";
import RCConfigCANGroupModal from "./RCConfigCANGroupModal";
import RCConfigEncoderModal from "./RCConfigEncoderModal";
import RCConfigPWMGroupModal from "./RCConfigPWMGroupModal";

type DeviceType = "PWM" | "CAN" | "Encoder";

const RCCreateDeviceModal: React.FC<ModalImplProps<void>> = ({ modal, parent }) => {
	const { openModal } = useUIContext()
	const [type, setType] = useState<DeviceType>("PWM");

	useEffect(() => {
		modal!.props.onAccept = () => {
			console.log(type);
			const miraObjs = [...World.sceneRenderer.sceneObjects.entries()].filter(
				(x) => x[1] instanceof MirabufSceneObject,
			);
			if (miraObjs.length > 0) {
				const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism;
				const simLayer = World.simulationSystem.getSimulationLayer(mechanism);
				console.log("simlayer", simLayer);
				if (!(simLayer?.brain instanceof WPILibBrain))
					simLayer?.setBrain(
						new WPILibBrain(miraObjs[0][1] as MirabufSceneObject),
					);
			}
			switch (type) {
				case "PWM":
					openModal(<RCConfigPWMGroupModal />, modal);
					break;
				case "CAN":
					openModal(<RCConfigCANGroupModal />, modal);
					break;
				case "Encoder":
					openModal(<RCConfigEncoderModal />, modal);
					break;
				default:
					break;
			}
		};
		modal!.props.onCancel = () => openModal(<RoboRIOModal />, modal);
	}, []);

	return (
		<Select
			label={"Type"}
			onChange={(e) => {
				setType(e.target.value as DeviceType);
			}}
		>
			{["PWM", "CAN", "Encoder"].map((t) => (
				<MenuItem key={t} value={t}>{t}</MenuItem>
			))}
		</Select>
	);
};

export default RCCreateDeviceModal;
