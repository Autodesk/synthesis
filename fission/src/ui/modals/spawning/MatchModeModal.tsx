import Dropdown from "@/components/Dropdown"
import Label, { LabelSize } from "@/components/Label"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const MatchModeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const robotsPerAlliance = 3
    const robots = ["Dozer_v9.mira", "Team_2471_2018_v7.mira", "None"]
    const fields = [
        "FRC Field 2018_v12.mira",
        "FRC Field 2019_v10.mira",
        "FRC Field 2020-21_v4.mira",
        "FRC Field 2022_v4.mira",
        "FRC_Field_2023_v7.mira",
    ]
    // biome-ignore-start lint/correctness/useJsxKeyInIterable: This file is unused but I can't figure out why these dropdowns exist like this
    return (
        <Modal
            name="Field and Robot Selection"
            icon={SynthesisIcons.GEAR}
            modalId={modalId}
            acceptName="Load"
            cancelEnabled={false}
            onAccept={() => {}}
        >
            <Label size={LabelSize.LARGE}>Select Red Robots</Label>
            {Array(robotsPerAlliance)
                .fill(0)
                .map(() => (
                    <Dropdown options={robots} onSelect={() => {}} />
                ))}
            <Label size={LabelSize.LARGE}>Select Blue Robots</Label>
            {Array(robotsPerAlliance)
                .fill(0)
                .map(() => (
                    <Dropdown options={robots} onSelect={() => {}} />
                ))}
            <Label size={LabelSize.LARGE}>Select Field</Label>
            <Dropdown options={fields} onSelect={() => {}} />
        </Modal>
    )
    // biome-ignore-end lint/correctness/useJsxKeyInIterable: This file is unused but I can't figure out why these dropdowns exist like this
}

export default MatchModeModal
