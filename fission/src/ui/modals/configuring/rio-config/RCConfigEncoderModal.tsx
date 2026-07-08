import { FormControl, InputLabel, MenuItem, TextField } from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import type React from "react"
import { useEffect, useState } from "react"
import EncoderStimulus from "@/systems/simulation/stimulus/EncoderStimulus"
import { SimEncoderInput } from "@/systems/simulation/wpilib_brain/sim/SimCANEncoder"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"

const RCConfigEncoderModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [_name, setName] = useState<string>("")

    let stimuli: EncoderStimulus[] = []
    let simLayer
    let brain: WPILibBrain | undefined

    const miraObj = World.sceneRenderer.mirabufSceneObjects.getRobots()[0]
    if (miraObj != null) {
        // TODO: make the object selectable
        const mechanism = miraObj.mechanism
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
        const onBeforeAccept = () => {
            if (selectedDevice && selectedStimulus && brain)
                brain.addSimInput(new SimEncoderInput(selectedDevice, selectedStimulus))
        }
        const onCancel = () => openModal(RoboRIOModal, undefined, modal)

        configureScreen(modal!, { title: "Create Device", acceptText: "Done" }, { onBeforeAccept, onCancel })
    }, [brain, selectedDevice, selectedStimulus, openModal, modal])

    return (
        <>
            <Label size="sm">Name</Label>
            <TextField placeholder="..." className="w-full" onChange={e => setName(e.target.value)} />
            <FormControl fullWidth>
                <InputLabel id="can-encoders-label">CAN Encoders</InputLabel>
                <Select
                    labelId="can-encoders-label"
                    label="CAN Encoders"
                    onChange={e => setSelectedDevice(e.target.value as string)}
                >
                    {devices.map(d => (
                        <MenuItem key={`encoder-type-${d[0]}`} value={d[0]}>
                            {d[0]}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl fullWidth>
                <InputLabel id="stimuli-label">Stimuli</InputLabel>
                <Select
                    labelId="stimuli-label"
                    label="Stimuli"
                    onChange={e => setSelectedStimulus(stimMap.get(e.target.value as string))}
                >
                    {[...stimMap.keys()].map(s => (
                        <MenuItem key={`stim-type-${s}`} value={s}>
                            {s}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
        </>
    )
}

export default RCConfigEncoderModal
