import { Box, styled } from "@mui/material"
import APS from "@/aps/APS"
import MirabufCachingService, { backUpMap, MiraType } from "@/mirabuf/MirabufLoader"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import { random } from "@/util/Random"
import Button from "../components/Button"
import { globalAddToast } from "../components/GlobalUIControls"
import Label from "../components/Label"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"
import { usePanelControlContext } from "../helpers/UsePanelManager"
import { colorNameToVar } from "../helpers/UseThemeHelpers"
import { ToastType } from "../ToastContext"

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    marginTop: "0.5rem",
})

function toggleDragMode() {
    const dragSystem = World.dragModeSystem
    if (dragSystem) {
        dragSystem.enabled = !dragSystem.enabled
        const status = dragSystem.enabled ? "enabled" : "disabled"
        globalAddToast("info", "Drag Mode", `Drag mode has been ${status}`)
    }
}

const DebugPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel } = usePanelControlContext()

    return (
        <Panel
            openLocation="center"
            name={"Debug Tools"}
            icon={SynthesisIcons.BUG_LARGE}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Close"
        >
            <Box
                component="div"
                alignItems="center"
                sx={{
                    padding: "0.25rem",
                    overflowY: "auto",
                    borderRadius: "0.5rem",
                    backgroundColor: colorNameToVar("BackgroundSecondary"),
                }}
                justifyContent={"center"}
                textAlign={"center"}
                minWidth={"290px"}
            >
                <Box display="flex" flexDirection={"column"} gap="0.25rem" width={"80%"} margin={"auto"}>
                    <LabelStyled>Generic</LabelStyled>
                    <Button
                        value={"Toasts"}
                        onClick={() => {
                            const type: ToastType = ["info", "warning", "error"][Math.floor(random() * 3)] as ToastType
                            globalAddToast(type, type, "This is a test toast to test the toast system")
                        }}
                        className="w-full"
                    />
                    <Button
                        value={"The Poker"}
                        onClick={() => {
                            openPanel("poker")
                        }}
                        className="w-full"
                    />
                    <Button value={"Toggle Drag Mode"} onClick={toggleDragMode} className="w-full" />
                    <Button
                        value={"Clear Preferences"}
                        onClick={() => PreferencesSystem.clearPreferences()}
                        className="w-full"
                    />

                    <LabelStyled>Autodesk Platform Services</LabelStyled>
                    <Button
                        value={"Refresh APS Token"}
                        onClick={async () => {
                            const auth = await APS.getAuth()
                            auth && APS.refreshAuthToken(auth.refresh_token, true)
                        }}
                        className="w-full"
                    />
                    <Button
                        value={"Expire APS Token"}
                        onClick={async () => {
                            if (await APS.isSignedIn()) {
                                APS.setExpiresAt(Date.now())
                                APS.getAuthOrLogin()
                            }
                        }}
                        className="w-full"
                    />

                    <LabelStyled>Caching Service</LabelStyled>
                    <Button
                        value={"Print Mira Maps"}
                        onClick={() => {
                            console.log(MirabufCachingService.getCacheMap(MiraType.ROBOT))
                            console.log(MirabufCachingService.getCacheMap(MiraType.FIELD))
                            console.log(backUpMap[MiraType.ROBOT])
                            console.log(backUpMap[MiraType.FIELD])
                        }}
                        className="w-full"
                    />
                    <Button
                        value={"Clear Mira Cache"}
                        onClick={() => MirabufCachingService.removeAll()}
                        className="w-full"
                    />

                    <LabelStyled>Code Simulation</LabelStyled>
                    <Button value={"WS Viewer"} onClick={() => openPanel("ws-view")} className="w-full" />
                </Box>
            </Box>
        </Panel>
    )
}

export default DebugPanel
