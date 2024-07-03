import { useEffect, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import Slider from "@/components/Slider"
import World from "@/systems/World"

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const defaultZoneSize = 0.5
    const [, setNode] = useState<string>("Click to select")
    const [, setZoneSize] = useState<number>(defaultZoneSize)

    useEffect(() => {
        // implementing spherical placeholder for intake placement        
        const mesh = World.SceneRenderer.CreateSphere(5.0)
    })

    return (
        <Panel
            name="Configure Pickup"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
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
