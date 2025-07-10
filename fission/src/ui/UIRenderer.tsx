import { Modal } from "@/components/Modal"
import { Panel } from "@/components/Panel"
import { useContext, useEffect } from "react"
import type React from "react"
import { ThemeContext } from "./ThemeProvider"
import { UIContext } from "./UIProvider"
import AutoTestPanel from "./panels/AutoTestPanel"
import ScoreboardPanel from "./panels/ScoreboardPanel"
import PokerPanel from "./panels/PokerPanel"
import ChooseSchemePanel from "./panels/configuring/ChooseSchemePanel"
import WiringPanel from "./panels/simulation/WiringPanel"
import ConfigurePanel from "./panels/configuring/assembly-config/ConfigurePanel"
import MirabufCachingService, { MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import World from "@/systems/World"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem"

export type UIRendererProps = object // TODO: add actual props or delete

async function spawnCachedMira(info: MirabufCacheInfo, type: MiraType) {
    // If spawning a field, then remove all other fields
    if (type === MiraType.FIELD) {
        World.SceneRenderer.RemoveAllFields()
    }

    World.PhysicsSystem.HoldPause(PAUSE_REF_ASSEMBLY_SPAWNING)
    await MirabufCachingService.Get(info.id, type)
        .then(assembly => {
            if (assembly) {
                CreateMirabuf(assembly).then(x => {
                    if (x) {
                        World.SceneRenderer.RegisterSceneObject(x)
                    }
                })

                if (!info.name) MirabufCachingService.CacheInfo(info.cacheKey, type, assembly.info?.name ?? undefined)
            } else {
                console.error("Failed to spawn robot")
            }
        })
        .finally(() => {
            setTimeout(() => World.PhysicsSystem.ReleasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
        })
}

export const UIRenderer: React.FC<UIRendererProps> = () => {
    const { modal, openModal: _openModal, panels, openPanel, enqueueSnackbar } = useContext(UIContext)

    const { mode, toggleColorMode, primaryColor, secondaryColor, setPrimaryColor, setSecondaryColor } =
        useContext(ThemeContext)

    // TODO: figure this out

    // TODO: remove default panel
    // biome-ignore lint/correctness/useExhaustiveDependencies: adding deps will trigger a refresh loop
    useEffect(() => {
        MirabufCachingService.CacheRemote(
            "https://synthesis.autodesk.com/api/mira/robots/Dozer_v9.mira",
            MiraType.ROBOT
        ).then(cacheInfo => {
            if (cacheInfo)
                spawnCachedMira(cacheInfo, MiraType.ROBOT).then(() => {
                    console.log(
                        `opening test panel ${openPanel(<ConfigurePanel />, undefined, "top-right", {
                            onClose: () => console.log("closed test panel"),
                            onAccept: () => enqueueSnackbar("ACCEPTED!", { variant: "success" }),
                            onCancel: () => enqueueSnackbar("CANCELED!", { variant: "error" }),
                        })}`
                    )
                })
        })
    }, [])
    return (
        <>
            <div id="panel-container" className="relative pointer-events-none w-[100vw] h-[100vh]">
                {panels.map((p, _i) => (
                    <Panel key={`panel-${p.id}`} panel={p}>
                        {p.content}
                    </Panel>
                ))}
            </div>
            <Modal modal={modal}>{modal?.content}</Modal>
        </>
    )
}
