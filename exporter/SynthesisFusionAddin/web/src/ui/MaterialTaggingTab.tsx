import {
    Box,
    Button,
    IconButton,
    MenuItem,
    Paper,
    Select,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
} from "@mui/material"
import DeleteIcon from "@mui/icons-material/Delete"


import { useRef, useState } from "react"
import { type FusionBody, selectBody } from "../lib"
import { Global_SetAlert } from "../lib/GlobalUtils.tsx"

export const MATERIALS = ["Softbody", "Rigid", "Chain", "Spring", "Rope"] as const
export type Material = (typeof MATERIALS)[number]
export type TaggedBody = FusionBody & {
    material: Material
}
interface MaterialTaggingTabProps {
    tags: TaggedBody[]
    updateTags: (cb: (tags: TaggedBody[]) => void) => void
    // updateJoint: <K extends keyof Joint>(index: number, key: K, value: Joint[K]) => void
    // removeJoint: (index: number) => void
}
function MaterialTaggingTab({ tags, updateTags }: MaterialTaggingTabProps) {
    function updateTag<K extends keyof TaggedBody>(index: number, key: K, value: TaggedBody[K]) {
        updateTags(items => {
            items[index][key] = value
        })
    }
    const [selectingActive, setSelectingActive] = useState(false)
    const selectionCancelCallback = useRef<(() => void) | undefined>(undefined)
    return (
        <>
            <h4>
                {tags.length} Tagged Item{tags.length == 1 ? "" : "s"}
            </h4>
            <TableContainer component={Paper} elevation={6}>
                <Table sx={{ minWidth: 650 }} aria-label="simple table" size={"small"}>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ width: "28%" }} align="center">
                                Body Name
                            </TableCell>
                            <TableCell sx={{ width: "28%" }} align="center">
                                Component
                            </TableCell>
                            <TableCell sx={{ width: "40%" }} align="center">
                                Material
                            </TableCell>
                            <TableCell sx={{ width: "4%" }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {tags.map((tag, i) => (
                            <TableRow key={tag.entityToken} sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                                <TableCell>{tag.name}</TableCell>
                                <TableCell>{tag.componentName}</TableCell>
                                <TableCell align="center">
                                    <Select
                                        value={tag.material}
                                        fullWidth
                                        onChange={e => {
                                            updateTag(i, "material", e.target.value)
                                        }}>
                                        {MATERIALS.map(material => (
                                            <MenuItem key={material} value={material}>
                                                {material}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </TableCell>
                                <TableCell align="center">
                                    <IconButton
                                        color="error"
                                        onClick={() => {
                                            updateTags(draft => {
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
                    color="secondary"
                    loading={selectingActive}
                    loadingIndicator={"Selecting..."}
                    onClick={async () => {
                        setSelectingActive(true)
                        // const data = await initiateSelection("Select joint")

                        const data: FusionBody | undefined = await new Promise(async resolve => {
                            selectionCancelCallback.current = () => {
                                resolve(undefined)
                            }
                            resolve(await selectBody())
                        })

                        setSelectingActive(false)
                        if (data == null) return
                        if (tags.some(tag => tag.entityToken == data.entityToken)) {
                            console.warn("attempted to add existing element")
                            Global_SetAlert("warning", "Component already added")
                            return
                        }
                        updateTags(draft => {
                            draft.push({
                                ...data,
                                material: "Rigid",
                            })
                        })
                    }}>
                    Add Body
                </Button>
            </Box>
        </>
    )
}

export default MaterialTaggingTab
