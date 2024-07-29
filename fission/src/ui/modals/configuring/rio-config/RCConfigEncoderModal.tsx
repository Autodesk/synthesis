import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import { FaPlus } from "react-icons/fa6"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import Dropdown from "@/components/Dropdown"
import NumberInput from "@/components/NumberInput"
import Driver from "@/systems/simulation/driver/Driver"
import WPILibBrain, { simMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Stimulus from "@/systems/simulation/stimulus/Stimulus"
import EncoderStimulus from "@/systems/simulation/stimulus/EncoderStimulus"

const RCConfigEncoderModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [name, setName] = useState<string>("")

    let stimuli: EncoderStimulus[] = []
    let simLayer
    let brain: WPILibBrain

    const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
    if (miraObjs.length > 0) {
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
        stimuli = simLayer?.stimuli.filter(s => s instanceof EncoderStimulus) ?? []
        brain = simLayer?.brain as WPILibBrain
    }

    let devices: [string, unknown][] = []

    const encoders = simMap.get("CANEncoder")
    if (encoders) {
        devices = [...encoders.entries()]
    }

    const stimMap: { [key: string]: Stimulus } = {}

    stimuli.forEach(stim => {
        const label = `${stim.constructor.name} ${stim.info?.name && "(" + stim.info!.name + ")"}`
        stimMap[label] = stim
    })

    const [selectedDevice, setSelectedDevice] = useState<string>(devices[0] && devices[0][0])
    const [selectedStimulus, setSelectedStimulus] = useState<Stimulus | undefined>(stimuli[0])
    const [conversionFactor, setConversionFactor] = useState<number>(1)

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                console.log(name, selectedDevice, selectedStimulus)
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input placeholder="..." className="w-full" onInput={setName} />
            <Dropdown
                label="Encoders"
                options={devices.map(n => n[0])}
                onSelect={s => setSelectedDevice(s)}
            />
            <Dropdown
                label="Stimuli"
                options={Object.keys(stimMap)}
                onSelect={s => setSelectedStimulus(stimMap[s])}
            />
            <NumberInput
                placeholder="Conversion Factor"
                defaultValue={conversionFactor}
                label="Conversion Factor"
                onInput={n => {
                    setConversionFactor(n || 0)
                }}
            />
        </Modal>
    )
}

export default RCConfigEncoderModal
