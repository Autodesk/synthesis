import Button, { ButtonSize } from "@/components/Button"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import { ChangeEvent, useRef, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import World from "@/systems/World"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import Dropdown from "@/ui/components/Dropdown"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"

const ImportLocalMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined >(undefined)

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

    const typeSelected = (type: string) => {
        switch (type) {
            case "Robot" : setSelectedType(MiraType.ROBOT);
            break;
            case "Field" : setSelectedType(MiraType.FIELD);
            break;
        }
    }

    return (
        <Modal
            name={"Import Local Assemblies"}
            icon={<FaPlus />}
            modalId={modalId}
            acceptEnabled={selectedFile !== undefined && miraType !== undefined}
            onAccept={ async () => {
                if (selectedFile && miraType != undefined) {
                    showTooltip("controls", [
                        { control: "WASD", description: "Drive" },
                        { control: "E", description: "Intake" },
                        { control: "Q", description: "Dispense" },
                    ])
                    

                    const hashBuffer = await selectedFile.arrayBuffer()
                    const info = await MirabufCachingService.CacheLocal(hashBuffer, miraType)
                    const assembly = await MirabufCachingService.Get(info!.id, miraType)
                    await CreateMirabuf(assembly!).then(x => {
                            if (x) {
                                World.SceneRenderer.RegisterSceneObject(x)
                            }
                        }).then(() => { 
                            if (!info!.name) MirabufCachingService.SetInfo(info!.cacheKey, miraType, assembly?.info?.name ?? undefined)
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
                <Dropdown
                    label="Type" options={["None","Robot", "Field"]} onSelect={(selected: string) => {typeSelected(selected)}}
                />
            </div>
        </Modal>
    )

}


export default ImportLocalMirabufModal
