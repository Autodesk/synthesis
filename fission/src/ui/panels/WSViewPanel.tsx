import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SimMapUpdateEvent, SimGeneric, simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import {
    Box,
    Stack,
    styled,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
} from "@mui/material"
import { useCallback, useEffect, useMemo, useState } from "react"
import Dropdown from "../components/Dropdown"
import Input from "../components/Input"
import Button from "../components/Button"
import { SynthesisIcons } from "../components/StyledComponents"

type ValueType = "string" | "number" | "object" | "boolean"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
    color: "white",
})

function formatMap(map: Map<string, number>): string {
    let entries: string = ""
    map.forEach((value, key) => {
        entries += `${key} : ${value}`
    })
    return entries
}

function generateTableBody() {
    const names: SimType[] = [
        SimType.PWM,
        SimType.SimDevice,
        SimType.CANMotor,
        SimType.CANEncoder,
        SimType.Gyro,
        SimType.Accel,
        SimType.DIO,
        SimType.AI,
        SimType.AO,
    ]

    console.log(simMap)

    return (
        <TableBody>
            {names.map(name =>
                simMap.has(name) ? (
                    [...simMap.get(name)!.entries()]
                        // most devices don't have <init field but we want to hide the ones that do
                        .filter(x => !Object.keys(x[1]).includes("<init") || !!(x[1].get("<init") ?? false) == true)
                        .map(x => (
                            <TableRow key={x[0]}>
                                <TableCell>
                                    <TypoStyled>{name}</TypoStyled>
                                </TableCell>
                                <TableCell>
                                    <TypoStyled>{x[0]}</TypoStyled>
                                </TableCell>
                                <TableCell>
                                    <TypoStyled>{formatMap(x[1])}</TypoStyled>
                                </TableCell>
                            </TableRow>
                        ))
                ) : (
                    <></>
                )
            )}
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
            SimGeneric.Set(simType, device, field, parseInt(value)) // 1 or 0 (change to float if needed)
            break
        default:
            SimGeneric.Set(simType, device, field, parseFloat(value))
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
            return <></>
        }

        return <Dropdown options={[...simMap.get(selectedType)!.keys()]} onSelect={v => setSelectedDevice(v)} />
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
            icon={SynthesisIcons.Connect}
            panelId={panelId}
            openLocation="right"
            sidePadding={4}
        >
            <TableContainer
                sx={{
                    maxWidth: "80vw",
                    maxHeight: "80vh",
                }}
            >
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>
                                <TypoStyled>Type</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>Device</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>Data</TypoStyled>
                            </TableCell>
                        </TableRow>
                    </TableHead>
                    {tb}
                </Table>
            </TableContainer>
            <Stack>
                <Dropdown
                    options={["PWM", "SimDevice", "CANMotor", "CANEncoder", "Gyro"]}
                    onSelect={v => setSelectedType(v as unknown as SimType)}
                />
                {deviceSelect}
                {selectedDevice ? (
                    <Box>
                        <Input placeholder="Field Name" onInput={v => setField(v)} />
                        <Input placeholder="Value" onInput={v => setValue(v)} />
                        <Dropdown
                            options={["string", "number", "object", "boolean"]}
                            onSelect={v => setSelectedValueType(v as ValueType)}
                        />
                        <Button
                            value={"Set"}
                            onClick={() =>
                                setGeneric(selectedType ?? SimType.PWM, selectedDevice, field, value, selectedValueType)
                            }
                        />
                    </Box>
                ) : (
                    <></>
                )}
            </Stack>
        </Panel>
    )
}

export default WSViewPanel
