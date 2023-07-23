import React from 'react'
import Modal from '../components/Modal';
import Stack, { StackDirection } from '../components/Stack';
import Label, { LabelSize } from '../components/Label'
import LabeledButton, { LabelPlacement } from '../components/LabeledButton'

const DownloadAssetsModal: React.FC = () => (
    <Modal name={"Download Assets"} icon="https://placeholder.co/512x512">
        <Stack direction={StackDirection.Horizontal} spacing={10}>
            <Stack direction={StackDirection.Vertical} spacing={10}>
                <Label size={LabelSize.Large}>Robot Assets</Label>
                <LabeledButton label={"5975 Ziggy (2022)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"2374 Dogs (2022)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"1234 Bark (2012)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"1213 Coffee (2021)"} value={"Download"} placement={LabelPlacement.Left} />
            </Stack>
            <Stack direction={StackDirection.Vertical} spacing={10}>
                <Label size={LabelSize.Large}>Field Assets</Label>
                <LabeledButton label={"5975 Ziggy (2022)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"2374 Dogs (2022)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"1234 Bark (2012)"} value={"Download"} placement={LabelPlacement.Left} />
                <LabeledButton label={"1213 Coffee (2021)"} value={"Download"} placement={LabelPlacement.Left} />
            </Stack>
        </Stack>
    </Modal >
)

export default DownloadAssetsModal;
