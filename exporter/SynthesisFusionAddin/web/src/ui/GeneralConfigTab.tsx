import AnimationIcon from "@mui/icons-material/Animation"
import ArchiveIcon from "@mui/icons-material/Archive"
import BalanceIcon from "@mui/icons-material/Balance"
import LaunchIcon from "@mui/icons-material/Launch"
import SaveIcon from "@mui/icons-material/Save"
import TuneIcon from "@mui/icons-material/Tune"
import {
    Box,
    Collapse,
    InputAdornment,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    MenuItem,
    Select,
    Slider,
    Switch,
    TextField,
} from "@mui/material"
import type * as React from "react"
import type { ChangeEvent } from "react"
import { ExportLocation, ExportMode, type GeneralConfig } from "../lib/types.ts"

interface ConfigTabProps {
    config: GeneralConfig
    updateConfigItem: <K extends keyof GeneralConfig>(key: K, value: GeneralConfig[K]) => void
}
function GeneralConfigTab({ config, updateConfigItem }: ConfigTabProps): React.ReactElement {
    function updateSelect<K extends keyof GeneralConfig>(key: K) {
        return (
            e:
                | ChangeEvent<Omit<HTMLInputElement, "value"> & { value: GeneralConfig[K] }>
                | (Event & { target: { value: GeneralConfig[K] } })
        ) => {
            updateConfigItem(key, e.target.value)
        }
    }

    function updateLiteral<K extends keyof GeneralConfig>(key: K) {
        return (_: Event | ChangeEvent, v: GeneralConfig[K]) => {
            updateConfigItem(key, v)
        }
    }

    return (
        <Box>
            <List>
                <ListItem>
                    <ListItemIcon>
                        <AnimationIcon />
                    </ListItemIcon>
                    <ListItemText primary={"Exporter Mode"} secondary="Does this object move dynamically?" />
                    <Select
                        value={config.exportMode}
                        onChange={updateSelect("exportMode")}
                        style={{ minWidth: "10rem" }}
                    >
                        <MenuItem value={ExportMode.ROBOT}>Dynamic</MenuItem>
                        <MenuItem value={ExportMode.FIELD}>Static</MenuItem>
                    </Select>
                </ListItem>
                <ListItem>
                    <ListItemIcon>
                        <SaveIcon />
                    </ListItemIcon>
                    <ListItemText primary={"Export Location"} secondary="Where should the exported file be saved?" />
                    <Select
                        value={config.exportLocation}
                        onChange={updateSelect("exportLocation")}
                        style={{ minWidth: "10rem" }}
                    >
                        <MenuItem value={ExportLocation.UPLOAD}>Upload to APS</MenuItem>
                        <MenuItem value={ExportLocation.DOWNLOAD}>Download</MenuItem>
                    </Select>
                </ListItem>
                <Collapse in={config.exportMode === ExportMode.ROBOT}>
                    <ListItem>
                        <ListItemIcon>
                            <BalanceIcon />
                        </ListItemIcon>
                        <ListItemText
                            primary="Auto Calculate Robot Weight"
                            secondary="Approximates the weight of your robot assembly based on defined materials"
                        />
                        <Switch
                            edge="end"
                            onChange={(_, v) => {
                                updateConfigItem("autoCalcRobotWeight", v)
                                if (v) {
                                    updateConfigItem("robotWeight", config.calculatedRobotWeight)
                                }
                            }}
                            checked={config.autoCalcRobotWeight}
                        />
                    </ListItem>
                    <ListItem>
                        <ListItemIcon></ListItemIcon>
                        <ListItemText
                            inset
                            primary="Robot Weight"
                            secondary="Manually provided robot weight value (kg)"
                        />
                        <TextField
                            placeholder="0.0"
                            type="number"
                            disabled={config.autoCalcRobotWeight}
                            slotProps={{
                                htmlInput: {
                                    min: 0,
                                    step: 1,
                                },
                                input: {
                                    endAdornment: <InputAdornment position="end">kg</InputAdornment>,
                                },
                            }}
                            size="small"
                            style={{ minWidth: "10rem" }}
                            onChange={e => {
                                updateConfigItem("robotWeight", safeParseFloat(e.target.value) ?? 0)
                            }}
                            value={config.robotWeight}
                        />
                    </ListItem>
                </Collapse>
                <ListItem>
                    <ListItemIcon>
                        <ArchiveIcon />
                    </ListItemIcon>
                    <ListItemText
                        primary="Compress Output"
                        secondary="Compress the output file for a smaller file size."
                    />
                    <Switch edge="end" onChange={updateLiteral("compressOutput")} checked={config.compressOutput} />
                </ListItem>
                <Collapse in={config.exportMode === ExportMode.ROBOT}>
                    {/* TODO: enable when mix and match is created */}
                    {/*<ListItem>*/}
                    {/*    <ListItemIcon>*/}
                    {/*        <PrecisionManufacturingIcon />*/}
                    {/*    </ListItemIcon>*/}
                    {/*    <ListItemText primary="Export as Part" secondary="Use to export as a part for Mix And Match" />*/}
                    {/*    <Switch edge="end" onChange={updateLiteral("exportAsPart")} checked={config.exportAsPart} />*/}
                    {/*</ListItem>*/}
                    <ListItem>
                        <ListItemIcon>
                            <TuneIcon />
                        </ListItemIcon>
                        <ListItemText
                            primary="Override Friction"
                            secondary="Manually override the default friction values on the bodies in the assembly. From 0 (ice) to 1 (rubber)"
                        />
                        <Switch
                            edge="end"
                            onChange={updateLiteral("frictionOverride")}
                            checked={config.frictionOverride}
                        />
                    </ListItem>
                    <Collapse in={config.frictionOverride}>
                        <ListItem dense>
                            <ListItemIcon></ListItemIcon>
                            {/*<ListItemText inset primary="Friction Coefficient" secondary="From 0 (ice) to 1 (rubber)."/>*/}
                            <Slider
                                min={0}
                                max={1}
                                step={0.01}
                                style={{ minWidth: "10rem", marginLeft: "2rem" }}
                                onChange={updateLiteral("frictionOverrideCoeff")}
                                value={config.frictionOverrideCoeff}
                            />
                            <TextField
                                placeholder="0.0"
                                type="number"
                                size="small"
                                slotProps={{
                                    htmlInput: {
                                        min: 0,
                                        step: 0.01,
                                        max: 1000,
                                    },
                                }}
                                style={{ paddingLeft: "2rem", minWidth: "5rem" }}
                                onChange={e => {
                                    updateConfigItem(
                                        "frictionOverrideCoeff",
                                        safeParseFloat(e.target.value) ?? config.frictionOverrideCoeff
                                    )
                                }}
                                value={config.frictionOverrideCoeff}
                            />
                        </ListItem>
                    </Collapse>
                </Collapse>
                <ListItem>
                    <ListItemIcon>
                        <LaunchIcon />
                    </ListItemIcon>
                    <ListItemText
                        primary="Open Synthesis on Export"
                        secondary="Launch the Synthesis website after successful export"
                    />
                    <Switch
                        edge="end"
                        onChange={updateLiteral("openSynthesisUponExport")}
                        checked={config.openSynthesisUponExport}
                    />
                </ListItem>
            </List>
        </Box>
    )
}

function safeParseFloat(value: string | undefined): number | undefined {
    if (value === undefined) {
        return
    }
    const parsed = parseFloat(value)
    return Number.isNaN(parsed) ? undefined : parsed
}

export default GeneralConfigTab
