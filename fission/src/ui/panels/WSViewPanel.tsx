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
import { GrConnect } from "react-icons/gr"
import Dropdown from "../components/Dropdown"
import Input from "../components/Input"
import Button from "../components/Button"

type ValueType = "string" | "number" | "object" | "boolean"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
    color: "white",
})

function generateTableBody() {
    return (
        <TableBody>
            {simMap.has("PWM") ? (
                [...simMap.get("PWM")!.entries()]
                    .filter(x => x[1]["<init"] == true)
                    .map(x => {
                        return (
                            <TableRow key={x[0]}>
                                <TableCell>
                                    <TypoStyled>PWM</TypoStyled>
                                </TableCell>
                                <TableCell>
                                    <TypoStyled>{x[0]}</TypoStyled>
                                </TableCell>
                                <TableCell>
                                    <TypoStyled>{JSON.stringify(x[1])}</TypoStyled>
                                </TableCell>
                            </TableRow>
                        )
                    })
            ) : (
                <></>
            )}
            {simMap.has("SimDevice") ? (
                [...simMap.get("SimDevice")!.entries()].map(x => {
                    return (
                        <TableRow key={x[0]}>
                            <TableCell>
                                <TypoStyled>SimDevice</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{x[0]}</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{JSON.stringify(x[1])}</TypoStyled>
                            </TableCell>
                        </TableRow>
                    )
                })
            ) : (
                <></>
            )}
            {simMap.has("CANMotor") ? (
                [...simMap.get("CANMotor")!.entries()].map(x => {
                    return (
                        <TableRow key={x[0]}>
                            <TableCell>
                                <TypoStyled>CAN Motor</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{x[0]}</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{JSON.stringify(x[1])}</TypoStyled>
                            </TableCell>
                        </TableRow>
                    )
                })
            ) : (
                <></>
            )}
            {simMap.has("CANEncoder") ? (
                [...simMap.get("CANEncoder")!.entries()].map(x => {
                    return (
                        <TableRow key={x[0]}>
                            <TableCell>
                                <TypoStyled>CAN Encoder</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{x[0]}</TypoStyled>
                            </TableCell>
                            <TableCell>
                                <TypoStyled>{JSON.stringify(x[1])}</TypoStyled>
                            </TableCell>
                        </TableRow>
                    )
                })
            ) : (
                <></>
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
        <Panel name={"WS View Panel"} icon={<GrConnect />} panelId={panelId} openLocation="right" sidePadding={4}>
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
                    options={["PWM", "SimDevice", "CANMotor", "CANEncoder"]}
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
                                setGeneric(selectedType ?? "PWM", selectedDevice, field, value, selectedValueType)
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
