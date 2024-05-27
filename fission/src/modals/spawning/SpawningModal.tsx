import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Button, { ButtonSize } from "../../components/Button"
import { useModalControlContext } from "../../ModalContext"
import Label from "@/components/Label"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"

interface FieldEntry {
    displayName: string;
    src: string;
}

interface FieldCardProps {
    entry: FieldEntry;
    select: (entry: FieldEntry) => void;
}

const FieldCard: React.FC<FieldCardProps> = ({ entry, select }) => {
    return (
        <div key={entry.src} className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2">
            <Label className="text-wrap break-all">{entry.displayName}</Label>
            <Button
                value="Spawn"
                onClick={() => select(entry)}
            />
        </div>
    )
}

export const AddFieldsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { closeModal } = useModalControlContext()

    const [fields, setFields] = useState<FieldEntry[] | null>(null);

    useEffect(() => {
        (async () => {
            fetch('/api/mira/manifest.json').then(x => x.json()).then(x => {
                const fields: FieldEntry[] = [];
                for (const src of x['fields']) {
                    if (typeof(src) == 'string') {
                        fields.push({ src: `/api/mira/Fields/${src}`, displayName: src })
                    } else {
                        fields.push({ src: src['src'], displayName: src['displayName'] })
                    }
                }
                setFields(fields)
            })
        })()
    }, []);

    const selectField = (entry: FieldEntry) => {
        console.log(`Mira: '${entry.src}'`)
        CreateMirabufFromUrl(entry.src).then(x => {
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
                {fields ? fields.map(x => FieldCard({entry: x, select: selectField})) : 'no fields'}
            </div>
        </Modal>
    )
}

export const SpawningModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal name={"Spawning"} icon={<FaPlus />} modalId={modalId}>
            <Stack direction={StackDirection.Vertical}>
                <Button
                    value={"Robots"}
                    onClick={() => openModal("add-robot")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
                <Button
                    value={"Fields"}
                    onClick={() => openModal("add-field")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
            </Stack>
        </Modal>
    )
}
