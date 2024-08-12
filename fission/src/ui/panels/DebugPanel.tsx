import Panel, { PanelPropsImpl } from "../components/Panel"
import Button from "../components/Button"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { MainHUD_AddToast } from "../components/MainHUD"
import { ToastType } from "../ToastContext"
import { Random } from "@/util/Random"
import MirabufCachingService, {
    backUpFields as hashedMiraFields,
    backUpRobots as hashedMiraRobots,
    MiraType,
} from "@/mirabuf/MirabufLoader"
import { Box, styled } from "@mui/material"
import { usePanelControlContext } from "../PanelContext"
import APS from "@/aps/APS"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import Jolt from "@barclah/jolt-physics"
import Label from "../components/Label"
import { colorNameToVar } from "../ThemeContext"
import { SynthesisIcons } from "../components/StyledComponents"
import { useModalControlContext } from "../ModalContext"

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    marginTop: "0.5rem",
})

async function TestGodMode() {
    const robot: MirabufSceneObject = [...World.SceneRenderer.sceneObjects.entries()]
        .filter(x => {
            const y = x[1] instanceof MirabufSceneObject
            return y
        })
        .map(x => x[1])[0] as MirabufSceneObject
    const rootNodeId = robot.GetRootNodeId()
    if (rootNodeId == undefined) {
        console.error("Robot root node not found for god mode")
        return
    }
    const robotPosition = World.PhysicsSystem.GetBody(rootNodeId).GetPosition()
    const [ghostBody, _ghostConstraint] = World.PhysicsSystem.CreateGodModeBody(rootNodeId, robotPosition as Jolt.Vec3)

    // Move ghostBody to demonstrate godMode movement
    await new Promise(f => setTimeout(f, 1000))
    World.PhysicsSystem.SetBodyPosition(
        ghostBody.GetID(),
        new JOLT.Vec3(robotPosition.GetX(), robotPosition.GetY() + 2, robotPosition.GetZ())
    )
    await new Promise(f => setTimeout(f, 1000))
    World.PhysicsSystem.SetBodyPosition(ghostBody.GetID(), new JOLT.Vec3(2, 2, 2))
}

const DebugPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    return (
        <Panel
            openLocation="center"
            name={"Debug Panel"}
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
                            MainHUD_AddToast(type, type, "This is a test toast to test the toast system")
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
                    <Button value={"Test God Mode"} onClick={TestGodMode} className="w-full" />
                    <Button
                        value={"Clear Preferences"}
                        onClick={() => PreferencesSystem.clearPreferences()}
                        className="w-full"
                    />

                    <LabelStyled>Autodesk Platform Services</LabelStyled>
                    <Button
                        value={"Refresh APS Token"}
                        onClick={async () =>
                            APS.isSignedIn() && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
                        }
                        className="w-full"
                    />
                    <Button
                        value={"Expire APS Token"}
                        onClick={() => {
                            if (APS.isSignedIn()) {
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
                    <Button
                        value={"WS Test"}
                        onClick={() => {
                            // worker?.postMessage({ command: 'connect' });
                            const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(
                                x => x[1] instanceof MirabufSceneObject
                            )
                            console.log(`Number of mirabuf scene objects: ${miraObjs.length}`)
                            if (miraObjs.length > 0) {
                                const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
                                const simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
                                simLayer?.SetBrain(new WPILibBrain(mechanism))
                            }
                        }}
                        className="w-full"
                    />
                    <Button value={"WS Viewer"} onClick={() => openPanel("ws-view")} className="w-full" />
                    <Button value={"RoboRIO"} onClick={() => openModal("roborio")} className="w-full" />
                </Box>
            </Box>
        </Panel>
    )
}

export default DebugPanel
