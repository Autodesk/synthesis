import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ModalContext"
import { FaPlus } from "react-icons/fa6"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import Dropdown from "@/components/Dropdown"

const RCConfigEncoderModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")
    const [selectedSignal, setSelectedSignal] = useState<string>("")
    const [selectedChannelA, setSelectedChannelA] = useState<number>(0)
    const [selectedChannelB, setSelectedChannelB] = useState<number>(0)
    const [conversionFactor, setConversionFactor] = useState<number>(1)

    const numPorts = 10
    const signals = ["Rev0 (uuid)", "Rev1 (uuid)", "Rev2 (uuid)", "Rev3 (uuid)"]

    if (!selectedSignal) setSelectedSignal(signals[0])

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                // mostly doing this so eslint doesn't complain about unused variables
                console.log(
                    name,
                    selectedSignal,
                    selectedChannelA,
                    selectedChannelB,
                    conversionFactor
                )
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input placeholder="..." className="w-full" onInput={setName} />
            <Dropdown
                label="Signal"
                options={signals}
                onSelect={s => setSelectedSignal(s)}
            />
            <Dropdown
                label="Channel A"
                options={[...Array(numPorts).keys()].map(n => n.toString())}
                onSelect={s => setSelectedChannelA(parseInt(s))}
            />
            <Dropdown
                label="Channel B"
                options={[...Array(numPorts).keys()].map(n => n.toString())}
                onSelect={s => setSelectedChannelB(parseInt(s))}
            />
            <Input
                numeric
                placeholder="Conversion Factor"
                defaultValue={conversionFactor.toString()}
                label="Conversion Factor"
                onInput={n => {
                    setConversionFactor(n != "" ? parseFloat(n) : 0)
                }}
            />
        </Modal>
    )
}

export default RCConfigEncoderModal
