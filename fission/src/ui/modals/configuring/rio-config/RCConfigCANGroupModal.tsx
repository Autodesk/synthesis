import { Box, Stack, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type Driver from "@/systems/simulation/driver/Driver"
import { CANOutputGroup } from "@/systems/simulation/wpilib_brain/SimOutput"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import ScrollView from "@/ui/components/ScrollView"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"

const RCConfigCANGroupModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([])

    let drivers: Driver[] = []
    let simLayer
    let brain: WPILibBrain | undefined

    const miraObj = World.sceneRenderer.mirabufSceneObjects.getRobots()[0]
    if (miraObj != null) {
        const mechanism = miraObj.mechanism
        simLayer = World.simulationSystem.getSimulationLayer(mechanism)
        drivers = simLayer?.drivers ?? []
        brain = simLayer?.brain as WPILibBrain
    }

    const cans = getSimMap()?.get(SimType.CAN_MOTOR) ?? new Map<string, Map<string, number>>()
    const devices: [string, Map<string, number | boolean | string>][] = [...cans.entries()]
        .filter(([_, data]) => data.get("<init"))
        .reverse()

    useEffect(() => {
        const onBeforeAccept = () => {
            if (brain) {
                brain.addSimOutput(new CANOutputGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
            }
        }
        const onCancel = () => {
            openModal(RoboRIOModal, undefined, modal)
        }

        configureScreen(modal!, { title: "Create Device", acceptText: "Done" }, { onBeforeAccept, onCancel })
    }, [brain, name, checkedPorts, checkedDrivers, openModal, modal])

    return (
        <>
            <Label size="sm">Name</Label>
            <TextField placeholder="..." className="w-full" onChange={e => setName(e.target.value)} />
            <Stack direction="row" className="w-full min-w-full">
                <Box className="w-max">
                    <Label size="md">Ports</Label>
                    <ScrollView>
                        {devices.map(([p, _]) => (
                            <Checkbox
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
                    <Label size="md">Signals</Label>
                    <ScrollView>
                        {drivers.map((driver, idx) => (
                            <Checkbox
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
