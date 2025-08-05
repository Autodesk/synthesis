import type React from "react"
import { useEffect, useState } from "react"
import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import { SynthesisIcons } from "./StyledComponents"

interface UserIconProps {
    className: string
}

const UserIcon: React.FC<UserIconProps> = ({ className }) => {
    const [userInfo, setUserInfo] = useState(APS.userInfo)

    useEffect(() => {
        document.addEventListener(APS_USER_INFO_UPDATE_EVENT, () => setUserInfo(APS.userInfo))
    }, [])

    if (!userInfo) {
        return SynthesisIcons.QUESTION
    } else {
        return <img src={userInfo.picture} className={`object-contain aspect-square ${className}`}></img>
    }
}

export default UserIcon
