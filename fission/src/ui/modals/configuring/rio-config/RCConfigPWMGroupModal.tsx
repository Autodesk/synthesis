import React, { useContext, useEffect, useState } from "react";
import WPILibBrain, {
	getSimMap,
} from "@/systems/simulation/wpilib_brain/WPILibBrain";
import { PWMOutputGroup } from "@/systems/simulation/wpilib_brain/SimOutput";
import World from "@/systems/World";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import Driver from "@/systems/simulation/driver/Driver";
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain";
import { SynthesisIcons } from "@/ui/components/StyledComponents";
import { ModalImplProps } from "@/ui/components/Modal";
import { UIContext } from "@/ui/UIProvider";
import RoboRIOModal from "../RoboRIOModal";
import { Checkbox, FormControlLabel, Stack, TextField, Typography } from "@mui/material";
import { Box } from "@mui/system";

const RCConfigPWMGroupModal: React.FC<ModalImplProps<void>> = ({ modal, parent }) => {
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

	let devices: [string, unknown][] = [];
	const pwms = getSimMap()?.get(SimType.PWM);
	if (pwms) {
		devices = [...pwms.entries()].filter(([_, data]) => data.get("<init"));
	}

	useEffect(() => {
		modal!.props.onAccept = () => {
			brain.addSimOutput(
				new PWMOutputGroup(name, checkedPorts, checkedDrivers),
			);
			console.log(name, checkedPorts, checkedDrivers);
		};
        modal!.props.onCancel = () => {
            openModal(<RoboRIOModal />, modal)
        }
	}, [name, checkedPorts, checkedDrivers]);

	return (
        <>
			<Typography variant="h6">Name</Typography>
			<TextField placeholder="..." className="w-full" onChange={e => setName(e.target.value)} />
			<Stack
				direction="row"
				className="w-full min-w-full"
			>
				<Box className="w-max">
					<Typography>Ports</Typography>
					<ScrollView className="h-full px-2">
						{devices.map(([p, _]) => (
                            <FormControlLabel label={p} control={
							<Checkbox
								key={p}
								defaultChecked={false}
								onChange={(e) => {
                                    const checked = e.target.checked
									const port = parseInt(p);
									if (checked && !checkedPorts.includes(port)) {
										setCheckedPorts([...checkedPorts, port]);
									} else if (!checked && checkedPorts.includes(port)) {
										setCheckedPorts(checkedPorts.filter((a) => a !== port));
									}
								}}
							/>} />
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

export default RCConfigPWMGroupModal;
