import { Box, Checkbox, FormControlLabel, Stack, TextField, Typography } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type Driver from "@/systems/simulation/driver/Driver"
import { CANOutputGroup } from "@/systems/simulation/wpilib_brain/SimOutput"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { getSimMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/UIProvider"
import RoboRIOModal from "../RoboRIOModal"
import ScrollView from "@/ui/components/ScrollView"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"

const RCConfigCANGroupModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([])

    let drivers: Driver[] = []
    let simLayer
    let brain: WPILibBrain | undefined

    const miraObjs = [...World.sceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
    if (miraObjs.length > 0) {
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.simulationSystem.getSimulationLayer(mechanism)
        drivers = simLayer?.drivers ?? []
        brain = simLayer?.brain as WPILibBrain
    }

    const cans = getSimMap()?.get(SimType.CAN_MOTOR) ?? new Map<string, Map<string, number>>()
    const devices: [string, Map<string, number | boolean | string>][] = [...cans.entries()]
        .filter(([_, data]) => data.get("<init"))
        .reverse()

    useEffect(() => {
        const onAccept = () => {
            if (brain) {
                brain.addSimOutput(new CANOutputGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
            }
        }
        const onCancel = () => {
            openModal(<RoboRIOModal />, modal)
        }

        configureScreen(modal!, {}, { onAccept, onCancel })
    }, [brain, name, checkedPorts, checkedDrivers, openModal, modal])

    return (
        <>
            <Typography variant="h6">Name</Typography>
            <TextField placeholder="..." className="w-full" onChange={e => setName(e.target.value)} />
            <Stack direction="row" className="w-full min-w-full">
                <Box className="w-max">
                    <Typography>Ports</Typography>
                    <ScrollView>
                        {devices.map(([p, _]) => (
                            <StatefulCheckbox
                                label={p.toString()}
                                key={p}
                                checked={false}
                                onClick={checked => {
                                    const port = parseInt(p.split("[")[1].split("]")[0])
                                    console.log(port)
                                    if (checked && !checkedPorts.includes(port)) {
                                        setCheckedPorts([...checkedPorts, port])
                                    } else if (!checked && checkedPorts.includes(port)) {
                                        setCheckedPorts(checkedPorts.filter(a => a !== port))
                                    }
                                }}
                            />
                        ))}
                    </ScrollView>
                </Box>
                <Box className="w-max">
                    <Typography>Signals</Typography>
                    <ScrollView>
                        {drivers.map((driver, idx) => (
                            <StatefulCheckbox
                                label={`${driver.constructor.name} ${driver.info?.name && "(" + driver.info!.name + ")"}`}
                                key={`${driver.constructor.name}-${idx}`}
                                checked={false}
                                onClick={checked => {
                                    if (checked && !checkedDrivers.includes(driver)) {
                                        setCheckedDrivers([...checkedDrivers, driver])
                                    } else if (!checked && checkedDrivers.includes(driver)) {
                                        setCheckedDrivers(checkedDrivers.filter(a => a !== driver))
                                    }
                                }}
                            />
                        ))}
                    </ScrollView>
                </Box>
            </Stack>
        </>
    )
}

export default RCConfigCANGroupModal
