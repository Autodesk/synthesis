import React, { useMemo, useReducer } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaWrench } from "react-icons/fa6"
import Button, { ButtonProps } from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { usePanelControlContext } from "@/ui/PanelContext"
import { setSelectedBrainIndexGlobal } from "@/ui/panels/configuring/ChooseInputSchemePanel"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSystem from "@/systems/input/InputSystem"
import { IoTrashBin } from "react-icons/io5"

interface AssemblyCardProps {
    mira: MirabufSceneObject
    update: React.DispatchWithoutAction
}

const ButtonSecondary: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            sizeOverrideClass="px-6 py-2"
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-cancel-button hover:brightness-90"
        ></Button>
    )
}

const AssemblyCard: React.FC<AssemblyCardProps> = ({ mira, update }) => {
    const { openPanel } = usePanelControlContext()
    const { closeModal } = useModalControlContext()

    const brain = useMemo(() => (mira.brain as SynthesisBrain)?.brainIndex, [mira])

    return (
        <div
            key={mira.id}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{`[${InputSystem.brainIndexSchemeMap.get(brain)?.schemeName ?? "-"}] ${mira.assemblyName}`}</Label>
            <div className="flex flex-row gap-4">
                <Button
                    sizeOverrideClass="px-6 py-2"
                    value="Set Scheme"
                    onClick={() => {
                        setSelectedBrainIndexGlobal(brain)
                        openPanel("choose-scheme")
                        closeModal()
                    }}
                />
                <ButtonSecondary
                    value={<IoTrashBin />}
                    onClick={() => {
                        World.SceneRenderer.RemoveSceneObject(mira.id)
                        update()
                    }}
                />
            </div>
        </div>
    )
}

const ManageAssembliesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [_, update] = useReducer(x => !x, false)

    const assemblies = [...World.SceneRenderer.sceneObjects.entries()]
        .filter(x => {
            const y = x[1] instanceof MirabufSceneObject
            return y
        })
        .map(x => x[1] as MirabufSceneObject)

    return (
        <Modal name={"Manage Assemblies"} icon={<FaWrench />} modalId={modalId} acceptEnabled={false} cancelName="Back">
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {assemblies ? `${assemblies.length} Assemblies` : "No Assemblies"}
                </Label>
                {assemblies.map(x => (
                    <AssemblyCard key={x.id} mira={x} update={update} />
                ))}
            </div>
        </Modal>
    )
}

export default ManageAssembliesModal
