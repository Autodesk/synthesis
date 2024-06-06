import React, { useEffect, useReducer, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button";
import Label, { LabelSize } from "@/components/Label";
import World from "@/systems/World";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import getHubs, { Hub } from "@/aps/Hubs";
import getProjects, { Project } from "@/aps/Projects";

interface ItemCardProps {
    id: string;
    name: string;
    onClick: () => void;
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, onClick }) => {
    return (
        <div key={id} className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2">
            <Label className="text-wrap break-all">{name}</Label>
            <Button
                value=">"
                onClick={onClick}
            />
        </div>
    )
}

const ImportMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

    const [selectedHub, setSelectedHub] = useState<Hub | undefined>(undefined)
    const [selectedProject, setSelectedProject] = useState<Project | undefined>(undefined)

    const [hubs, setHubs] = useState<Hub[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            setHubs(await getHubs())
        })()
    }, [])

    const [projects, setProjects] = useState<Project[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (selectedHub) {
                setProjects(await getProjects(selectedHub))
            }
        })()
    }, [selectedHub])

    return (
        <Modal
            name={"Manage Assemblies"}
            icon={<FaPlus />}
            modalId={modalId}
            onAccept={() => {
                    // showTooltip("controls", [
                    //     { control: "WASD", description: "Drive" },
                    //     { control: "E", description: "Intake" },
                    //     { control: "Q", description: "Dispense" },
                    // ]);
                }
            }
        >
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                {
                    !selectedHub
                        ? hubs?.map(x => ItemCard({ name: x.name, id: x.id, onClick: () => setSelectedHub(x) }))
                        : !selectedProject
                            ? projects?.map(x => ItemCard({ name: x.name, id: x.id, onClick: () => setSelectedProject(x) }))
                            : <Label>Selected Project '{selectedProject.name}'</Label>
                }
            </div>
        </Modal>
    )
}

export default ImportMirabufModal