import type React from "react"
import { useIsMobile } from "@/ui/helpers/useIsMobile"
import MobileHUD from "./MobileHUD"
import TopBar from "./TopBar"

/** Choosing between mobile HUD and Topbar for desktop. */
const MainHUD: React.FC = () => {
    const isMobile = useIsMobile()
    return isMobile ? <MobileHUD /> : <TopBar />
}

export default MainHUD
