import { Button, FormControlLabel, Radio, Stack, Typography } from "@mui/material"
import type React from "react"
import { useContext, useState } from "react"
import { UIContext, useUIContext } from "../UIProvider"

const RobotSwitchPanel: React.FC = () => {
    const [robots, setRobots] = useState(["Dozer_v9_0", "Team 2471 (2018) v7_0"])
    const [selected, setSelected] = useState(0)
    const { openModal } = useUIContext()

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
                <Button onClick={() => /* TODO: openModal("robots")*/ undefined}>Add</Button>
                <Button onClick={() => setRobots(robots.filter(r => r !== robots[selected]))}>Remove</Button>
            </Stack>
        </>
    )
}

export default RobotSwitchPanel
