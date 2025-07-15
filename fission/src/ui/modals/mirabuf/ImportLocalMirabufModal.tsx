import Button, { ButtonSize } from "@/components/Button"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { ChangeEvent, useRef, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import World from "@/systems/World"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"

const ImportLocalMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()
    const { openPanel } = usePanelControlContext()

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>(MiraType.ROBOT)

    const uploadClicked = () => {
        if (fileUploadRef.current) {
            fileUploadRef.current.click()
        }
    }

    const onInputChanged = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]
            setSelectedFile(file)
        }
    }

    return (
        <Modal
            name={"Import From File"}
            icon={SynthesisIcons.IMPORT}
            modalId={modalId}
            acceptEnabled={selectedFile !== undefined && miraType !== undefined}
            onCancel={() => openPanel("import-mirabuf")}
            onAccept={async () => {
                if (selectedFile && miraType != undefined) {
                    showTooltip("controls", [
                        { control: "WASD", description: "Drive" },
                        { control: "E", description: "Intake" },
                        { control: "Q", description: "Dispense" },
                    ])

                    const hashBuffer = await selectedFile.arrayBuffer()
                    World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
                    await MirabufCachingService.cacheAndGetLocalWithInfo(hashBuffer, miraType)
                        .then(result => {
                            if (result) {
                                return createMirabuf(result.assembly, undefined, result.cacheInfo.id)
                            }
                            return undefined
                        })
                        .then(x => {
                            if (x) {
                                World.sceneRenderer.registerSceneObject(x)

                                globalOpenPanel("initial-config")
                            }
                        })
                        .finally(() =>
                            setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
                        )
                }
            }}
        >
            <div className="flex flex-col items-center gap-5">
                <input ref={fileUploadRef} onChange={onInputChanged} type="file" hidden={true} />

                <ToggleButtonGroup
                    value={miraType}
                    exclusive
                    onChange={(_, v) => v != null && setSelectedType(v)}
                    {...SoundPlayer.buttonSoundEffects()}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robot</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Field</ToggleButton>
                </ToggleButtonGroup>
                <Button value="Upload File" size={ButtonSize.LARGE} onClick={uploadClicked} />
                {selectedFile && (
                    <Label
                        className="text-center"
                        size={LabelSize.MEDIUM}
                    >{`Selected File: ${selectedFile.name}`}</Label>
                )}
            </div>
        </Modal>
    )
}

export default ImportLocalMirabufModal
