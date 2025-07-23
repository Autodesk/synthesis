import { Button, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import StatefulCheckbox from "../components/StatefulCheckbox"

const RobotSwitchPanel: React.FC = () => {
    const [robots, setRobots] = useState(["Dozer_v9_0", "Team 2471 (2018) v7_0"])
    const [selected, setSelected] = useState(0)

    return (
        <>
            <Typography variant="h3">MultiBot</Typography>
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
