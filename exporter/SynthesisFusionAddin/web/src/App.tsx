import { useCallback, useEffect, useState } from "react"

import "./App.css"
import { RestartAlt, Settings, SportsFootball } from "@mui/icons-material"
import DownloadIcon from "@mui/icons-material/Download"
import PrecisionManufacturingIcon from "@mui/icons-material/PrecisionManufacturing"
import SaveIcon from "@mui/icons-material/Save"
import {
    AppBar,
    Backdrop,
    Box,
    Button,
    CircularProgress,
    Container,
    Stack,
    Tab,
    Tabs,
    ThemeProvider,
} from "@mui/material"
import { current } from "immer"
import { useImmer } from "use-immer"
import { sendData, sendDataAndToast } from "./lib"
import { Global_SetAlert } from "./lib/GlobalUtils.tsx"
import { createJoint } from "./lib/joints.ts"
import { theme } from "./lib/theme.ts"
import {
    DefaultExporterConfig,
    type ExporterConfig,
    ExportMode,
    type Gamepiece,
    type GeneralConfig,
    type Joint,
    WheelType,
} from "./lib/types.ts"
import GamepiecesConfigTab from "./ui/GamepiecesConfigTab.tsx"
import GeneralConfigTab from "./ui/GeneralConfigTab.tsx"
import GlobalAlert from "./ui/GlobalAlert.tsx"
import JointsConfigTab from "./ui/JointsConfigTab.tsx"
import type { TaggedBody } from "./ui/MaterialTaggingTab.tsx"

function TabPanel(props: { children?: React.ReactNode; value: number; index: number }) {
    const { children, value, index, ...other } = props

    return (
        <div hidden={value !== index} id={`tabpanel-${index.toString()}`} {...other}>
            <Container sx={{ p: 3 }}>{children}</Container>
        </div>
    )
}

function App() {
    const [activeTab, setActiveTab] = useState(0)
    const [generalConfig, updateGeneralConfig] = useImmer(DefaultExporterConfig())
    const [joints, updateJoints] = useImmer<Joint[]>([])
    const [gamepieces, updateGamepieces] = useImmer<Gamepiece[]>([])
    const [taggedBodies, updateTaggedBodies] = useImmer<TaggedBody[]>([])
    const [isSelecting, setIsSelecting] = useState(false)
    const updateConfigItem = <K extends keyof GeneralConfig>(k: K, v: GeneralConfig[K]) => {
        updateGeneralConfig(config => {
            config[k] = v
        })
    }

    const loadConfigFromFusion = useCallback(() => {
        if (typeof window.adsk === "undefined") {
            requestAnimationFrame(loadConfigFromFusion)
            return
        }
        sendData("init", {})
            .then(data => {
                if (data === undefined) {
                    Global_SetAlert("error", "Could not extract data from fusion")
                    return
                }
                console.log(data)
                updateGeneralConfig(config => {
                    const entries = Object.entries(data.options)
                    entries.forEach(([k, v]) => {
                        if (k in config) {
                            config[k as "robotWeight"] = v as number // Object.entries is terribly typed
                        }
                    })
                    config.calculatedRobotWeight = data.calculatedMass
                    config.robotWeight = config.autoCalcRobotWeight ? data.calculatedMass : config.robotWeight
                })
                updateJoints(() => {
                    if (data.options.joints.length === 0) {
                        return data.jointData.map(fusionJoint => createJoint(fusionJoint))
                    }
                    const res: Joint[] = data.options.joints
                        .map(joint => {
                            const wheel = joint.isWheel
                                ? data.options.wheels.find(wheel => wheel.jointToken === joint.jointToken)
                                : undefined
                            const fusionJoint = data.jointData.find(j => j.entityToken === joint.jointToken)
                            if (fusionJoint === undefined) {
                                return null // No longer in the assembly
                            }
                            return {
                                ...joint,
                                wheelType: wheel?.wheelType ?? WheelType.STANDARD,
                                name: fusionJoint.name,
                                type: fusionJoint.jointType,
                            }
                        })
                        .filter(e => e != null)
                    return res
                })
                updateGamepieces(() => {
                    const res: Gamepiece[] = data.options.gamepieces
                        .map(gamepiece => {
                            const fusionGamepiece = data.gamepieceData.find(
                                g => g.occurrenceToken === gamepiece.occurrenceToken
                            )
                            if (fusionGamepiece === undefined) {
                                return null // No longer in the assembly
                            }
                            return {
                                occurrenceToken: gamepiece.occurrenceToken,
                                userDefinedMass: gamepiece.weight,
                                friction: gamepiece.friction,
                                calculatedMass: fusionGamepiece.mass,
                                name: fusionGamepiece.name,
                                entityIDs: fusionGamepiece.entityIDs,
                            }
                        })
                        .filter(e => e != null)
                    return res
                })

                updateTaggedBodies(() => {
                    const res: TaggedBody[] = Object.entries(data.options.tags)
                        .map(([key, value]) => {
                            const fusionBody = data.tagData.find(b => b.entityToken === key)
                            if (fusionBody === undefined) {
                                return null // No longer in the assembly
                            }
                            return {
                                ...fusionBody,
                                material: value,
                            }
                        })
                        .filter(e => e != null)
                    return res
                })
            })
            .catch((e: unknown) => {
                console.error(e)
                Global_SetAlert("error", "Could not load config")
            })
    }, [updateGamepieces, updateJoints, updateGeneralConfig, updateTaggedBodies])

    useEffect(() => {
        loadConfigFromFusion()
    }, [loadConfigFromFusion])

    const getFinalizedConfig = () =>
        new Promise<ExporterConfig>(resolve => {
            updateGeneralConfig(cfg => {
                cfg.gamepieces = gamepieces.map(gamepiece => ({
                    occurrenceToken: gamepiece.occurrenceToken,
                    weight: generalConfig.autoCalcGamepieceWeight
                        ? gamepiece.calculatedMass
                        : gamepiece.userDefinedMass,
                    friction: gamepiece.friction,
                }))
                cfg.joints = joints
                cfg.wheels = joints
                    .filter(joint => joint.isWheel)
                    .map(joint => ({
                        jointToken: joint.jointToken,
                        signalType: joint.signalType,
                        wheelType: joint.wheelType,
                    }))
                cfg.tags = Object.fromEntries(taggedBodies.map(b => [b.entityToken, b.material]))
                resolve(current(cfg))
            })
        })
    return (
        <ThemeProvider theme={theme}>
            <Backdrop
                sx={theme => ({
                    color: "#fff",
                    zIndex: theme.zIndex.drawer + 1,
                    backdropFilter: "blur(0px)",
                })}
                open={isSelecting}
                onClick={() => {
                    Global_SetAlert(
                        "info",
                        "Click on an element in the Fusion assembly, or press escape in the main Fusion window"
                    )
                }}
            >
                <Stack direction={"column"} alignItems={"center"} justifyContent={"center"} gap={"1rem"}>
                    <Box
                        sx={theme => ({
                            paddingX: "0.5rem",
                            paddingY: "0.4rem",
                            borderRadius: "0.5rem",
                            backgroundColor: theme.palette.grey.A700,
                            boxShadow: theme.shadows[5],
                        })}
                    >
                        Click on an element in the Fusion assembly
                    </Box>
                    <CircularProgress color="inherit" />
                </Stack>
            </Backdrop>
            <GlobalAlert />
            <AppBar position={"sticky"}>
                <Box>
                    <Tabs
                        sx={{ bgcolor: "white" }}
                        value={activeTab}
                        onChange={(_, v) => {
                            setActiveTab(v as number)
                        }}
                    >
                        <Tab icon={<Settings />} iconPosition={"start"} label="General" />
                        <Tab
                            icon={<PrecisionManufacturingIcon />}
                            iconPosition={"start"}
                            label="Joints"
                            disabled={generalConfig.exportMode === ExportMode.FIELD}
                        />
                        <Tab
                            icon={<SportsFootball />}
                            iconPosition={"start"}
                            label="Gamepieces"
                            disabled={generalConfig.exportMode === ExportMode.ROBOT}
                        />

                        {/*<Tab icon={<Texture />} iconPosition={"start"} label="Materials" />*/}

                        {/*<Tab label="APS" />*/}
                    </Tabs>
                </Box>
            </AppBar>
            <TabPanel value={activeTab} index={0}>
                <GeneralConfigTab config={generalConfig} updateConfigItem={updateConfigItem} />
            </TabPanel>
            <TabPanel value={activeTab} index={1}>
                <JointsConfigTab
                    joints={joints}
                    updateJoints={updateJoints}
                    selection={{ isSelecting, setIsSelecting }}
                />
            </TabPanel>
            <TabPanel value={activeTab} index={2}>
                <GamepiecesConfigTab
                    gamepieces={gamepieces}
                    updateGamepieces={updateGamepieces}
                    config={generalConfig}
                    updateConfigItem={updateConfigItem}
                    selection={{ isSelecting, setIsSelecting }}
                />
            </TabPanel>
            {/*<TabPanel value={activeTab} index={3}>*/}
            {/*    <MaterialTaggingTab*/}
            {/*        tags={taggedBodies}*/}
            {/*        updateTags={updateTaggedBodies}*/}
            {/*        selection={{ isSelecting, setIsSelecting }}*/}
            {/*    />*/}
            {/*</TabPanel>*/}
            <Container
                sx={{
                    position: "sticky",
                    bgcolor: "white",
                    display: "flex",
                    flexDirection: "row",
                    gap: "0.5rem",
                    paddingY: "0.5rem",
                    bottom: 0,
                }}
            >
                <Button
                    variant="contained"
                    color="error"
                    sx={{ flexGrow: 1 }}
                    onClick={async () => {
                        updateGeneralConfig(DefaultExporterConfig())
                        updateJoints([])
                        updateGamepieces([])
                        await sendData("save", DefaultExporterConfig())
                        loadConfigFromFusion()
                        Global_SetAlert("info", "Configuration reset")
                    }}
                    startIcon={<RestartAlt />}
                >
                    Reset
                </Button>
                <Button
                    variant="contained"
                    color="primary"
                    sx={{ flexGrow: 9 }}
                    onClick={async () => sendDataAndToast("export", await getFinalizedConfig(), "Exported!")}
                    startIcon={<DownloadIcon />}
                >
                    Export
                </Button>
                <Button
                    variant="contained"
                    color="secondary"
                    sx={{ flexGrow: 1 }}
                    onClick={async () => sendDataAndToast("save", await getFinalizedConfig(), "Saved!")}
                    startIcon={<SaveIcon />}
                >
                    Save
                </Button>
            </Container>
        </ThemeProvider>
    )
}

export default App
