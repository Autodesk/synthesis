import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import Button from "@/components/Button.tsx"
import { useModalControlContext } from "../helpers/UseModalManager"
import { Global_AddToast } from "@/components/GlobalUIControls.ts"
import { SpawnCachedMira } from "@/ui/panels/mirabuf/ImportMirabufPanel";
import { MiraType } from "@/mirabuf/MirabufLoader";
import { GetCacheInfo } from "@/ui/panels/mirabuf/ImportMirabufPanel.tsx"
import MirabufCachingService, {
    backUpFields,
    backUpRobots,
    canOPFS,
    MirabufCacheInfo,
    MirabufRemoteInfo,
} from "@/mirabuf/MirabufLoader"


const MainMenuModal: React.FC<ModalPropsImpl & { startSingleplayerCallback: () => void }> = ({
    modalId,
    startSingleplayerCallback,
}) => {
    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Welcome"}
            icon={SynthesisIcons.Gamepad}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <div className="flex flex-col">
                <Button
                    value={"Singleplayer"}
                    onClick={() => {
                        closeModal()
                        startSingleplayerCallback()
                    }}
                    className="w-full my-1"
                />
                <Button
                    value={"Load Default"}
                    onClick={() => {
                        closeModal()
                        startSingleplayerCallback()
                        MirabufCachingService.CacheRemote("/api/mira/fields/FRC Field 2018_v13.mira", MiraType.FIELD)
                            .then(cacheInfoField => {
                                if (cacheInfoField) {
                                    SpawnCachedMira(cacheInfoField, MiraType.FIELD)
                                }
                            })
                        MirabufCachingService.CacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT)
                            .then(cacheInfoRobot => {
                                if (cacheInfoRobot) {
                                    SpawnCachedMira(cacheInfoRobot, MiraType.ROBOT)
                                }
                            })
                        
                            
                }

                    }
                    className="w-full my-1"
                />
                <Button
                    value={"Multiplayer"}
                    onClick={() => {
                        Global_AddToast?.("error", "Not Supported", "Multiplayer is not yet supported. Come back soon!")
                    }}
                    className="w-full mt-1 mb-3"
                />
            </div>
        </Modal>
    )
}

export default MainMenuModal
