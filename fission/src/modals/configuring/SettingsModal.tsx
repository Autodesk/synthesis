import React from "react"
import { useModalControlContext } from "../../ModalContext"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaGear } from "react-icons/fa6"
import Label, { LabelSize } from "../../components/Label"
import Dropdown from "../../components/Dropdown"
import Button from "../../components/Button"
import Slider from "../../components/Slider"
import Checkbox from "../../components/Checkbox"

const SettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal name="Settings" icon={<FaGear />} modalId={modalId}>
            <Label size={LabelSize.Medium}>Screen Settings</Label>
            <Dropdown label="Screen Mode" options={["Windowed", "Fullscreen"]} onSelect={() => {}} />
            <Dropdown label="Quality Settings" options={["Low", "Medium", "High", "Ultra"]} onSelect={() => {}} />
            <Button value="Theme Editor" onClick={() => openModal("theme-editor")} />
            <Label size={LabelSize.Medium}>Camera Settings</Label>
            <Slider
                min={1}
                max={15}
                defaultValue={15}
                label={"Zoom Sensitivity"}
                format={{ maximumFractionDigits: 2 }}
            />
            <Slider
                min={1}
                max={15}
                defaultValue={10}
                label={"Pitch Sensitivity"}
                format={{ maximumFractionDigits: 2 }}
            />
            <Slider min={1} max={15} defaultValue={3} label={"Yaw Sensitivity"} format={{ maximumFractionDigits: 2 }} />
            <Label size={LabelSize.Medium}>Preferences</Label>
            <Checkbox label="Report Analytics" defaultState={false} />
            <Checkbox label="Use Metric" defaultState={true} />
            <Checkbox label="Render Score Zones" defaultState={false} />
        </Modal>
    )
}

export default SettingsModal
