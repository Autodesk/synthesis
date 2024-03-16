import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import ScrollView from "@/components/ScrollView"

const ManageAssembliesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

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
            <ScrollView maxHeight="max-h-70vh" className="h-64" children={
                <div>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                    <h1>Hello</h1>
                </div>
            }>
            </ScrollView>
        </Modal>
    )
}

export default ManageAssembliesModal