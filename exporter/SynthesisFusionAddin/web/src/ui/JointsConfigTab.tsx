import {
    Box,
    Button,
    Checkbox,
    IconButton,
    InputAdornment,
    MenuItem,
    Paper,
    Select,
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
import { type FusionJoint, selectJoint } from "../lib/joints"
import { type Joint, JointParentType, JointType, SignalType, WheelType } from "../lib/types"
import { Global_SetAlert } from "../lib/GlobalUtils.tsx"
import { createJoint, jointInfo, signalInfo } from "../lib/joints"



interface JointsConfigTabProps {
    joints: Joint[]
    updateJoints: (cb: (joints: Joint[]) => void) => void
    // updateJoint: <K extends keyof Joint>(index: number, key: K, value: Joint[K]) => void
    // removeJoint: (index: number) => void
}
function JointsConfigTab({ joints, updateJoints }: JointsConfigTabProps) {
    function updateJoint<K extends keyof Joint>(index: number, key: K, value: Joint[K]) {
        updateJoints(joints => {
            joints[index][key] = value
        })
    }

    const [selectingJoint, setSelectingJoint] = useState(false)
    const jointCancelCallback = useRef<(() => void) | undefined>(undefined)
    return (
        <>
            <h4>
                {joints.length} Joint{joints.length != 1 ? "s" : ""}
            </h4>
            <TableContainer component={Paper} elevation={6} sx={{ marginBottom: "10px" }}>
                <Table sx={{ minWidth: 650 }} aria-label="simple table">
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ width: "5%" }} align="center">
                                Type
                            </TableCell>
                            <TableCell sx={{ width: "12%" }} align="center">
                                Name
                            </TableCell>
                            <TableCell sx={{ width: "10%" }} align="center">
                                Parent Joint
                            </TableCell>
                            <TableCell sx={{ width: "10%" }} align="center">
                                Signal Type
                            </TableCell>
                            <TableCell sx={{ width: "30%" }} align="center">
                                Joint Speed
                            </TableCell>
                            <TableCell sx={{ width: "30%" }} align="center">
                                Joint Force
                            </TableCell>
                            <TableCell sx={{ width: "5%" }} align="center">
                                Is Wheel?
                            </TableCell>
                            <TableCell sx={{ width: "3%" }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {joints.map((row, i) => (
                            <TableRow key={row.id} sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                                {/*<TableCell>{jointInfo[row.type]?.name}</TableCell>*/}
                                <TableCell align="center">
                                    <img
                                        src={jointInfo[row.type]?.icon}
                                        alt={jointInfo[row.type]?.name}
                                        title={jointInfo[row.type]?.name}
                                    />
                                </TableCell>
                                <TableCell align="center">{row.name}</TableCell>
                                <TableCell align="center">
                                    <Select
                                        size="small"
                                        value={row.parentNode}
                                        onChange={e => {
                                            updateJoint(i, "parentNode", e.target.value)
                                        }}
                                        fullWidth>
                                        <MenuItem value={JointParentType.ROOT}>Root</MenuItem>
                                        <MenuItem value={JointParentType.END}>End</MenuItem>
                                        {/*{joints.map((joint, j) => {*/}
                                        {/*    if (j == i || joint.parentNode != "root") return*/}
                                        {/*    return <MenuItem value={joint.id}>{joint.name}</MenuItem>*/}
                                        {/*})}*/}
                                    </Select>
                                </TableCell>
                                <TableCell align="center">
                                    <Select
                                        size="small"
                                        value={row.signalType}
                                        onChange={e => {
                                            updateJoint(i, "signalType", e.target.value)
                                        }}
                                        fullWidth
                                        sx={{
                                            backgroundColor: signalInfo[row.signalType].bg,
                                            outlineColor: signalInfo[row.signalType].outline,
                                            borderColor: signalInfo[row.signalType].outline,
                                        }}>
                                        <MenuItem sx={{ color: signalInfo[SignalType.PWM].fg }} value={SignalType.PWM}>
                                            PWM
                                        </MenuItem>
                                        <MenuItem sx={{ color: signalInfo[SignalType.CAN].fg }} value={SignalType.CAN}>
                                            CAN
                                        </MenuItem>
                                        <MenuItem
                                            sx={{ color: signalInfo[SignalType.PASSIVE].fg }}
                                            value={SignalType.PASSIVE}>
                                            Passive
                                        </MenuItem>
                                    </Select>
                                </TableCell>
                                <TableCell align="center">
                                    <TextField
                                        type="number"
                                        size="small"
                                        slotProps={{
                                            input: {
                                                endAdornment: (
                                                    <InputAdornment position="end">
                                                        {jointInfo[row.type]?.speedUnits}
                                                    </InputAdornment>
                                                ),
                                            },
                                        }}
                                        value={row.speed}
                                        onInput={e => {
                                            updateJoint(
                                                i,
                                                "speed",
                                                parseFloat((e.target as HTMLInputElement).value) || 0
                                            )
                                        }}
                                    />
                                </TableCell>
                                <TableCell align="center">
                                    <TextField
                                        type="number"
                                        size="small"
                                        slotProps={{
                                            input: {
                                                endAdornment: <InputAdornment position="end">N</InputAdornment>,
                                            },
                                        }}
                                        value={row.force}
                                        onInput={e => {
                                            updateJoint(
                                                i,
                                                "force",
                                                parseFloat((e.target as HTMLInputElement).value) || 0
                                            )
                                        }}
                                    />
                                </TableCell>
                                <TableCell align="center">
                                    <Checkbox
                                        disabled={row.type != JointType.RevoluteJointType}
                                        checked={row.isWheel}
                                        onChange={(_, v) => {
                                            updateJoint(i, "isWheel", v)
                                        }}
                                    />
                                </TableCell>
                                <TableCell align="center">
                                    <IconButton
                                        color="error"
                                        onClick={() => {
                                            updateJoints(joints => {
                                                joints.splice(i, 1)
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
                    color="secondary"
                    sx={{ px: "20px" }}
                    loading={selectingJoint}
                    loadingIndicator={"Selecting..."}
                    title={"Select a joint in Fusion"}
                    onClick={async () => {
                        setSelectingJoint(true)
                        // const data = await initiateSelection("Select joint")

                        const data: FusionJoint | undefined = await new Promise(resolve => {
                            jointCancelCallback.current = () => {
                                resolve(undefined)
                            }
                            selectJoint()
                                .then(v => {
                                    resolve(v)
                                })
                                .catch(console.error)
                        })

                        setSelectingJoint(false)
                        if (data == null) return
                        if (joints.some(joint => joint.id == data.entityToken)) {
                            Global_SetAlert("warning", "Joint already selected")
                            return
                        }
                        updateJoints(draft => {
                            draft.push(createJoint(data))
                        })
                    }}>
                    Add Joint
                </Button>
                {/*<Button*/}
                {/*    disabled={!selectingJoint}*/}
                {/*    onClick={async () => {*/}
                {/*        jointCancelCallback.current?.()*/}
                {/*    }}*/}
                {/*    color="warning"*/}
                {/*    variant="contained">*/}
                {/*    Cancel*/}
                {/*</Button>*/}
            </Box>

            <h4>
                {joints.filter(j => j.isWheel).length} Wheel{joints.filter(j => j.isWheel).length != 1 ? "s" : ""}
            </h4>
            <TableContainer component={Paper} elevation={6}>
                <Table sx={{ minWidth: 650 }} aria-label="simple table">
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ width: "5%" }} align="center">
                                Joint
                            </TableCell>
                            <TableCell sx={{ width: "10%" }} align="center">
                                Type
                            </TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {joints.map(
                            (joint, i) =>
                                joint.isWheel && (
                                    <TableRow key={joint.id} sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                                        {/*<TableCell>{jointInfo[row.type]?.name}</TableCell>*/}
                                        <TableCell align="center">{joint.name}</TableCell>
                                        <TableCell align="center">
                                            <Select
                                                size="small"
                                                value={joint.wheelType}
                                                onChange={e => {
                                                    updateJoint(i, "wheelType", e.target.value)
                                                }}
                                                fullWidth>
                                                <MenuItem value={WheelType.STANDARD}>Standard</MenuItem>
                                                <MenuItem value={WheelType.MECANUM}>Mecanum</MenuItem>
                                                <MenuItem value={WheelType.OMNI}>Omni</MenuItem>
                                            </Select>
                                        </TableCell>
                                    </TableRow>
                                )
                        )}
                    </TableBody>
                </Table>
            </TableContainer>
        </>
    )
}

export default JointsConfigTab
