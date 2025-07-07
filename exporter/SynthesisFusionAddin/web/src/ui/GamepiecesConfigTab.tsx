import {
    Box,
    Button,
    FormControlLabel,
    IconButton,
    InputAdornment,
    Paper,
    Slider,
    Switch,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
} from "@mui/material"
import DeleteIcon from "@mui/icons-material/Delete"

import { useRef, useState } from "react"
import { type FusionGamepiece, selectGamepiece } from "../lib"
import { type Gamepiece, type GeneralConfig } from "../lib/types"
import { Global_SetAlert } from "../lib/GlobalUtils.tsx"

interface GamepiecesConfigTabProps {
    gamepieces: Gamepiece[]
    updateGamepieces: (cb: (gamepieces: Gamepiece[]) => void) => void
    config: GeneralConfig
    updateConfigItem: <K extends keyof GeneralConfig>(key: K, value: GeneralConfig[K]) => void
    // updateJoint: <K extends keyof Joint>(index: number, key: K, value: Joint[K]) => void
    // removeJoint: (index: number) => void
}
function GamepiecesConfigTab({ gamepieces, updateGamepieces, config, updateConfigItem }: GamepiecesConfigTabProps) {
    function updateItem<K extends keyof Gamepiece>(index: number, key: K, value: Gamepiece[K]) {
        updateGamepieces(items => {
            items[index][key] = value
        })
    }

    const [selectingActive, setSelectingActive] = useState(false)
    const selectionCancelCallback = useRef<(() => void) | undefined>(undefined)
    return (
        <>
            <FormControlLabel
                control={<Switch />}
                label="Automatically calculate gamepiece weight"
                value={config.autoCalcGamepieceWeight}
                onChange={(_, v) => {
                    updateConfigItem("autoCalcGamepieceWeight", v)
                }}
            />
            <TableContainer component={Paper}>
                <Table sx={{ minWidth: 650 }} aria-label="simple table" size={"small"}>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ width: "20%" }} align="center">
                                Name
                            </TableCell>
                            <TableCell sx={{ width: "20%" }} align="center">
                                Weight (kg)
                            </TableCell>
                            <TableCell sx={{ width: "60%" }} align="center">
                                Friction
                            </TableCell>
                            <TableCell sx={{ width: "3%" }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {gamepieces.map((gamepiece, i) => (
                            <TableRow
                                key={gamepiece.entityToken}
                                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                                <TableCell>{gamepiece.name}</TableCell>
                                <TableCell align="center">
                                    <TextField
                                        type="number"
                                        size="small"
                                        disabled={config.autoCalcGamepieceWeight}
                                        slotProps={{
                                            input: {
                                                endAdornment: <InputAdornment position="end">kg</InputAdornment>,
                                            },
                                        }}
                                        value={
                                            config.autoCalcGamepieceWeight
                                                ? gamepiece.calculatedMass
                                                : gamepiece.userDefinedMass
                                        }
                                        onInput={e => {
                                            updateItem(
                                                i,
                                                "userDefinedMass",
                                                parseFloat((e.target as HTMLInputElement).value) || 0
                                            )
                                        }}
                                    />
                                </TableCell>
                                <TableCell align="center">
                                    <Box display={"flex"} flexDirection={"row"}>
                                        <Slider
                                            min={0}
                                            max={1}
                                            step={0.01}
                                            style={{ marginRight: "2rem" }}
                                            onChange={(_, v) => {
                                                updateItem(i, "friction", v)
                                            }}
                                            sx={{ flexBasis: "80%" }}
                                            value={gamepiece.friction}
                                        />
                                        <TextField
                                            type="number"
                                            size="small"
                                            slotProps={{
                                                htmlInput: {
                                                    step: 0.05,
                                                    min: 0,
                                                    max: 1,
                                                },
                                            }}
                                            sx={{ flexBasis: "20%" }}
                                            value={gamepiece.friction}
                                            onInput={e => {
                                                updateItem(
                                                    i,
                                                    "friction",
                                                    parseFloat((e.target as HTMLInputElement).value) || 0
                                                )
                                            }}
                                        />
                                    </Box>
                                </TableCell>
                                <TableCell align="center">
                                    <IconButton
                                        color="error"
                                        onClick={() => {
                                            updateGamepieces(draft => {
                                                draft.splice(i, 1)
                                            })
                                        }}>
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <Box
                paddingTop="15px"
                style={{
                    display: "flex",
                    justifyContent: "left",
                    alignItems: "center",
                    gap: "10px",
                }}>
                <Button
                    variant="contained"
                    loading={selectingActive}
                    loadingIndicator={"Selecting..."}
                    onClick={async () => {
                        setSelectingActive(true)
                        // const data = await initiateSelection("Select joint")

                        const data: FusionGamepiece | undefined = await new Promise(async resolve => {
                            selectionCancelCallback.current = () => {
                                resolve(undefined)
                            }
                            resolve(await selectGamepiece())
                        })

                        setSelectingActive(false)
                        if (data == null) return
                        if (gamepieces.some(gamepiece => gamepiece.entityIDs.includes(data.entityToken))) {
                            console.warn("attempted to add existing element")
                            Global_SetAlert("warning", "Component already added")
                            return
                        }
                        updateGamepieces(draft => {
                            const roundedMass = Math.round(data.mass * 100) / 100
                            draft.push({
                                ...data,
                                userDefinedMass: roundedMass,
                                calculatedMass: roundedMass,
                                friction: 0.5,
                            })
                        })
                    }}>
                    Add Gamepiece
                </Button>
                <Button
                    disabled={!selectingActive}
                    onClick={() => {
                        selectionCancelCallback.current?.()
                        setSelectingActive(false)
                    }}
                    color="warning"
                    variant="contained">
                    Cancel
                </Button>
            </Box>
        </>
    )
}

export default GamepiecesConfigTab
