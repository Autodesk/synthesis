import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import Checkbox from "@/components/Checkbox.tsx"
import Label from "../components/Label"
import type { PanelImplProps } from "../components/Panel"
import { Button } from "../components/StyledComponents"
import { useUIContext } from "../helpers/UIProviderHelpers"

const RobotSwitchPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [robots, setRobots] = useState(["Dozer_v9_0", "Team 2471 (2018) v7_0"])
    const [selected, setSelected] = useState(0)

    useEffect(() => {
        configureScreen(panel!, { title: "MultiBot" }, {})
    }, [])

    return (
        <>
            <Label size="md">MultiBot</Label>
            <form>
                <fieldset>
                    {robots.map((name: string, i: number) => (
                        // fixme: new checkbox
                        <Checkbox
                            label={name}
                            checked={i == selected}
                            className="whitespace-nowrap"
                            onClick={() => setSelected(i)}
                            key={i}
                        />
                    ))}
                </fieldset>
            </form>
            <Stack direction="row">
                <Button onClick={() => /* TODO: openModal("robots") <- what modal is this??? */ undefined}>Add</Button>
                <Button onClick={() => setRobots(robots.filter(r => r !== robots[selected]))}>Remove</Button>
            </Stack>
        </>
    )
}

export default RobotSwitchPanel
