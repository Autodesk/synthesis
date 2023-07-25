import React, { useState } from "react"
import Label, { LabelSize } from "../components/Label"
import Panel from "../components/Panel"
import { IoPeople } from "react-icons/io5"
import Checkbox from "../components/Checkbox"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import { useModalControlContext } from "../ModalContext"

const MultiBotPanel: React.FC = () => {
    const [robots, setRobots] = useState([
        "Dozer_v9_0",
        "Team 2471 (2018) v7_0",
    ])
    const [selected, setSelected] = useState(0)
    const { openModal } = useModalControlContext()
    return (
        <Panel name={"MultiBot"} icon={<IoPeople />}>
            <Label size={LabelSize.Medium}>MultiBot</Label>
            {robots.map((name: string, i: number) => (
                <Checkbox
                    label={name}
                    defaultState={i == selected}
                    onClick={() => setSelected(i)}
                />
            ))}
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

export default MultiBotPanel
