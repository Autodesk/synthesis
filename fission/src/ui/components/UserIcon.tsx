import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import { useEffect, useState } from "react"
import { SynthesisIcons } from "./StyledComponents"

interface UserIconProps {
    className: string
}

export function UserIcon({ className }: UserIconProps) {
    const [userInfo, setUserInfo] = useState(APS.userInfo)

    useEffect(() => {
        document.addEventListener(APS_USER_INFO_UPDATE_EVENT, () => setUserInfo(APS.userInfo))
    }, [])

    if (!userInfo) {
        return SynthesisIcons.Question
    } else {
        return <img src={userInfo.picture} className={`object-contain aspect-square ${className}`}></img>
    }
}
