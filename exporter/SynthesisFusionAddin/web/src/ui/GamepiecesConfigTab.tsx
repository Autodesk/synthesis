import BalanceIcon from "@mui/icons-material/Balance"
import DeleteIcon from "@mui/icons-material/Delete"
import {
    Box,
    Button,
    IconButton,
    InputAdornment,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
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
import { selectGamepiece } from "../lib"
import { Global_SetAlert } from "../lib/GlobalUtils.tsx"
import type { Gamepiece, GeneralConfig } from "../lib/types"

interface GamepiecesConfigTabProps {
    gamepieces: Gamepiece[]
    updateGamepieces: (cb: (gamepieces: Gamepiece[]) => void) => void
    config: GeneralConfig
    updateConfigItem: <K extends keyof GeneralConfig>(key: K, value: GeneralConfig[K]) => void
    selection: {
        isSelecting: boolean
        setIsSelecting: (value: boolean) => void
    }
}

function GamepiecesConfigTab({
    gamepieces,
    updateGamepieces,
    config,
    updateConfigItem,
    selection,
}: GamepiecesConfigTabProps) {
    function updateItem<K extends keyof Gamepiece>(index: number, key: K, value: Gamepiece[K]) {
        updateGamepieces(items => {
            items[index][key] = value
        })
    }
    return (
        <>
            <List component={Paper}>
                <ListItem>
                    <ListItemIcon>
                        <BalanceIcon />
                    </ListItemIcon>
                    <ListItemText
                        primary={"Automatically Calculate Gamepiece Weight"}
                        secondary="Approximates the weight of your gamepiece assemblies based on defined materials"
                    />
                    <Switch
                        value={config.autoCalcGamepieceWeight}
                        onChange={(_, v) => {
                            updateConfigItem("autoCalcGamepieceWeight", v)
                        }}
                    />
                </ListItem>
            </List>
            <h4>
                {gamepieces.length} Gamepiece{gamepieces.length === 1 ? "" : "s"}
            </h4>
            <TableContainer component={Paper} elevation={6}>
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
                                key={gamepiece.occurrenceToken}
                                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
                            >
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
                                        }}
                                    >
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
                }}
            >
                <Button
                    variant="contained"
                    color="secondary"
                    loading={selection.isSelecting}
                    sx={{ px: "20px" }}
                    loadingIndicator={"Selecting..."}
                    onClick={async () => {
                        selection.setIsSelecting(true)

                        const data = await selectGamepiece()

                        selection.setIsSelecting(false)
                        if (data == null) return
                        const allDuplicates = data.every(newgamepiece => {
                            if (gamepieces.some(gamepiece => gamepiece.entityIDs.includes(newgamepiece.entityIDs[0]))) {
                                console.warn("attempted to add existing element")
                                Global_SetAlert("warning", "Component already added")
                                return true
                            }
                            return false
                        })
                        if (allDuplicates) {
                            return
                        }
                        updateGamepieces(draft => {
                            data.forEach(gamepiece => {
                                const roundedMass = Math.round(gamepiece.mass * 100) / 100
                                draft.push({
                                    ...gamepiece,
                                    userDefinedMass: roundedMass,
                                    calculatedMass: roundedMass,
                                    friction: 0.5,
                                })
                            })
                        })
                    }}
                >
                    Add Gamepiece
                </Button>
            </Box>
        </>
    )
}

export default GamepiecesConfigTab
