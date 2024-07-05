import { createTheme } from "@mui/material";
import defaultColors from "@/SynthesisColorPalette";
import { RgbaColor } from "react-colorful";

function toHex(n: number) {
    let str = Math.min(255, Math.max(0, n)).toString(16)
    while (str.length < 2) {
        str = '0' + str
    }
    return str
}

function colorString(color: RgbaColor) {
    const str = `#${toHex(color.r)}${toHex(color.g)}${toHex(color.b)}${toHex(color.a * 255)}`
    console.debug(str)
    return str
}

export const interactiveElementSolidColor = "rgb(250, 162, 27)"
export const interactiveElementRightColor = "rgb(218, 102, 89)"
export const interactiveElementLeftColor = "rgb(224, 130, 65)"
export const interactiveBackgroundColor = "rgb(52, 58, 64)"
export const acceptButtonColor = "rgb(71, 138, 226)"
export const cancelButtonColor = "rgb(231, 85, 81)"

const defaultTheme = createTheme({
    components: {
        MuiButton: {
            styleOverrides: {
                root: ({ ownerState }) => ({
                    background: `linear-gradient(to right, ${interactiveElementLeftColor}, ${interactiveElementRightColor})`,
                    // borderRadius: '0.125rem',
                    color: 'white',
                    fontFamily: 'Artifakt Legend',
                    fontWeight: '600',
                    textTransform: 'none',
                    width: 'fit-content',
                    height: 'fit-content',
                    padding: ownerState.size == 'small' ?
                        '0.5rem 1rem' : ownerState.size == 'medium' ?
                            '0.75rem 1.25rem' :
                            '1rem 1.75rem',
                }),
            },
        }
    }
})

export default defaultTheme