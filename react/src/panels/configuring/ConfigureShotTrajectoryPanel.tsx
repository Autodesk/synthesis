import { useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "../../components/Panel"
import SelectButton from "../../components/SelectButton"
import Slider from "../../components/Slider"

const ConfigureShotTrajectoryPanel: React.FC<PanelPropsImpl> = ({
    panelId,
}) => {
    const defaultShootSpeed = 5;
    const [node, setNode] = useState<string>("Click to select");
    const [shootSpeed, setShootSpeed] = useState<number>(defaultShootSpeed);

    return (
        <Panel
            name="Configure Shooting"
            icon={<FaGear />}
            panelId={panelId}
            onAccept={() => {
                // send node and speed config
            }}
        >
            <SelectButton placeholder="Select shoot node" onSelect={setNode} />
            <Slider
                min={0}
                max={10}
                defaultValue={defaultShootSpeed}
                label="Speed"
                format={{ maximumFractionDigits: 2 }}
                onChange={setShootSpeed}
            />
        </Panel>
    )
}

export default ConfigureShotTrajectoryPanel
