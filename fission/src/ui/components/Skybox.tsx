import World from "@/systems/World"
import { useTheme } from "@/ui/ThemeContext"

const Skybox = () => {
    const { currentTheme, themes } = useTheme()
    if (World.SceneRenderer) {
        World.SceneRenderer.updateSkyboxColors(themes[currentTheme])
    }
    return <></>
}

export default Skybox
