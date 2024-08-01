import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import Dropdown from "@/components/Dropdown"
import NumberInput from "@/components/NumberInput"
import WPILibBrain, { simMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EncoderStimulus from "@/systems/simulation/stimulus/EncoderStimulus"
import { SimEncoderInput } from "@/systems/simulation/wpilib_brain/SimInput"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const RCConfigEncoderModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [_name, setName] = useState<string>("")

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

    const stimMap: { [key: string]: EncoderStimulus } = {}

    stimuli.forEach(stim => {
        const label = `${stim.constructor.name} ${stim.info?.name && "(" + stim.info!.name + ")"}`
        stimMap[label] = stim
    })

    const [selectedDevice, setSelectedDevice] = useState<string>(devices[0] && devices[0][0])
    const [selectedStimulus, setSelectedStimulus] = useState<EncoderStimulus | undefined>(stimuli[0])
    const [conversionFactor, setConversionFactor] = useState<number>(1)

    return (
        <Modal
            name="Create Device"
            icon={SynthesisIcons.Add}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                if (selectedDevice && selectedStimulus)
                    brain.addSimInput(new SimEncoderInput(selectedDevice, selectedStimulus, conversionFactor))
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input placeholder="..." className="w-full" onInput={setName} />
            <Dropdown label="Encoders" options={devices.map(n => n[0])} onSelect={s => setSelectedDevice(s)} />
            <Dropdown label="Stimuli" options={Object.keys(stimMap)} onSelect={s => setSelectedStimulus(stimMap[s])} />
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
