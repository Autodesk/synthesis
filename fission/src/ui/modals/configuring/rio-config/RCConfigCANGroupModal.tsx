import {
	Box,
	Checkbox,
	FormControlLabel,
	Stack,
	TextField,
	Typography,
} from "@mui/material";
import type React from "react";
import { useContext, useEffect, useState } from "react";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import type Driver from "@/systems/simulation/driver/Driver";
import { CANOutputGroup } from "@/systems/simulation/wpilib_brain/SimOutput";
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain";
import {
	getSimMap,
	SimType,
} from "@/systems/simulation/wpilib_brain/WPILibBrain";
import World from "@/systems/World";
import type { ModalImplProps } from "@/ui/components/Modal";
import { UIContext } from "@/ui/UIProvider";
import RoboRIOModal from "../RoboRIOModal";

const RCConfigCANGroupModal: React.FC<ModalImplProps<void>> = ({ modal, parent }) => {
	const { openModal } = useContext(UIContext);
	const [name, setName] = useState<string>("");
	const [checkedPorts, setCheckedPorts] = useState<number[]>([]);
	const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([]);

	let drivers: Driver[] = [];
	let simLayer;
	let brain: WPILibBrain;

	const miraObjs = [...World.sceneRenderer.sceneObjects.entries()].filter(
		(x) => x[1] instanceof MirabufSceneObject,
	);
	if (miraObjs.length > 0) {
		const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism;
		simLayer = World.simulationSystem.getSimulationLayer(mechanism);
		drivers = simLayer?.drivers ?? [];
		brain = simLayer?.brain as WPILibBrain;
	}

	const cans =
		getSimMap()?.get(SimType.CAN_MOTOR) ??
		new Map<string, Map<string, number>>();
	const devices: [string, Map<string, number | boolean | string>][] = [
		...cans.entries(),
	]
		.filter(([_, data]) => data.get("<init"))
		.reverse();

	useEffect(() => {
		modal!.props.onAccept = () => {
			brain.addSimOutput(
				new CANOutputGroup(name, checkedPorts, checkedDrivers),
			);
			console.log(name, checkedPorts, checkedDrivers);
		};
		modal!.props.onCancel = () => {
			openModal(<RoboRIOModal />, modal);
		};
	}, []);

	return (
		<>
			<Typography variant="h7">Name</Typography>
			<TextField
				placeholder="..."
				className="w-full"
				onChange={(e) => setName(e.target.value)}
			/>
			<Stack direction="row" className="w-full min-w-full">
				<Box className="w-max">
					<Typography>Ports</Typography>
					<ScrollView className="h-full px-2">
						{devices.map(([p, _]) => (
							<FormControlLabel
								label={p.toString()}
								control={
									<Checkbox
										key={p}
										defaultChecked={false}
										onClick={(checked) => {
											const port = parseInt(p.split("[")[1].split("]")[0]);
											console.log(port);
											if (checked && !checkedPorts.includes(port)) {
												setCheckedPorts([...checkedPorts, port]);
											} else if (!checked && checkedPorts.includes(port)) {
												setCheckedPorts(checkedPorts.filter((a) => a !== port));
											}
										}}
									/>
								}
							/>
						))}
					</ScrollView>
				</Box>
				<Box className="w-max">
					<Typography>Signals</Typography>
					<ScrollView className="h-full px-2">
						{drivers.map((driver, idx) => (
							<Checkbox
								key={`${driver.constructor.name}-${idx}`}
								label={`${driver.constructor.name} ${driver.info?.name && "(" + driver.info!.name + ")"}`}
								defaultState={false}
								onClick={(checked) => {
									if (checked && !checkedDrivers.includes(driver)) {
										setCheckedDrivers([...checkedDrivers, driver]);
									} else if (!checked && checkedDrivers.includes(driver)) {
										setCheckedDrivers(
											checkedDrivers.filter((a) => a !== driver),
										);
									}
								}}
							/>
						))}
					</ScrollView>
				</Box>
			</Stack>
		</>
	);
};

export default RCConfigCANGroupModal;
