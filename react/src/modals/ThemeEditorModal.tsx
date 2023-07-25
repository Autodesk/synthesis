import React, { useState } from 'react'
import { FaChessBoard } from 'react-icons/fa6'
import Modal, { ModalPropsImpl } from '../components/Modal'
import { RgbaColorPicker } from 'react-colorful'
import Stack, { StackDirection } from '../components/Stack'
import Dropdown from '../components/Dropdown'
import Button from '../components/Button'

type Color = [red: number, green: number, blue: number, alpha: number];

const colors: { [name: string]: Color } = {
    "InteractiveElementSolid": [250, 162, 27, 255],
    "InteractiveElementLeft": [224, 130, 65, 255],
    "InteractiveElementRight": [218, 102, 89, 255],
    "InteractiveSecondary": [204, 124, 0, 255],
    "Background": [33, 37, 41, 255],
    "BackgroundSecondary": [52, 58, 64, 255],
    "MainText": [248, 249, 250, 255],
    "Scrollbar": [213, 216, 223, 255],
    "AcceptButton": [34, 139, 230, 255],
    "CancelButton": [250, 82, 82, 255],
    "InteractiveElementText": [0, 0, 0, 255],
    "Icon": [255, 255, 255, 255],
    "HighlightHover": [89, 255, 133, 255],
    "HighlightSelect": [255, 89, 133, 255],
    "SkyboxTop": [255, 255, 255, 255],
    "SkyboxBottom": [255, 255, 255, 255],
    "FloorGrid": [93, 93, 93, 255]
};

const ThemeEditorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [selectedColor, setSelectedColor] = useState("");
    const [currentColor, setCurrentColor] = useState<Color>([0, 0, 0, 0]);

    return (
        <Modal name="Theme Editor" icon={<FaChessBoard />} modalId={modalId}>
            <Stack direction={StackDirection.Horizontal}>
                <Stack direction={StackDirection.Vertical}>
                    <Dropdown label="Select a Theme" options={["Default", "Test 1"]} className="h-min" />
                    <Stack direction={StackDirection.Horizontal} spacing={10}>
                        <Button value="Create Theme" />
                        <Button value="Delete Selected" />
                        <Button value="Delete All" />
                    </Stack>
                    <RgbaColorPicker onChange={c => {
                        const color: Color = [c.r, c.g, c.b, c.a]
                        colors[selectedColor] = color;
                        setCurrentColor(color)
                    }} />
                </Stack>
                <div className="w-full h-full">
                    <div className="w-max m-4 h-full overflow-scroll">
                        {Object.entries(colors).map(([n, c]) => (
                            <div
                                key={n}
                                className={`flex flex-row gap-2 content-middle align-center cursor-pointer ${n == selectedColor ? "bg-gray-700" : ""}`} onClick={() => setSelectedColor(n)}>
                                <div className="w-6 h-6 rounded-md" style={{ background: `rgba(${c[0]}, ${c[1]}, ${c[2]}, ${c[3]})` }}></div>
                                <div className="h-6 text-white">{n}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </Stack>
        </Modal>
    )
}

export default ThemeEditorModal;
