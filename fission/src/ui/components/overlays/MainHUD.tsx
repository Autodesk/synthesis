import type React from "react"
import { useIsMobile } from "@/ui/helpers/useIsMobile.ts"
import MobileHUD from "./MobileHUD.tsx"
import TopBar from "../TopBar.tsx"

/** Choosing between mobile HUD and Topbar for desktop. */
const MainHUD: React.FC = () => {
    const isMobile = useIsMobile()
    return isMobile ? <MobileHUD /> : <TopBar />
}

export default MainHUD
