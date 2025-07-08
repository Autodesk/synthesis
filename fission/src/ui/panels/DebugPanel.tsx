import Panel, { PanelPropsImpl } from "../components/Panel"
import Button from "../components/Button"
import World from "@/systems/World"
import { ToastType } from "../ToastContext"
import { Random } from "@/util/Random"
import MirabufCachingService, {
    backUpFields as hashedMiraFields,
    backUpRobots as hashedMiraRobots,
    MiraType,
} from "@/mirabuf/MirabufLoader"
import { Box, styled } from "@mui/material"
import { usePanelControlContext } from "../helpers/UsePanelManager"
import APS from "@/aps/APS"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Label from "../components/Label"
import { colorNameToVar } from "../helpers/UseThemeHelpers"
import { SynthesisIcons } from "../components/StyledComponents"
import { Global_AddToast } from "../components/GlobalUIControls"

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    marginTop: "0.5rem",
})

function ToggleDragMode() {
    const dragSystem = World.DragModeSystem
    if (dragSystem) {
        dragSystem.enabled = !dragSystem.enabled
        const status = dragSystem.enabled ? "enabled" : "disabled"
        Global_AddToast?.("info", "Drag Mode", `Drag mode has been ${status}`)
    }
}

const DebugPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel } = usePanelControlContext()

    return (
        <Panel
            openLocation="center"
            name={"Debug Tools"}
            icon={SynthesisIcons.BugLarge}
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
                            const type: ToastType = ["info", "warning", "error"][Math.floor(Random() * 3)] as ToastType
                            Global_AddToast?.(type, type, "This is a test toast to test the toast system")
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
                    <Button value={"Toggle Drag Mode"} onClick={ToggleDragMode} className="w-full" />
                    <Button
                        value={"Clear Preferences"}
                        onClick={() => PreferencesSystem.clearPreferences()}
                        className="w-full"
                    />

                    <LabelStyled>Autodesk Platform Services</LabelStyled>
                    <Button
                        value={"Refresh APS Token"}
                        onClick={async () =>
                            (await APS.isSignedIn()) && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
                        }
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
                            console.log(MirabufCachingService.GetCacheMap(MiraType.ROBOT))
                            console.log(MirabufCachingService.GetCacheMap(MiraType.FIELD))
                            console.log(hashedMiraRobots)
                            console.log(hashedMiraFields)
                        }}
                        className="w-full"
                    />
                    <Button
                        value={"Clear Mira Cache"}
                        onClick={() => MirabufCachingService.RemoveAll()}
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
