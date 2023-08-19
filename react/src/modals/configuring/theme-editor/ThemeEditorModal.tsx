import React, { useState } from "react"
import { FaChessBoard } from "react-icons/fa6"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { RgbaColor, RgbaColorPicker } from "react-colorful"
import Stack, { StackDirection } from "../../../components/Stack"
import Dropdown from "../../../components/Dropdown"
import Button from "../../../components/Button"
import { ColorName, useTheme } from "../../../ThemeContext"

const ThemeEditorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const {
        themes,
        currentTheme,
        defaultTheme,
        setTheme,
        updateColor,
        createTheme,
        deleteTheme,
        deleteAllThemes,
        applyTheme
    } = useTheme()

    const [selectedColor, setSelectedColor] = useState<ColorName>("InteractiveElementSolid")
    const [, setCurrentColor] = useState<RgbaColor>({ r: 0, g: 0, b: 0, a: 0 })

    return (
        <Modal name="Theme Editor" icon={<FaChessBoard />} modalId={modalId} onAccept={() => { applyTheme(currentTheme); setTheme(currentTheme) }}>
            <Stack direction={StackDirection.Horizontal}>
                <Stack direction={StackDirection.Vertical}>
                    <Dropdown
                        label="Select a Theme"
                        options={Object.keys(themes)}
                        onSelect={opt => setTheme(opt)}
                        className="h-min"
                    />
                    <Stack direction={StackDirection.Horizontal} spacing={10}>
                        <Button
                            value="Create Theme"
                            onClick={() => {
                                const newThemeName = `${Object.keys(themes).length}`
                                createTheme(newThemeName)
                                setTheme(newThemeName)
                            }}
                        />
                        <Button
                            value="Delete Selected"
                            onClick={() => {
                                deleteTheme(currentTheme)
                            }}
                        />
                        <Button
                            value="Delete All"
                            onClick={() => {
                                deleteAllThemes()
                            }}
                        />
                    </Stack>
                    <RgbaColorPicker
                        color={themes[currentTheme] ? themes[currentTheme][selectedColor] : { r: 0, g: 0, b: 0, a: 0 }}
                        onChange={c => {
                            if (currentTheme == "Default") return;
                            setCurrentColor(c)
                            updateColor(currentTheme, selectedColor, c)
                        }}
                    />
                </Stack>
                <div className="w-full h-full">
                    <div className="w-max m-4 h-full overflow-y-scroll pr-4 flex flex-col">
                        {Object.entries(themes[currentTheme]).map(([n, c]) => (
                            <div
                                key={n}
                                className={`flex flex-row gap-2 content-middle align-center cursor-pointer rounded-md p-1 ${n == selectedColor ? "bg-gray-700" : ""
                                    }`}
                                onClick={() => {
                                    setSelectedColor(n as ColorName)
                                }}
                            >
                                <div
                                    className="w-6 h-6 rounded-md"
                                    style={{
                                        background: `rgba(${c.r}, ${c.g}, ${c.b}, ${c.a})`,
                                    }}
                                ></div>
                                <div className="h-6 text-main-text">{n}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </Stack>
        </Modal>
    )
}

export default ThemeEditorModal
