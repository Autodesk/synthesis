import Button, { ButtonSize } from "@/components/Button"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { ChangeEvent, useRef, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import World from "@/systems/World"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { mirabuf } from "@/proto/mirabuf"

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
                        .then(x => {
                            if (x) {
                                // TODO This function shouldn't cache game pieces when imported locally!!!
                                return createMirabuf(x.assembly, x.cacheInfo.id, miraType)
                            }
                            return undefined
                        })
                        .then(x => {
                            if (x) {
                                const { mainSceneObject, gamePieces } = x

                                World.sceneRenderer.registerSceneObject(mainSceneObject)
                                gamePieces?.forEach(async instance => {
                                    const assembly = instance.parser.assembly
                                    const buffer = mirabuf.Assembly.encode(assembly).finish()

                                    const cacheInfo = await MirabufCachingService.cacheLocal(buffer, MiraType.PIECE)
                                    if (!cacheInfo) return

                                    if (!cacheInfo.name) {
                                        MirabufCachingService.cacheInfo(
                                            cacheInfo.cacheKey,
                                            MiraType.PIECE,
                                            assembly.info?.name ?? undefined
                                        )
                                    }

                                    const sceneObject = new MirabufSceneObject(
                                        instance,
                                        assembly.info?.name!,
                                        cacheInfo.id
                                    )
                                    World.sceneRenderer.registerSceneObject(sceneObject)
                                })

                                console.log(`Loaded ${mainSceneObject.miraType.toString()} Locally`)
                                if (mainSceneObject.miraType === MiraType.ROBOT) {
                                    globalOpenPanel("initial-config")
                                }
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
                    <ToggleButton value={MiraType.PIECE}>Piece</ToggleButton>
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
