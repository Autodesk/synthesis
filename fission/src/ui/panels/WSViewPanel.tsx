import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SimMapUpdateEvent, SimGeneric, simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { Button, Dropdown } from "@mui/base"
import { Box, MenuItem, Select, Stack, styled, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import { GrConnect } from "react-icons/gr"

type ValueType = "string" | "number" | "object" | "boolean"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
})

function generateTableBody() {
    return (
        <TableBody>
            {simMap.has('PWM') ? [...simMap.get('PWM')!.entries()].filter(x => x[1]["<init"] == true).map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>PWM</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
            {simMap.has('SimDevice') ? [...simMap.get('SimDevice')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>SimDevice</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }): <></>}
            {simMap.has('CANMotor') ? [...simMap.get('CANMotor')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>CAN Motor</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
            {simMap.has('CANEncoder') ? [...simMap.get('CANEncoder')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>CAN Encoder</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
        </TableBody>
    )
}

function setGeneric(simType: SimType, device: string, field: string, value: string, valueType: ValueType) {
    switch (valueType) {
        case "number":
            SimGeneric.Set(simType, device, field, parseFloat(value))
            break
        case "object":
            SimGeneric.Set(simType, device, field, JSON.parse(value))
            break
        case "boolean":
            SimGeneric.Set(simType, device, field, value.toLowerCase() == "true")
            break
        default:
            SimGeneric.Set(simType, device, field, value)
            break
    }
}

const WSViewPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {

    const [tb, setTb] = useState(generateTableBody())

    const [selectedType, setSelectedType] = useState<SimType | undefined>()
    const [selectedDevice, setSelectedDevice] = useState<string | undefined>()
    const [field, setField] = useState<string>("")
    const [value, setValue] = useState<string>("")
    const [selectedValueType, setSelectedValueType] = useState<ValueType>("string")

    const deviceSelect = useMemo(() => {
        if (!selectedType || !simMap.has(selectedType)) {
            return (<></>)
        }

        return (
            <Select
                onChange={x => setSelectedDevice(x.target.value as string)}
            >{
                [...simMap.get(selectedType)!.keys()].map(x => {
                    return (<MenuItem value={x}>{x}</MenuItem>)
                })
            }</Select>
        )
    }, [selectedType])

    useEffect(() => {
        setSelectedDevice(undefined)
    }, [selectedType])

    const onSimMapUpdate = useCallback((_: Event) => {
        setTb(generateTableBody())
    }, [])

    useEffect(() => {
        window.addEventListener(SimMapUpdateEvent.TYPE, onSimMapUpdate)

        return () => {
            window.removeEventListener(SimMapUpdateEvent.TYPE, onSimMapUpdate)
        }
    }, [onSimMapUpdate])

    return (
        <Panel
            name={"WS View Panel"}
            icon={<GrConnect />}
            panelId={panelId}
            openLocation="right"
            sidePadding={4}
        >
            <TableContainer
                sx={{
                    maxWidth: '80vw',
                    maxHeight: '80vh',
                }}
            >
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell><TypoStyled>Type</TypoStyled></TableCell>
                            <TableCell><TypoStyled>Device</TypoStyled></TableCell>
                            <TableCell><TypoStyled>Data</TypoStyled></TableCell>
                        </TableRow>
                    </TableHead>
                    {tb}
                </Table>
            </TableContainer>
            <Stack>
                <Select
                    onChange={x => setSelectedType(x.target.value as SimType)}
                >
                    <MenuItem value="PWM">PWM</MenuItem>
                    <MenuItem value="SimDevice">SimDevice</MenuItem>
                    <MenuItem value="CANMotor">CAN Motor</MenuItem>
                    <MenuItem value="CANEncoder">CAN Encoder</MenuItem>
                </Select>
                {deviceSelect}
                {selectedDevice
                    ? <Box>
                        <TextField
                            onChange={x => setField(x.target.value as string)}
                        >
                        </TextField>
                        <TextField
                            onChange={x => setValue(x.target.value as string)}
                        >
                        </TextField>
                        <Select
                            onChange={x => setSelectedValueType(x.target.value as ValueType)}
                        >
                            <MenuItem value="string">String</MenuItem>
                            <MenuItem value="number">Number</MenuItem>
                            <MenuItem value="object">Object</MenuItem>
                            <MenuItem value="boolean">Boolean</MenuItem>
                        </Select>
                        <Button
                            onClick={x => setGeneric(selectedType!, selectedDevice, field, value, selectedValueType)}
                        >
                            Set
                        </Button>
                    </Box>
                    : <></>
                }
            </Stack>
        </Panel>
    )
}

export default WSViewPanel
