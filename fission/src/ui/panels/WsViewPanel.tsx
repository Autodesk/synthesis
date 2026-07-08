/**
 * TODO: I don't have the time but this whole thing needs to be remade to
 * make debugging signal data easier.
 */

import {
    Box,
    Button,
    MenuItem,
    Select,
    Stack,
    styled,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography,
} from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import SimGeneric from "@/systems/simulation/wpilib_brain/sim/SimGeneric"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import type { PanelImplProps } from "../components/Panel"
import { useUIContext } from "../helpers/UIProviderHelpers"

const TABLE_UPDATE_INTERVAL = 250

type ValueType = "string" | "number" | "object" | "boolean"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
    color: "white",
})

// function formatMap(map: Map<string, number | boolean | string>): string {
//     let entries: string = ""
//     map.forEach((value, key) => {
//         entries += `${key} : ${value}`
//     })
//     return entries
// }

function generateTableBody() {
    // const names: SimType[] = [
    //     SimType.PWM,
    //     SimType.SimDevice,
    //     SimType.CANMotor,
    //     SimType.CANEncoder,
    //     SimType.Gyro,
    //     SimType.Accel,
    //     SimType.DIO,
    //     SimType.AI,
    //     SimType.AO,
    // ]

    return (
        <TableBody>
            {/* {names.map(name =>
                simMap.has(name) ? (
                    [...getSimMap()?.get(name)!.entries()]
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
            )} */}
        </TableBody>
    )
}

function setGeneric(simType: SimType, device: string, field: string, value: string, valueType: ValueType) {
    switch (valueType) {
        case "number":
            SimGeneric.set(simType, device, field, parseFloat(value))
            break
        case "object":
            SimGeneric.set(simType, device, field, JSON.parse(value))
            break
        case "boolean":
            SimGeneric.set(simType, device, field, parseInt(value)) // 1 or 0 (change to float if needed)
            break
        default:
            SimGeneric.set(simType, device, field, parseFloat(value))
            break
    }
}

const WSViewPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    // const [tb, setTb] = useState(generateTableBody())
    const { configureScreen } = useUIContext()

    const [table, updateTable] = useReducer(_ => generateTableBody(), generateTableBody())

    const [selectedType, setSelectedType] = useState<SimType | undefined>()
    const [selectedDevice, setSelectedDevice] = useState<string | undefined>()
    const [field, setField] = useState<string>("")
    const [value, setValue] = useState<string>("")
    const [selectedValueType, setSelectedValueType] = useState<ValueType>("string")

    // const deviceSelect = useMemo(() => {
    //     if (!selectedType || !simMap.has(selectedType)) {
    //         return <></>
    //     }

    //     return <Dropdown options={[...simMap.get(selectedType)!.keys()]} onSelect={v => setSelectedDevice(v)} />
    // }, [selectedType])

    useEffect(() => {
        setSelectedDevice(undefined)
    }, [])

    useEffect(() => {
        const func = () => {
            updateTable()
        }
        const id: NodeJS.Timeout = setInterval(func, TABLE_UPDATE_INTERVAL)

        return () => {
            clearTimeout(id)
        }
    }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "WS View Panel" }, {})
    }, [])

    return (
        <Stack>
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
                    {table}
                </Table>
            </TableContainer>
            <Stack>
                <Select value={selectedType} onChange={e => setSelectedType(e.target.value as SimType)}>
                    {["PWM", "SimDevice", "CANMotor", "CANEncoder", "Gyro"].map(t => (
                        <MenuItem key={`device-type-${t}`} value={t}>
                            {t}
                        </MenuItem>
                    ))}
                </Select>
                {/* {deviceSelect} */}
                {selectedDevice ? (
                    <Box>
                        <TextField placeholder="Field Name" onChange={e => setField(e.target.value)} />
                        <TextField placeholder="Value" onChange={e => setValue(e.target.value)} />
                        <Select
                            value={selectedValueType}
                            onChange={e => setSelectedValueType(e.target.value as ValueType)}
                        >
                            {["string", "number", "object", "boolean"].map(t => (
                                <MenuItem key={`value-type-${t}`} value={t}>
                                    {t}
                                </MenuItem>
                            ))}
                        </Select>
                        <Button
                            onClick={() =>
                                setGeneric(selectedType ?? SimType.PWM, selectedDevice, field, value, selectedValueType)
                            }
                        >
                            Set
                        </Button>
                    </Box>
                ) : (
                    <></>
                )}
            </Stack>
        </Stack>
    )
}

export default WSViewPanel
