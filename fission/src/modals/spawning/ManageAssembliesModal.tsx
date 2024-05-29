import React, { RefObject, useEffect, useRef } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import ScrollView from "@/components/ScrollView"
import Button from "@/components/Button";
import Label, { LabelSize } from "@/components/Label";
import World from "@/systems/World";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";

interface AssemblyCardProps {
    id: number;
}

const AssemblyCard: React.FC<AssemblyCardProps> = ({ id }) => {
    return (
        <div key={id} className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2">
            <Label className="text-wrap break-all">{id}</Label>
            <Button
                value="Delete"
                onClick={() => World.SceneRenderer.RemoveSceneObject(id)}
            />
        </div>
    )
}

const ManageAssembliesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

    const assemblies = [...World.SceneRenderer.sceneObjects.entries()].filter(x => { const y = (x[1] instanceof MirabufSceneObject); return y }).map(x => x[0])

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
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">{assemblies ? `${assemblies.length} Assemblies` : 'No Assemblies'}</Label>
                {
                    assemblies.map(x => AssemblyCard({id: x}))
                }
            </div>
        </Modal>
    )
}

export default ManageAssembliesModal