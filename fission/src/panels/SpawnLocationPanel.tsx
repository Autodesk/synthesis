import { useTooltipControlContext } from "@/TooltipContext"
import Button from "../components/Button"
import Panel, { PanelPropsImpl } from "../components/Panel"

const SpawnLocationsPanel: React.FC<PanelPropsImpl> = ({
    panelId,
    openLocation,
    sidePadding,
}) => {
    const robotsPerAlliance = 3
    const alliances = 2

    const { showTooltip } = useTooltipControlContext()
    showTooltip("controls", [
        { control: "Scroll", description: "Rotate Robot" },
        { control: "Shift", description: "Hold to Snap" },
    ])

    return (
        <Panel
            name="Set Spawn Locations"
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
        >
            <table>
                <tbody>
                    {Array(alliances)
                        .fill(0)
                        .map((n, i) => (
                            <tr key={`${n}-${i}`}>
                                {Array(robotsPerAlliance)
                                    .fill(0)
                                    .map((o: number, j: number) => (
                                        <td className="p-2" key={`${o}-${j}`}>
                                            <Button
                                                value={`${["Red", "Blue"][i]} ${
                                                    j + 1
                                                }`}
                                                className="w-32 h-16"
                                                colorOverrideClass={`bg-match-${
                                                    ["red", "blue"][i]
                                                }-alliance`}
                                            />
                                        </td>
                                    ))}
                            </tr>
                        ))}
                </tbody>
            </table>
        </Panel>
    )
}

export default SpawnLocationsPanel
