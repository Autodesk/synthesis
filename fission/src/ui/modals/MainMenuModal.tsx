import React from "react"
import Button from "@/components/Button.tsx"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import { useModalControlContext } from "../helpers/UseModalManager"
import MirabufCachingService, {MirabufCacheInfo, MiraType} from "@/mirabuf/MirabufLoader.ts";
import {spawnCachedMira} from "@/panels/mirabuf/ImportMirabufPanel.tsx";

const MainMenuModal: React.FC<ModalPropsImpl & { startSingleplayerCallback: () => void }> = ({
    modalId,
    startSingleplayerCallback,
}) => {
    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Welcome"}
            icon={SynthesisIcons.GAMEPAD}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <div className="flex flex-col mb-3">
                <Button
                    value={"Singleplayer"}
                    onClick={() => {
                        closeModal()
                        startSingleplayerCallback()
                    }}
                    className="w-full my-1"
                />
                <Button
                    value={"Multiplayer"}
                    onClick={() => {
                        globalAddToast("error", "Not Supported", "Multiplayer is not yet supported. Come back soon!")
                    }}
                    className="w-full my-1"
                />
                {import.meta.env.DEV && <Button
                    value={"Load Dev"}
                    onClick={() => {
                        const params = new URLSearchParams(window.location.search)
                        const promises:Promise<MirabufCacheInfo|undefined>[] = []
                        const field = params.get("field")
                        if (field) {
                            promises.push(MirabufCachingService.cacheRemote(
                                field,
                                MiraType.FIELD
                            ))
                        }
                        const robots = params.getAll("robot")
                        robots.forEach((robot) => {
                            promises.push(MirabufCachingService.cacheRemote(
                                robot,
                                MiraType.ROBOT
                            ))
                        })
                        closeModal()
                        startSingleplayerCallback()
                        Promise.all(promises).then((cached) => {
                            cached.forEach((item) => {
                                if (item) {
                                    spawnCachedMira(item, item.miraType)
                                }
                            })
                        })
                    }}
                    className="w-full my-1"
                />
                }
            </div>
        </Modal>
    )
}

export default MainMenuModal
