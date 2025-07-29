import { MenuItem, Select, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EncoderStimulus from "@/systems/simulation/stimulus/EncoderStimulus"
import { SimEncoderInput } from "@/systems/simulation/wpilib_brain/SimInput"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"
import Label from "@/ui/components/Label"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"

const RCConfigEncoderModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [_name, setName] = useState<string>("")

    let stimuli: EncoderStimulus[] = []
    let simLayer
    let brain: WPILibBrain | undefined

    const miraObjs = [...World.sceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
    if (miraObjs.length > 0) {
        // TODO: make the object selectable
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        simLayer = World.simulationSystem.getSimulationLayer(mechanism)
        stimuli = simLayer?.stimuli.filter(s => s instanceof EncoderStimulus) ?? []
        brain = simLayer?.brain as WPILibBrain
    }

    const devices: [string, unknown][] = [...(getSimMap()?.get(SimType.CAN_ENCODER)?.entries() ?? [])] // ugly

    const stimMap = new Map<string, EncoderStimulus>()

    stimuli.forEach(stim => {
        const label = `${stim.constructor.name} ${stim.info?.name && "(" + stim.info!.name + ")"}`
        stimMap.set(label, stim)
    })

    const [selectedDevice, setSelectedDevice] = useState<string>(devices[0] && devices[0][0])
    const [selectedStimulus, setSelectedStimulus] = useState<EncoderStimulus | undefined>(stimuli[0])

    useEffect(() => {
        const onAccept = () => {
            if (selectedDevice && selectedStimulus && brain)
                brain.addSimInput(new SimEncoderInput(selectedDevice, selectedStimulus))
        }
        const onCancel = () => openModal(<RoboRIOModal />, modal)

        configureScreen(modal!, { title: "Create Device", acceptText: "Done" }, { onAccept, onCancel })
    }, [brain, selectedDevice, selectedStimulus, openModal, modal])

    return (
        <>
            <Label size="sm">Name</Label>
            <TextField placeholder="..." className="w-full" onChange={e => setName(e.target.value)} />
            <Select label="CAN Encoders" onChange={e => setSelectedDevice(e.target.value as string)}>
                {devices.map(d => (
                    <MenuItem key={`encoder-type-${d[0]}`} value={d[0]}>
                        {d[0]}
                    </MenuItem>
                ))}
            </Select>
            <Select label="Stimuli" onChange={e => setSelectedStimulus(stimMap.get(e.target.value as string))}>
                {[...stimMap.keys()].map(s => (
                    <MenuItem key={`stim-type-${s}`} value={s}>
                        {s}
                    </MenuItem>
                ))}
            </Select>
        </>
    )
}

export default RCConfigEncoderModal
