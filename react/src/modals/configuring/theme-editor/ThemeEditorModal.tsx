import { useModalControlContext } from "@/ModalContext"
import {
    ColorName,
    Theme,
    useTheme
} from "@/ThemeContext"
import Button from "@/components/Button"
import Dropdown from "@/components/Dropdown"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import React, { useState } from "react"
import { RgbaColor, RgbaColorPicker } from "react-colorful"
import { FaChessBoard } from "react-icons/fa6"

const ThemeEditorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { themes, currentTheme, setTheme, updateColor, applyTheme } =
        useTheme()

    const { openModal } = useModalControlContext()

    const [selectedColor, setSelectedColor] = useState<ColorName>(
        "InteractiveElementSolid"
    )
    const [selectedTheme, setSelectedTheme] = useState<string>(currentTheme)
    const [currentColor, setCurrentColor] = useState<RgbaColor>({ r: 0, g: 0, b: 0, a: 0 })
    // needs to be useState so it doesn't get reset on re-render
    const [initialThemeValues] = useState<Theme>({ ...themes[selectedTheme] })

    return (
        <Modal
            name="Theme Editor"
            icon={<FaChessBoard />}
            modalId={modalId}
            middleEnabled={true}
            middleName="Preview"
            onAccept={() => {
                applyTheme(selectedTheme)
                setTheme(selectedTheme)
            }}
            onMiddle={() => {
                applyTheme(selectedTheme)
            }}
            onCancel={() => {
                themes[currentTheme] = initialThemeValues
                applyTheme(currentTheme)
            }}
        >
            <Stack direction={StackDirection.Horizontal}>
                <Stack direction={StackDirection.Vertical}>
                    <Dropdown
                        label="Select a Theme"
                        options={[
                            currentTheme,
                            ...Object.keys(themes).filter(
                                t => t != currentTheme
                            ),
                        ]}
                        onSelect={setSelectedTheme}
                        className="h-min"
                    />
                    <Stack direction={StackDirection.Horizontal} spacing={10}>
                        <Button
                            value="Create Theme"
                            onClick={() => {
                                openModal("new-theme")
                            }}
                        />
                        <Button
                            value="Delete Selected"
                            onClick={() => {
                                openModal("delete-theme")
                            }}
                        />
                        <Button
                            value="Delete All"
                            onClick={() => {
                                openModal("delete-all-themes")
                            }}
                        />
                    </Stack>
                    <RgbaColorPicker
                        color={
                            themes[selectedTheme]
                                ? themes[selectedTheme][selectedColor]
                                : { r: 0, g: 0, b: 0, a: 0 }
                        }
                        onChange={c => {
                            if (selectedTheme == "Default") return
                            setCurrentColor(c)
                            updateColor(selectedTheme, selectedColor, c)
                        }}
                    />
                    <p>{JSON.stringify(currentColor)}</p>
                </Stack>
                <div className="w-full h-full">
                    <div className="w-max m-4 h-full overflow-y-scroll pr-4 flex flex-col">
                        {Object.entries(themes[selectedTheme]).map(([n, c]) => (
                            <div
                                key={n}
                                className={`flex flex-row gap-2 content-middle align-center cursor-pointer rounded-md p-1 ${n == selectedColor ? "bg-background-secondary" : ""
                                    }`}
                                onClick={() => {
                                    setSelectedColor(n as ColorName)
                                }}
                            >
                                <div
                                    className={`w-6 h-6 rounded-md`}
                                    style={{
                                        background: `rgba(${c.r}, ${c.g}, ${c.b}, ${c.a})`
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
