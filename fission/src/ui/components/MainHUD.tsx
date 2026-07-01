import type React from "react"
import { useIsMobile } from "@/ui/helpers/useIsMobile"
import MobileHUD from "./MobileHUD"
import TopBar from "./TopBar"

/**
 * Chooses the HUD for the current viewport: the desktop `TopBar` at/above the
 * MUI `md` breakpoint, or the mobile drawer (`MobileHUD`) below it. Unmounting
 * `TopBar` on mobile lets its cleanup reset the reserved scene offset to 0.
 */
const MainHUD: React.FC = () => {
    const isMobile = useIsMobile()
    return isMobile ? <MobileHUD /> : <TopBar />
}

export default MainHUD
