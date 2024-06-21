import Button, { ButtonSize } from "@/components/Button"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import { ChangeEvent, useRef, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import { useTooltipControlContext } from "@/TooltipContext"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"

const ImportLocalMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)

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
            name={"Import Local Assemblies"}
            icon={<FaPlus />}
            modalId={modalId}
            acceptEnabled={selectedFile !== undefined}
            onAccept={() => {
                    if (selectedFile) {
                        console.log(`Mira: '${selectedFile}'`)
                        showTooltip("controls", [
                            { control: "WASD", description: "Drive" },
                            { control: "E", description: "Intake" },
                            { control: "Q", description: "Dispense" },
                        ])

                        CreateMirabufFromUrl(URL.createObjectURL(selectedFile)).then(x => {
                            if (x) {
                                World.SceneRenderer.RegisterSceneObject(x)
                            }
                        })
                    }
                }
            }
        >
            <div className="flex flex-col items-center gap-5">
                <input ref={fileUploadRef} onChange={onInputChanged} type="file" hidden={true} />
                <Button value="Upload" size={ButtonSize.Large} onClick={uploadClicked} />
                {
                    selectedFile
                        ? (<Label className="text-center" size={LabelSize.Medium}>{`Selected File: ${selectedFile.name}`}</Label>)
                        : (<></>)
                }
            </div>
        </Modal>
    )
}

export default ImportLocalMirabufModal