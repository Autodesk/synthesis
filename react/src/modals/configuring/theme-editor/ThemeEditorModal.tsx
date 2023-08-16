import React, { useState } from "react"
import { FaChessBoard } from "react-icons/fa6"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { RgbaColor, RgbaColorPicker } from "react-colorful"
import Stack, { StackDirection } from "../../../components/Stack"
import Dropdown from "../../../components/Dropdown"
import Button from "../../../components/Button"

type Theme = { [name: string]: RgbaColor }

const defaultColors: Theme = {
    InteractiveElementSolid: { r: 250, g: 162, b: 27, a: 255 },
    InteractiveElementLeft: { r: 224, g: 130, b: 65, a: 255 },
    InteractiveElementRight: { r: 218, g: 102, b: 89, a: 255 },
    InteractiveSecondary: { r: 204, g: 124, b: 0, a: 255 },
    Background: { r: 33, g: 37, b: 41, a: 255 },
    BackgroundSecondary: { r: 52, g: 58, b: 64, a: 255 },
    MainText: { r: 248, g: 249, b: 250, a: 255 },
    Scrollbar: { r: 213, g: 216, b: 223, a: 255 },
    AcceptButton: { r: 34, g: 139, b: 230, a: 255 },
    CancelButton: { r: 250, g: 82, b: 82, a: 255 },
    InteractiveElementText: { r: 0, g: 0, b: 0, a: 255 },
    Icon: { r: 255, g: 255, b: 255, a: 255 },
    HighlightHover: { r: 89, g: 255, b: 133, a: 255 },
    HighlightSelect: { r: 255, g: 89, b: 133, a: 255 },
    SkyboxTop: { r: 255, g: 255, b: 255, a: 255 },
    SkyboxBottom: { r: 255, g: 255, b: 255, a: 255 },
    FloorGrid: { r: 93, g: 93, b: 93, a: 255 },
}

const defaultThemes: { [name: string]: Theme } = {
    Default: defaultColors,
}

const ThemeEditorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [selectedColor, setSelectedColor] = useState("")
    const [selectedTheme, setSelectedTheme] = useState(
        Object.keys(defaultThemes)[0]
    )
    const [, setCurrentColor] = useState<RgbaColor>({ r: 0, g: 0, b: 0, a: 0 })
    const [themes, setThemes] = useState<{ [name: string]: Theme }>({
        ...defaultThemes,
    })

    console.log(selectedTheme, themes)

    return (
        <Modal name="Theme Editor" icon={<FaChessBoard />} modalId={modalId}>
            <Stack direction={StackDirection.Horizontal}>
                <Stack direction={StackDirection.Vertical}>
                    <Dropdown
                        label="Select a Theme"
                        options={Object.keys(themes)}
                        onSelect={opt => setSelectedTheme(opt)}
                        className="h-min"
                    />
                    <Stack direction={StackDirection.Horizontal} spacing={10}>
                        <Button
                            value="Create Theme"
                            onClick={() => {
                                const newThemeName = `${
                                    Object.keys(themes).length
                                }`
                                setThemes({
                                    ...themes,
                                    [newThemeName]: { ...defaultColors },
                                })
                                setSelectedTheme(newThemeName)
                            }}
                        />
                        <Button
                            value="Delete Selected"
                            onClick={() => {
                                const themesCopy = { ...themes }
                                delete themesCopy[selectedTheme]
                                setSelectedTheme(
                                    Object.keys(themes).filter(
                                        k => k !== selectedTheme
                                    )[0]
                                )
                                setThemes(themesCopy)
                            }}
                        />
                        <Button value="Delete All" />
                    </Stack>
                    <RgbaColorPicker
                        color={themes[selectedTheme][selectedColor]}
                        onChange={c => {
                            themes[selectedTheme][selectedColor] = c
                            setCurrentColor(c)
                        }}
                    />
                </Stack>
                <div className="w-full h-full">
                    <div className="w-max m-4 h-full overflow-y-scroll pr-4 flex flex-col">
                        {Object.entries(themes[selectedTheme]).map(([n, c]) => (
                            <div
                                key={n}
                                className={`flex flex-row gap-2 content-middle align-center cursor-pointer rounded-md p-1 ${
                                    n == selectedColor ? "bg-gray-700" : ""
                                }`}
                                onClick={() => setSelectedColor(n)}
                            >
                                <div
                                    className="w-6 h-6 rounded-md"
                                    style={{
                                        background: `rgba(${c.r}, ${c.g}, ${c.b}, ${c.a})`,
                                    }}
                                ></div>
                                <div className="h-6 text-white">{n}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </Stack>
        </Modal>
    )
}

export default ThemeEditorModal
