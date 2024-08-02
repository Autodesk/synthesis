import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import Checkbox from "@/components/Checkbox"
import Container from "@/components/Container"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import WPILibBrain, { CANGroup, simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver from "@/systems/simulation/driver/Driver"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const RCConfigCANGroupModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([])

    let drivers: Driver[] = []
    let simLayer
    let brain: WPILibBrain

    const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
    console.log(`Number of mirabuf scene objects: ${miraObjs.length}`)
    if (miraObjs.length > 0) {
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
        drivers = simLayer?.drivers ?? []
        brain = new WPILibBrain(mechanism)
        simLayer?.SetBrain(brain)
    }

    const cans = simMap.get(SimType.CANMotor) ?? new Map<string, any>
    const devices: [string, any][] = [...cans.entries()].filter(([_, data]) => data["<init"])

    return (
        <Modal
            name="Create Device"
            icon={SynthesisIcons.Add}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                // no eslint complain
                brain.addSimOutputGroup(new CANGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
                const replacer = (_: unknown, value: unknown) => {
                    if (value instanceof Map) {
                        return Object.fromEntries(value)
                    } else {
                        return value
                    }
                }
                console.log(JSON.stringify(simMap, replacer))
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input placeholder="..." className="w-full" onInput={setName} />
            <Stack direction={StackDirection.Horizontal} className="w-full min-w-full">
                <Container className="w-max">
                    <Label>Ports</Label>
                    <ScrollView className="h-full px-2">
                        {devices.map(([p, _]) => (
                            <Checkbox
                                key={p}
                                label={p.toString()}
                                defaultState={false}
                                onClick={checked => {
                                    const port = parseInt(p)
                                    if (checked && !checkedPorts.includes(port)) {
                                        setCheckedPorts([...checkedPorts, port])
                                    } else if (!checked && checkedPorts.includes(port)) {
                                        setCheckedPorts(checkedPorts.filter(a => a != port))
                                    }
                                }}
                            />
                        ))}
                    </ScrollView>
                </Container>
                <Container className="w-max">
                    <Label>Signals</Label>
                    <ScrollView className="h-full px-2">
                        {drivers.map((driver, idx) => (
                            <Checkbox
                                key={`${driver.constructor.name}-${idx}`}
                                label={driver.constructor.name}
                                defaultState={false}
                                onClick={checked => {
                                    if (checked && !checkedDrivers.includes(driver)) {
                                        setCheckedDrivers([...checkedDrivers, driver])
                                    } else if (!checked && checkedDrivers.includes(driver)) {
                                        setCheckedDrivers(checkedDrivers.filter(a => a != driver))
                                    }
                                }}
                            />
                        ))}
                    </ScrollView>
                </Container>
            </Stack>
        </Modal>
    )
}

export default RCConfigCANGroupModal
