import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import Label, { LabelSize } from "@/components/Label"
import Input from "@/components/Input"
import Dropdown from "@/components/Dropdown"
import WPILibBrain, { simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
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
        // TODO: make the object selectable
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
        stimuli = simLayer?.stimuli.filter(s => s instanceof EncoderStimulus) ?? []
        brain = simLayer?.brain as WPILibBrain
    }

    const devices: [string, unknown][] = [...(simMap.get(SimType.CANEncoder)?.entries() ?? [])] // ugly

    const stimMap = new Map<string, EncoderStimulus>()

    stimuli.forEach(stim => {
        const label = `${stim.constructor.name} ${stim.info?.name && "(" + stim.info!.name + ")"}`
        stimMap.set(label, stim)
    })

    const [selectedDevice, setSelectedDevice] = useState<string>(devices[0] && devices[0][0])
    const [selectedStimulus, setSelectedStimulus] = useState<EncoderStimulus | undefined>(stimuli[0])

    return (
        <Modal
            name="Create Device"
            icon={SynthesisIcons.Add}
            modalId={modalId}
            acceptName="Done"
            onAccept={() => {
                if (selectedDevice && selectedStimulus)
                    brain.addSimInput(new SimEncoderInput(selectedDevice, selectedStimulus))
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Label size={LabelSize.Small}>Name</Label>
            <Input placeholder="..." className="w-full" onInput={setName} />
            <Dropdown label="Encoders" options={devices.map(n => n[0])} onSelect={s => setSelectedDevice(s)} />
            <Dropdown
                label="Stimuli"
                options={[...stimMap.keys()]}
                onSelect={s => setSelectedStimulus(stimMap.get(s))}
            />
        </Modal>
    )
}

export default RCConfigEncoderModal
