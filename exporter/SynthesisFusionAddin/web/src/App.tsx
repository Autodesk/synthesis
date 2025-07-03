import { useState } from "react"

import "./App.css"
import { selectJoint } from "./lib"
import { Box, Container, Tab, Tabs, Tooltip } from "@mui/material"
import GeneralConfigTab from "./ui/GeneralConfigTab.tsx"
import JointsConfigTab from "./ui/JointsConfigTab.tsx"
import { useImmer } from "use-immer"
import { DefaultExporterConfig, type ExporterConfig, type Gamepiece, type Joint } from "./lib/types.ts"
import GamepiecesConfigTab from "./ui/GamepiecesConfigTab.tsx"

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
    const updateConfigItem = <K extends keyof ExporterConfig>(k: K, v: ExporterConfig[K]) => {
        updateGeneralConfig(config => {
            config[k] = v
        })
    }
    return (
        <Box sx={{}}>
            <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
                <Tabs
                    value={activeTab}
                    onChange={(_, v) => {
                        setActiveTab(v)
                    }}>
                    <Tab label="General" />
                    {generalConfig.mode == "ROBOT" ? (
                        <Tab label="Joints" />
                    ) : (
                        <Tooltip title={"Only available when configuring a dynamic assembly"}>
                            <span>
                                <Tab label="Joints" disabled />
                            </span>
                        </Tooltip>
                    )}
                    {generalConfig.mode == "FIELD" ? (
                        <Tab label="Gamepieces" />
                    ) : (
                        <Tooltip title={"Only available when configuring a static assembly"}>
                            <span>
                                <Tab label="Gamepieces" disabled />
                            </span>
                        </Tooltip>
                    )}
                    <Tab label="APS" />
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
    )
}

export default App
