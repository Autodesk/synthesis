import World from "@/systems/World"
import { useTheme } from "../ThemeContext"

const Skybox = () => {
    const { currentTheme, themes } = useTheme()
    if (World.SceneRenderer) {
        World.SceneRenderer.UpdateSkyboxColors(themes[currentTheme])
    }
    return <></>
}

export default Skybox
