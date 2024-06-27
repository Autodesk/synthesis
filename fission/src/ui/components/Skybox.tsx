/* eslint-disable @typescript-eslint/no-explicit-any */

import World from "@/systems/World";

const Skybox = () => {
    const currentTheme = (window as any).getTheme();
    if (World.SceneRenderer) { World.SceneRenderer.updateSkyboxColors(currentTheme); }
    return (
        <></>
    );
}

export default Skybox;