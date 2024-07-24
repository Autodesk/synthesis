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
import WPILibBrain, { PWMGroup, simMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver from "@/systems/simulation/driver/Driver"

const RCConfigPWMGroupModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedDrivers, setCheckedDrivers] = useState<Driver[]>([])

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
        // TODO: set brain elsewhere when sim mode is better
        brain = new WPILibBrain(mechanism)
        simLayer?.SetBrain(brain)
    }

    let devices: [string, unknown][] = []
    let pwms;
    if ((pwms = simMap.get("PWM")) != undefined) {
        devices = [...pwms.entries()].filter(([_, data]) => data["<init"])
    }

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                // no eslint complain
                brain.addSimOutputGroup(new PWMGroup(name, checkedPorts, checkedDrivers))
                console.log(name, checkedPorts, checkedDrivers)
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
                                label={p}
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
                                label={`${driver.constructor.name} ${driver.info?.name && "(" + driver.info!.name + ")"}`}
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

export default RCConfigPWMGroupModal
