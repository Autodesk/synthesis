import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import { FaPlus } from "react-icons/fa6"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import Checkbox from "@/components/Checkbox"
import Container from "@/components/Container"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import WPILibBrain, { CANGroup, simMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver from "@/systems/simulation/driver/Driver"

const RCConfigCANGroupModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([])

    const numPorts = 8
    let drivers: Driver[] = []
    let simLayer;
    let brain: WPILibBrain;

    const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(
        x => x[1] instanceof MirabufSceneObject
    )
    console.log(`Number of mirabuf scene objects: ${miraObjs.length}`)
    if (miraObjs.length > 0) {
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
        drivers = simLayer?.drivers ?? []
        brain = new WPILibBrain(mechanism)
        simLayer?.SetBrain(brain)
    }

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                // no eslint complain
                brain.addSimOutputGroup(new CANGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
                const replacer = (key, value) => {
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
                        {[...Array(numPorts).keys()].map(p => (
                            <Checkbox
                                key={p}
                                label={p.toString()}
                                defaultState={false}
                                onClick={checked => {
                                    if (checked && !checkedPorts.includes(p)) {
                                        setCheckedPorts([...checkedPorts, p])
                                    } else if (!checked && checkedPorts.includes(p)) {
                                        setCheckedPorts(checkedPorts.filter(a => a != p))
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
