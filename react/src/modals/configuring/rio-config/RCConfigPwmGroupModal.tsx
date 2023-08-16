import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { useModalControlContext } from "../../../ModalContext"
import { FaPlus } from "react-icons/fa6"
import ScrollView from "../../../components/ScrollView"
import Stack, { StackDirection } from "../../../components/Stack"
import Checkbox from "../../../components/Checkbox"
import Container from "../../../components/Container"
import Label, { LabelSize } from "../../../components/Label"
import Input from "../../../components/Input"

const RCConfigPwmGroupModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")
    const [checkedPorts, setCheckedPorts] = useState<number[]>([])
    const [checkedSignals, setCheckedSignals] = useState<string[]>([])

    const numPorts = 8
    const signals = ["Rev0 (uuid)", "Rev1 (uuid)", "Rev2 (uuid)", "Rev3 (uuid)"]

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                console.log("Name: ", name)
                console.log("Checked Ports: ", checkedPorts)
                console.log("Checked Signals: ", checkedSignals)
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input
                placeholder="..."
                className="w-full"
                onInput={e => setName(e.target.value)}
            />
            <Stack
                direction={StackDirection.Horizontal}
                className="w-full min-w-full"
            >
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
                                    } else if (
                                        !checked &&
                                        checkedPorts.includes(p)
                                    ) {
                                        setCheckedPorts(
                                            checkedPorts.filter(a => a != p)
                                        )
                                    }
                                }}
                            />
                        ))}
                    </ScrollView>
                </Container>
                <Container className="w-max">
                    <Label>Signals</Label>
                    <ScrollView className="h-full px-2">
                        {signals.map(p => (
                            <Checkbox
                                key={p}
                                label={p.toString()}
                                defaultState={false}
                                onClick={checked => {
                                    if (
                                        checked &&
                                        !checkedSignals.includes(p)
                                    ) {
                                        setCheckedSignals([
                                            ...checkedSignals,
                                            p,
                                        ])
                                    } else if (
                                        !checked &&
                                        checkedSignals.includes(p)
                                    ) {
                                        setCheckedSignals(
                                            checkedSignals.filter(a => a != p)
                                        )
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

export default RCConfigPwmGroupModal
