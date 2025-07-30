import React from "react"
import Label, { LabelSize } from "@/components/Label"
import LabeledButton, { LabelPlacement } from "@/components/LabeledButton"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import { SynthesisIcons } from "../components/StyledComponents"

const DownloadAssetsModal: React.FC<ModalPropsImpl> = ({ modalId }) => (
    <Modal name={"Download Assets"} icon={SynthesisIcons.IMPORT} modalId={modalId}>
        <Stack direction={StackDirection.HORIZONTAL} spacing={10}>
            <Stack direction={StackDirection.VERTICAL} spacing={10}>
                <Label size={LabelSize.LARGE}>Robot Assets</Label>
                <LabeledButton label={"5975 Ziggy (2022)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"2374 Dogs (2022)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"1234 Bark (2012)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"1213 Coffee (2021)"} value={"Download"} placement={LabelPlacement.LEFT} />
            </Stack>
            <Stack direction={StackDirection.VERTICAL} spacing={10}>
                <Label size={LabelSize.LARGE}>Field Assets</Label>
                <LabeledButton label={"5975 Ziggy (2022)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"2374 Dogs (2022)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"1234 Bark (2012)"} value={"Download"} placement={LabelPlacement.LEFT} />
                <LabeledButton label={"1213 Coffee (2021)"} value={"Download"} placement={LabelPlacement.LEFT} />
            </Stack>
        </Stack>
    </Modal>
)

export default DownloadAssetsModal
