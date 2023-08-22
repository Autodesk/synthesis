import { useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "../../components/Panel"
import SelectButton from "../../components/SelectButton"
import Slider from "../../components/Slider"

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({
    panelId,
}) => {
    const defaultZoneSize = 0.5
    const [node, setNode] = useState<string>("Click to select")
    const [zoneSize, setZoneSize] = useState<number>(defaultZoneSize)

    return (
        <Panel
            name="Configure Pickup"
            icon={<FaGear />}
            panelId={panelId}
            onAccept={() => {
                // send zone config
            }}
        >
            <SelectButton onSelect={setNode} placeholder="Select pickup node" />
            <Slider
                min={0.1}
                max={1}
                defaultValue={defaultZoneSize}
                label="Zone Size"
                format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={setZoneSize}
            />
        </Panel>
    )
}

export default ConfigureGamepiecePickupPanel
