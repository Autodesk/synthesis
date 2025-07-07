import { useEffect, useState } from "react"

import "./App.css"
import { selectJoint, sendData } from "./lib"
import { Box, Button, Container, Tab, Tabs, Tooltip } from "@mui/material"
import GeneralConfigTab from "./ui/GeneralConfigTab.tsx"
import JointsConfigTab from "./ui/JointsConfigTab.tsx"
import { useImmer } from "use-immer"
import {
    DefaultExporterConfig,
    ExportMode,
    type Gamepiece,
    type GeneralConfig,
    type Joint,
    JointParentType,
} from "./lib/types.ts"
import GamepiecesConfigTab from "./ui/GamepiecesConfigTab.tsx"
import GlobalAlert from "./ui/GlobalAlert.tsx"
import { Global_SetAlert } from "./lib/GlobalUtils.tsx"
import DownloadIcon from "@mui/icons-material/Download"
import { original } from "immer"

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
        setTimeout(() => {
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
            })
        }, 500)
    }, [])
    const triggerExport = async () => {

        updateGeneralConfig((cfg) => {
        cfg.gamepieces = gamepieces.map(gamepiece => ({
            occurrenceToken: gamepiece.entityToken,
            weight: generalConfig.autoCalcGamepieceWeight ? gamepiece.calculatedMass : gamepiece.userDefinedMass,
            friction: gamepiece.friction,
        }))
        cfg.joints = joints.map(joint => ({
            jointToken: joint.id,
            parent: joint.parentNode == "root" ? JointParentType.ROOT : JointParentType.END, // What is this for?
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
        console.log(cfg)
        })
        await sendData("export", generalConfig)
    }
    return (
        <>
            <GlobalAlert />
            <Button
                fullWidth
                style={{ marginTop: "3rem" }}
                variant="contained"
                onClick={triggerExport}
                startIcon={<DownloadIcon />}>
                Export
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
