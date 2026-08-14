interface PaletteConfig {
    size: number
    startHue?: number
    saturation?: number
    lightness?: number
}

/**
 * Generates a palette deterministically so that it's the same on every startup.
 *
 * @return array of colors of form `hsl(h, s, v)`
 */
export const generatePalette = (config: PaletteConfig): string[] => {
    const { size, startHue = 0, saturation = 65, lightness = 55 } = config
    const palette: string[] = []

    const goldenRatioConjugate = 0.618033988749895
    let currentHue = startHue / 360

    for (let i = 0; i < size; i++) {
        const h = Math.round(currentHue * 360)
        palette.push(`hsl(${h}, ${saturation}%, ${lightness}%)`)
        currentHue = (currentHue + goldenRatioConjugate) % 1
    }

    return palette
}
