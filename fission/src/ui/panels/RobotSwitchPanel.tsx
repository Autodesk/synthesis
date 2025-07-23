import React, { useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Stack, { StackDirection } from "@/components/Stack"
import Button from "@/components/Button"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { SynthesisIcons } from "../components/StyledComponents"
import StatefulCheckbox from "@/components/StatefulCheckbox.tsx"

const RobotSwitchPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [robots, setRobots] = useState(["Dozer_v9_0", "Team 2471 (2018) v7_0"])
    const [selected, setSelected] = useState(0)
    const { openModal } = useModalControlContext()
    return (
        <Panel
            name={"MultiBot"}
            icon={SynthesisIcons.PEOPLE}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
        >
            <Label size={LabelSize.MEDIUM}>MultiBot</Label>
            <form>
                <fieldset>
                    {robots.map((name: string, i: number) => (
                        // fixme: new checkbox
                        <StatefulCheckbox
                            label={name}
                            checked={i == selected}
                            className="whitespace-nowrap"
                            onClick={() => setSelected(i)}
                            key={i}
                        />
                    ))}
                </fieldset>
            </form>
            <Stack direction={StackDirection.HORIZONTAL}>
                <Button value="Add" onClick={() => openModal("robots")} />
                <Button value="Remove" onClick={() => setRobots(robots.filter(r => r !== robots[selected]))} />
            </Stack>
        </Panel>
    )
}

export default RobotSwitchPanel
