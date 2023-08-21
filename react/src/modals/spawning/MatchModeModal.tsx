import { FaGear } from "react-icons/fa6"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import Dropdown from "../../components/Dropdown";
import Label, { LabelSize } from "../../components/Label";

const MatchModeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const robotsPerAlliance = 3;
    const robots = ["Dozer_v9.mira", "Team_2471_2018_v7.mira", "None"];
    const fields = [
        "FRC Field 2018_v12.mira",
        "FRC Field 2019_v10.mira",
        "FRC Field 2020-21_v4.mira",
        "FRC Field 2022_v4.mira",
        "FRC_Field_2023_v7.mira",
    ]

    return (
        <Modal
            name="Field and Robot Selection"
            icon={<FaGear />}
            modalId={modalId}
            acceptName="Load"
            cancelEnabled={false}
            onAccept={() => { }}
        >
            <Label size={LabelSize.Large}>Select Red Robots</Label>
            {Array(robotsPerAlliance).fill(0).map(() => (
                <Dropdown
                    options={robots}
                    onSelect={() => { }}
                />))
            }
            <Label size={LabelSize.Large}>Select Blue Robots</Label>
            {Array(robotsPerAlliance).fill(0).map(() => (
                <Dropdown
                    options={robots}
                    onSelect={() => { }}
                />))
            }
            <Label size={LabelSize.Large}>Select Field</Label>
            <Dropdown
                options={fields}
                onSelect={() => { }}
            />
        </Modal>
    )
}

export default MatchModeModal;
