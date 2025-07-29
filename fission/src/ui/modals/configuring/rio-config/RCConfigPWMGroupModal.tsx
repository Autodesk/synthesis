import React, { useEffect, useState } from "react"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import { PWMOutputGroup } from "@/systems/simulation/wpilib_brain/SimOutput"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver from "@/systems/simulation/driver/Driver"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"
import { Stack, TextField } from "@mui/material"
import { Box } from "@mui/system"
import ScrollView from "@/ui/components/ScrollView"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"
import Label from "@/ui/components/Label"

const RCConfigPWMGroupModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
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

    let devices: [string, unknown][] = []
    const pwms = getSimMap()?.get(SimType.PWM)
    if (pwms) {
        devices = [...pwms.entries()].filter(([_, data]) => data.get("<init"))
    }

    useEffect(() => {
        const onBeforeAccept = () => {
            if (brain) {
                brain.addSimOutput(new PWMOutputGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
            }
        }
        const onCancel = () => {
            openModal(<RoboRIOModal />, modal)
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
                            <StatefulCheckbox
                                label={p}
                                key={p}
                                checked={false}
                                onClick={checked => {
                                    const port = parseInt(p)
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

export default RCConfigPWMGroupModal
