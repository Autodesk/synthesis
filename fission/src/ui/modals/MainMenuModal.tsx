import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import Button from "@/components/Button.tsx"
import { useModalControlContext } from "../helpers/UseModalManager"
import { Global_AddToast } from "@/components/GlobalUIControls.ts"
import { SpawnCachedMira } from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { GetCacheInfo } from "@/ui/panels/mirabuf/ImportMirabufPanel.tsx"
import { MiraType } from "@/mirabuf/MirabufLoader"

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
                        const allFields = GetCacheInfo(MiraType.FIELD)
                        const displayField = allFields.find(field => field.id === "1750390764392") // field name:
                        const allRobots = GetCacheInfo(MiraType.ROBOT)
                        const displayRobot = allRobots.find(robot => robot.id === "1750914383413")

                        if (displayField) {
                            SpawnCachedMira(displayField, MiraType.FIELD)
                            if (displayRobot) {
                                SpawnCachedMira(displayRobot, MiraType.ROBOT)
                            }
                        }
                    }}
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
