import { useModalControlContext } from "@/ui/ModalContext"
import { ColorName, Theme, useTheme } from "@/ui/ThemeContext"
import Button from "@/components/Button"
import Dropdown from "@/components/Dropdown"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import { Random } from "@/util/Random"
import { extend as cdExtend, random as cdRandom, colord } from "colord"
import a11yPlugin from "colord/plugins/a11y"
import React, { useState } from "react"
import { HexColorInput, RgbaColor, RgbaColorPicker } from "react-colorful"
import { AiFillWarning } from "react-icons/ai"
import { FaChessBoard } from "react-icons/fa6"
cdExtend([a11yPlugin])

const ThemeEditorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const readabilityIndicator: boolean = false
    const { themes, initialThemeName, currentTheme, setTheme, updateColor, applyTheme } = useTheme()

    const { openModal } = useModalControlContext()

    const [selectedColor, setSelectedColor] = useState<ColorName>("InteractiveElementSolid")
    const [selectedTheme, setSelectedTheme] = useState<string>(currentTheme)
    const [, setCurrentColor] = useState<RgbaColor>({ r: 0, g: 0, b: 0, a: 0 })
    // needs to be useState so it doesn't get reset on re-render
    const [initialThemeValues] = useState<Theme>(JSON.parse(JSON.stringify(themes[selectedTheme])))

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
                <Stack direction={StackDirection.Vertical} align="center" justify="between" className="w-1/2">
                    <Stack direction={StackDirection.Vertical}>
                        <Dropdown
                            label="Select a Theme"
                            options={[currentTheme, ...Object.keys(themes).filter(t => t != currentTheme)]}
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
                            {false && (
                                <Button
                                    value="Delete All"
                                    onClick={() => {
                                        openModal("delete-all-themes")
                                    }}
                                />
                            )}
                        </Stack>
                    </Stack>
                    <Stack direction={StackDirection.Vertical}>
                        <RgbaColorPicker
                            color={
                                themes[selectedTheme]
                                    ? themes[selectedTheme][selectedColor].color
                                    : ({ r: 0, g: 0, b: 0, a: 0 } as RgbaColor)
                            }
                            onChange={c => {
                                if (selectedTheme == initialThemeName) return
                                setCurrentColor(c)
                                updateColor(selectedTheme, selectedColor, c)
                            }}
                        />
                        <HexColorInput
                            style={{
                                display: "block",
                                boxSizing: "border-box",
                                width: "90px",
                                margin: "10px 55px 0",
                                padding: "6px",
                                border: "1px solid #ddd",
                                borderRadius: "4px",
                                background: "#eee",
                                outline: "none",
                                font: "inherit",
                                textTransform: "uppercase",
                                textAlign: "center",
                                color: "black",
                            }}
                            color={colord(
                                themes[selectedTheme]
                                    ? themes[selectedTheme][selectedColor].color
                                    : ({ r: 0, g: 0, b: 0, a: 0 } as RgbaColor)
                            ).toHex()}
                            onChange={c => {
                                if (selectedTheme == initialThemeName) return
                                const color = colord(c).toRgb()
                                setCurrentColor(color)
                                updateColor(selectedTheme, selectedColor, color)
                            }}
                        />
                        <Button
                            value="Randomize Theme"
                            onClick={() => {
                                if (selectedTheme == initialThemeName) return
                                const keys: ColorName[] = Object.keys(themes[selectedTheme]) as ColorName[]
                                keys.forEach(k => {
                                    const randAlpha = () => Math.max(0.1, Random())
                                    updateColor(selectedTheme, k, {
                                        ...cdRandom().toRgb(),
                                        a: randAlpha(),
                                    } as RgbaColor)
                                })
                                applyTheme(selectedTheme)
                                setSelectedTheme(selectedTheme)
                                setCurrentColor(
                                    selectedTheme && themes[selectedTheme] && selectedColor
                                        ? themes[selectedTheme][selectedColor].color
                                        : { r: 0, g: 0, b: 0, a: 0 }
                                )
                                setSelectedColor(selectedColor)
                            }}
                        />
                    </Stack>
                </Stack>
                <div className="w-full h-full">
                    <div className="w-max m-4 h-full overflow-y-scroll pr-4 flex flex-col">
                        {Object.entries(themes[selectedTheme]).map(([n, c]) => (
                            <div
                                key={n}
                                className={`${currentTheme == initialThemeName ? "cursor-not-allowed" : "cursor-pointer"} flex flex-row gap-2 content-middle align-center cursor-pointer rounded-md p-1 ${
                                    n == selectedColor ? "bg-background-secondary" : ""
                                }`}
                                title={currentTheme == initialThemeName ? "Cannot edit the default theme" : undefined}
                                onClick={() => {
                                    if (currentTheme == initialThemeName) return
                                    setSelectedColor(n as ColorName)
                                }}
                            >
                                <div
                                    className={`w-6 h-6 rounded-md`}
                                    style={{
                                        background: `rgba(${c.color.r}, ${c.color.g}, ${c.color.b}, ${c.color.a})`,
                                    }}
                                ></div>
                                <div className="h-6 text-main-text">{n}</div>
                                {readabilityIndicator &&
                                    (() => {
                                        if (
                                            c.color.a < 0.1 ||
                                            ((n.toLowerCase().includes("text") || n.toLowerCase().includes("icon")) &&
                                                !n.toLowerCase().includes("closeicon"))
                                        ) {
                                            const aboveColors = c.above.map((above: ColorName | string) =>
                                                typeof above == "string" ? above : themes[selectedTheme][above]
                                            )
                                            const conflicting = aboveColors
                                                .map((above: string | RgbaColor) => [
                                                    above,
                                                    colord(c.color).isReadable(above),
                                                ])
                                                .filter(c => !c[1])
                                                .map(([n]) => n)

                                            if (
                                                currentTheme != initialThemeName &&
                                                (conflicting.length > 0 || c.color.a < 0.1)
                                            )
                                                return (
                                                    <div
                                                        className="flex flex-col w-6 align-center justify-center text-toast-warning"
                                                        title={
                                                            c.color.a < 0.1
                                                                ? "Alpha too low!"
                                                                : `This color will not be readable on top of ${conflicting.join(", ")}!`
                                                        }
                                                    >
                                                        <AiFillWarning />
                                                    </div>
                                                )
                                            else return <div className="w-6" />
                                        }
                                    })()}
                            </div>
                        ))}
                    </div>
                </div>
            </Stack>
        </Modal>
    )
}

export default ThemeEditorModal
