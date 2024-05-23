import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import World from "@/systems/World"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import { useModalControlContext } from "@/ModalContext"
import Label from "@/components/Label"
import Button from "@/components/Button"

interface FieldCardProps {
    field: string;
    select: (field: string) => void;
}

const FieldCard: React.FC<FieldCardProps> = ({ field, select }) => {
    return (
        <div className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2">
            <Label className="text-wrap break-all">{field}</Label>
            <Button
                value="Spawn"
                onClick={() => select(field)}
            />
        </div>
    )
}

const FieldsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { closeModal } = useModalControlContext()

    const fields = ["FRC_Field_2018_v14.mira"]

    const selectField = (field: string) => {
        CreateMirabufFromUrl(`test_mira/${field}`).then(x => {
            if (x) {
                World.SceneRenderer.RegisterSceneObject(x)
            }
        })

        closeModal()
    }

    return (
        <Modal
            name={"Field Selection"}
            icon={<FaPlus />}
            modalId={modalId}
            acceptEnabled={false}
        >
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                {fields.map(x => FieldCard({field: x, select: selectField}))}
            </div>
        </Modal>
    )
}

export default FieldsModal
