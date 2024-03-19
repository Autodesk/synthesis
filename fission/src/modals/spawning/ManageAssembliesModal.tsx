import React, { RefObject, useEffect, useRef } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import ScrollView from "@/components/ScrollView"

const ManageAssembliesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

    const refs: Array<RefObject<HTMLDivElement>> = new Array(2);
    for (let i = 0; i < refs.length; i++) {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        refs[i] = useRef<HTMLDivElement>(null);
    }

    useEffect(() => {
        refs.forEach((x, i) => {
            if (x.current) {
                x.current.innerHTML = `Item ${i}`;
            }
        });
    });

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
            <ScrollView maxHeight="max-h-70vh" className="h-32" children={
                <div>
                    {refs.map(x => <div ref={x}></div>)}
                </div>
            }>
            </ScrollView>
        </Modal>
    )
}

export default ManageAssembliesModal