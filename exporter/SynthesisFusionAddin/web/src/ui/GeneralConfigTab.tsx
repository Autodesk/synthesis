import * as React from "react"
import { type ChangeEvent } from "react"
import type { ExporterConfig } from "../lib/types.ts"
import {
    Box,
    Button,
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

import AnimationIcon from "@mui/icons-material/Animation"
import BalanceIcon from "@mui/icons-material/Balance"
import SaveIcon from "@mui/icons-material/Save"
import ArchiveIcon from "@mui/icons-material/Archive"
import PrecisionManufacturingIcon from "@mui/icons-material/PrecisionManufacturing"
import TuneIcon from "@mui/icons-material/Tune"
import LaunchIcon from "@mui/icons-material/Launch"
import DownloadIcon from "@mui/icons-material/Download"
import {sendData} from "../lib";

interface ConfigTabProps {
    config: ExporterConfig
    updateConfigItem: <K extends keyof ExporterConfig>(key: K, value: ExporterConfig[K]) => void
}
function GeneralConfigTab({ config, updateConfigItem }: ConfigTabProps): React.ReactElement {
    function updateSelect<K extends keyof ExporterConfig>(key: K) {
        return (
            e:
                | ChangeEvent<Omit<HTMLInputElement, "value"> & { value: ExporterConfig[K] }>
                | (Event & { target: { value: ExporterConfig[K] } })
        ) => {
            updateConfigItem(key, e.target.value)
        }
    }

    function updateLiteral<K extends keyof ExporterConfig>(key: K) {
        return (_: Event | ChangeEvent, v: ExporterConfig[K]) => {
            updateConfigItem(key, v)
        }
    }
    sendData("init", undefined).then((data) => {
      updateConfigItem("calculatedWeight", data?.mass ?? 0)
    })

    return (
        <>
            <Box>
                <List>
                    <ListItem>
                        <ListItemIcon>
                            <AnimationIcon />
                        </ListItemIcon>
                        <ListItemText primary={"Exporter Mode"} secondary="Does this object move dynamically?" />
                        <Select value={config.mode} onChange={updateSelect("mode")} style={{ minWidth: "10rem" }}>
                            <MenuItem value={"ROBOT"}>Dynamic</MenuItem>
                            <MenuItem value={"FIELD"}>Static</MenuItem>
                        </Select>
                    </ListItem>
                    <ListItem>
                        <ListItemIcon>
                            <SaveIcon />
                        </ListItemIcon>
                        <ListItemText
                            primary={"Export Location"}
                            secondary="Where should the exported file be saved?"
                        />
                        <Select
                            value={config.destination}
                            onChange={updateSelect("destination")}
                            style={{ minWidth: "10rem" }}>
                            <MenuItem value={"UPLOAD"}>Upload to APS</MenuItem>
                            <MenuItem value={"DOWNLOAD"}>Download</MenuItem>
                        </Select>
                    </ListItem>
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
                            onChange={updateLiteral("autoCalculateRobotWeight")}
                            checked={config.autoCalculateRobotWeight}
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
                                disabled={config.autoCalculateRobotWeight}
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
                                    updateConfigItem("userDefinedWeight", safeParseFloat(e.target.value) ?? 0)
                                }}
                                value={config.autoCalculateRobotWeight ? config.calculatedWeight : config.userDefinedWeight}
                            />
                        </ListItem>
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
                    <ListItem>
                        <ListItemIcon>
                            <PrecisionManufacturingIcon />
                        </ListItemIcon>
                        <ListItemText primary="Export as Part" secondary="Use to export as a part for Mix And Match" />
                        <Switch edge="end" onChange={updateLiteral("exportAsPart")} checked={config.exportAsPart} />
                    </ListItem>
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
                            onChange={updateLiteral("overrideFriction")}
                            checked={config.overrideFriction}
                        />
                    </ListItem>
                    <Collapse in={config.overrideFriction}>
                        <ListItem dense>
                            <ListItemIcon></ListItemIcon>
                            {/*<ListItemText inset primary="Friction Coefficient" secondary="From 0 (ice) to 1 (rubber)."/>*/}
                            <Slider
                                min={0}
                                max={1}
                                step={0.01}
                                style={{ minWidth: "10rem", marginLeft: "2rem" }}
                                onChange={updateLiteral("userDefinedFriction")}
                                value={config.userDefinedFriction}
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
                                        "userDefinedFriction",
                                        safeParseFloat(e.target.value) ?? config.userDefinedFriction
                                    )
                                }}
                                value={config.userDefinedFriction}
                            />
                        </ListItem>
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
                            onChange={updateLiteral("openSynthesisWhenDone")}
                            checked={config.openSynthesisWhenDone}
                        />
                    </ListItem>
                </List>
                <Button fullWidth style={{ marginTop: "3rem" }} variant="contained" startIcon={<DownloadIcon />}>
                    Export
                </Button>
            </Box>
        </>
    )
}

function safeParseFloat(value: string | undefined): number | undefined {
    if (value == undefined) {
        return
    }
    const parsed = parseFloat(value)
    return isNaN(parsed) ? undefined : parsed
}

export default GeneralConfigTab
