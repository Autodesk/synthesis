import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Button, { ButtonSize } from "../../components/Button"
import { useModalControlContext } from "@/ui/ModalContext"
import Label, { LabelSize } from "@/components/Label"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import { MiraType } from "@/mirabuf/MirabufLoader"

interface MirabufEntry {
    displayName: string
    src: string
}

interface MirabufCardProps {
    entry: MirabufEntry
    select: (entry: MirabufEntry) => void
}

const MirabufCard: React.FC<MirabufCardProps> = ({ entry, select }) => {
    return (
        <div
            key={entry.src}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{entry.displayName}</Label>
            <Button value="Spawn" onClick={() => select(entry)} />
        </div>
    )
}

export const AddRobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closeModal } = useModalControlContext()

    const [remoteRobots, setRemoteRobots] = useState<MirabufEntry[] | null>(null)

    useEffect(() => {
        ;(async () => {
            fetch("/api/mira/manifest.json")
                .then(x => x.json())
                .then(x => {
                    const robots: MirabufEntry[] = []
                    for (const src of x["robots"]) {
                        if (typeof src == "string") {
                            robots.push({
                                src: `/api/mira/Robots/${src}`,
                                displayName: src,
                            })
                        } else {
                            robots.push({
                                src: src["src"],
                                displayName: src["displayName"],
                            })
                        }
                    }
                    setRemoteRobots(robots)
                })
        })()
    }, [])

    const selectRobot = (entry: MirabufEntry) => {
        console.log(`Mira: '${entry.src}'`)
        showTooltip("controls", [
            { control: "WASD", description: "Drive" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ])

        CreateMirabufFromUrl(entry.src, MiraType.ROBOT).then(x => {
            if (x) {
                World.SceneRenderer.RegisterSceneObject(x)
            }
        })

        closeModal()
    }

    return (
        <Modal name={"Robot Selection"} icon={<FaPlus />} modalId={modalId} acceptEnabled={false}>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteRobots ? `${remoteRobots.length} Default Robots` : "No Default Robots"}
                </Label>
                {remoteRobots ? remoteRobots!.map(x => MirabufCard({ entry: x, select: selectRobot })) : <></>}
            </div>
        </Modal>
    )
}

export const AddFieldsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { closeModal } = useModalControlContext()

    const [remoteFields, setRemoteFields] = useState<MirabufEntry[] | null>(null)

    useEffect(() => {
        ;(async () => {
            fetch("/api/mira/manifest.json")
                .then(x => x.json())
                .then(x => {
                    const fields: MirabufEntry[] = []
                    for (const src of x["fields"]) {
                        if (typeof src == "string") {
                            fields.push({
                                src: `/api/mira/Fields/${src}`,
                                displayName: src,
                            })
                        } else {
                            fields.push({
                                src: src["src"],
                                displayName: src["displayName"],
                            })
                        }
                    }
                    setRemoteFields(fields)
                })
        })()
    }, [])

    const selectField = (entry: MirabufEntry) => {
        console.log(`Mira: '${entry.src}'`)
        CreateMirabufFromUrl(entry.src, MiraType.FIELD).then(x => {
            if (x) {
                World.SceneRenderer.RegisterSceneObject(x)
            }
        })

        closeModal()
    }

    return (
        <Modal name={"Field Selection"} icon={<FaPlus />} modalId={modalId} acceptEnabled={false}>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteFields ? `${remoteFields.length} Default Fields` : "No Default Fields"}
                </Label>
                {remoteFields ? remoteFields!.map(x => MirabufCard({ entry: x, select: selectField })) : <></>}
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
