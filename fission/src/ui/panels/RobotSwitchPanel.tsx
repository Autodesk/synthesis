import { Button, Stack, Typography } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import StatefulCheckbox from "@/components/StatefulCheckbox.tsx"
import Label from "../components/Label"
import { useUIContext } from "../helpers/UIProviderHelpers"
import { PanelImplProps } from "../components/Panel"

const RobotSwitchPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
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
                        <FormControlLabel
                            label={name}
                            control={
                                <Radio
                                    checked={i === selected}
                                    className="whitespace-nowrap"
                                    onChange={() => setSelected(i)}
                                    key={name}
                                />
                            }
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
