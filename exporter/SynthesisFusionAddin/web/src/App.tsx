import { useEffect, useState } from "react"

import "./App.css"
import { selectJoint, sendData } from "./lib"
import { Box, Button, Container, Tab, Tabs, Tooltip } from "@mui/material"
import GeneralConfigTab from "./ui/GeneralConfigTab.tsx"
import JointsConfigTab from "./ui/JointsConfigTab.tsx"
import { useImmer } from "use-immer"
import {
    DefaultExporterConfig,
    type ExporterConfig,
    ExportMode,
    type Gamepiece,
    type GeneralConfig,
    type Joint,
    JointType,
    WheelType,
} from "./lib/types.ts"
import GamepiecesConfigTab from "./ui/GamepiecesConfigTab.tsx"
import GlobalAlert from "./ui/GlobalAlert.tsx"
import { Global_SetAlert } from "./lib/GlobalUtils.tsx"
import DownloadIcon from "@mui/icons-material/Download"
import { current } from "immer"

function TabPanel(props: { children?: React.ReactNode; value: number; index: number }) {
    const { children, value, index, ...other } = props

    return (
        <div hidden={value !== index} id={`tabpanel-${index}`} {...other}>
            <Container sx={{ p: 3 }}>{children}</Container>
        </div>
    )
}

function App() {
    const [activeTab, setActiveTab] = useState(0)
    const [generalConfig, updateGeneralConfig] = useImmer(DefaultExporterConfig())
    const [joints, updateJoints] = useImmer<Joint[]>([])
    const [gamepieces, updateGamepieces] = useImmer<Gamepiece[]>([])
    const updateConfigItem = <K extends keyof GeneralConfig>(k: K, v: GeneralConfig[K]) => {
        updateGeneralConfig(config => {
            config[k] = v
        })
    }
    useEffect(() => {
        const fetchCb = () => {
            if (window.adsk == undefined) {
                requestAnimationFrame(fetchCb)
                return
            }
            console.log("Requesting data")
            sendData("init", {}).then(data => {
                if (data == undefined) {
                    Global_SetAlert("error", "Could not extract data from fusion")
                    return
                }
                console.log(data)
                updateGeneralConfig(config => {
                    const entries = Object.entries(data.options)
                    entries.forEach(([k, v]) => {
                        if (k in config) {
                            config[k as "robotWeight"] = v // Object.entries is terribly typed
                        }
                    })
                    config.calculatedRobotWeight = data.calculatedMass
                    config.robotWeight = config.autoCalcRobotWeight ? data.calculatedMass : config.robotWeight
                })
                updateJoints(() => {
                    const res: Joint[] = data.options.joints.map(joint => {
                        const wheel = joint.isWheel
                            ? data.options.wheels.find(wheel => wheel.jointToken === joint.jointToken)
                            : undefined
                        const fusionJoint = data.jointData.find(j => j.entityToken == joint.jointToken)
                        if (fusionJoint == undefined) {
                            return null // No longer in the assembly
                        }
                        return {
                            id: joint.jointToken,

                            parentNode: joint.parent,
                            force: joint.force,
                            isWheel: joint.isWheel,
                            speed: joint.speed,
                            signalType: joint.signalType,
                            wheelType: wheel?.wheelType ?? WheelType.STANDARD,
                            name: fusionJoint?.name ?? "",
                            type: fusionJoint?.jointType ?? JointType.RigidJointType,
                        }
                    }).filter((e) => e != null)
                    return res
                })
                updateGamepieces(() => {
                    const res: Gamepiece[] = data.options.gamepieces.map(gamepiece => {
                        const fusionGamepiece = data.gamepieceData.find(
                            g => g.occurrenceToken == gamepiece.occurrenceToken
                        )
                        if (fusionGamepiece == undefined) {
                            return null // No longer in the assembly
                        }
                        return {
                            occurrenceToken: gamepiece.occurrenceToken,
                            userDefinedMass: gamepiece.weight,
                            friction: gamepiece.friction,
                            calculatedMass: fusionGamepiece?.mass ?? 0,
                            name: fusionGamepiece?.name ?? "",
                            entityIDs: fusionGamepiece?.entityIDs ?? [],
                        }
                    }).filter((e) => e != null)
                    return res
                })
            })
        }
        fetchCb()
    }, [])
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
                cfg.joints = joints.map(joint => ({
                    jointToken: joint.id,
                    parent: joint.parentNode,
                    signalType: joint.signalType,
                    speed: joint.speed,
                    force: joint.force,
                    isWheel: joint.isWheel,
                }))
                cfg.wheels = joints
                    .filter(joint => joint.isWheel)
                    .map(joint => ({
                        jointToken: joint.id,
                        signalType: joint.signalType,
                        wheelType: joint.wheelType,
                    }))
                resolve(current(cfg))
            })
        })
    return (
        <>
            <GlobalAlert />
            <Button
                fullWidth
                style={{ margin: "0.5rem" }}
                variant="contained"
                onClick={async () => sendData("export", await getFinalizedConfig())}
                startIcon={<DownloadIcon />}>
                Export
            </Button>
            <Button
                fullWidth
                style={{ margin: "0.5rem" }}
                variant="contained"
                onClick={async () => sendData("save", await getFinalizedConfig())}
                startIcon={<DownloadIcon />}>
                Save
            </Button>
            <Box sx={{}}>
                <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
                    <Tabs
                        value={activeTab}
                        onChange={(_, v) => {
                            setActiveTab(v)
                        }}>
                        <Tab label="General" />
                        {generalConfig.exportMode == ExportMode.ROBOT ? (
                            <Tab label="Joints" />
                        ) : (
                            <Tooltip title={"Only available when configuring a dynamic assembly"}>
                                <span>
                                    <Tab label="Joints" disabled />
                                </span>
                            </Tooltip>
                        )}
                        {generalConfig.exportMode == ExportMode.FIELD ? (
                            <Tab label="Gamepieces" />
                        ) : (
                            <Tooltip title={"Only available when configuring a static assembly"}>
                                <span>
                                    <Tab label="Gamepieces" disabled />
                                </span>
                            </Tooltip>
                        )}
                        {/*<Tab label="APS" />*/}
                    </Tabs>
                </Box>
                <TabPanel value={activeTab} index={0}>
                    <GeneralConfigTab config={generalConfig} updateConfigItem={updateConfigItem} />
                </TabPanel>
                <TabPanel value={activeTab} index={1}>
                    <JointsConfigTab joints={joints} updateJoints={updateJoints} />
                </TabPanel>
                <TabPanel value={activeTab} index={2}>
                    <GamepiecesConfigTab
                        gamepieces={gamepieces}
                        updateGamepieces={updateGamepieces}
                        config={generalConfig}
                        updateConfigItem={updateConfigItem}
                    />
                </TabPanel>
                <TabPanel value={activeTab} index={3}>
                    <h3>APS</h3>
                    <button onClick={() => selectJoint()}>Select Joints</button>
                </TabPanel>
            </Box>
        </>
    )
}

export default App
