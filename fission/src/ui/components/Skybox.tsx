import World from "@/systems/World"
import { useTheme } from "@/ui/helpers/UseThemeHelpers"

const Skybox = () => {
    const { currentTheme, themes } = useTheme()
    if (World.sceneRenderer) {
        World.sceneRenderer.updateSkyboxColors(themes[currentTheme])
    }
    return <></>
}

export default Skybox
