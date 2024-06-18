import React, { useState } from "react"
import Label, { LabelSize } from "../components/Label"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { IoPeople } from "react-icons/io5"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import { useModalControlContext } from "../ModalContext"
import Checkbox from "../components/Checkbox"

const RobotSwitchPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation }) => {
    const [robots, setRobots] = useState([
        "Dozer_v9_0",
        "Team 2471 (2018) v7_0",
    ])
    const [selected, setSelected] = useState(0)
    const { openModal } = useModalControlContext()
    return (
        <Panel name={"MultiBot"} icon={<IoPeople />} panelId={panelId} openLocation={openLocation}>
            <Label size={LabelSize.Medium}>MultiBot</Label>
            <form>
                <fieldset>
                    {robots.map((name: string, i: number) => (
                        <Checkbox
                            label={name}
                            defaultState={i == selected}
                            className="whitespace-nowrap"
                            onClick={() => setSelected(i)}
                            stateOverride={i == selected}
                        />
                    ))}
                </fieldset>
            </form>
            <Stack direction={StackDirection.Horizontal}>
                <Button value="Add" onClick={() => openModal("robots")} />
                <Button
                    value="Remove"
                    onClick={() =>
                        setRobots(robots.filter(r => r !== robots[selected]))
                    }
                />
            </Stack>
        </Panel>
    )
}

export default RobotSwitchPanel
