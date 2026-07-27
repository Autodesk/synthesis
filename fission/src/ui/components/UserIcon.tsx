import type React from "react"
import { useEffect, useState } from "react"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import { SynthesisIcons } from "./StyledComponents"

interface UserIconProps {
    className: string
}

const UserIcon: React.FC<UserIconProps> = ({ className }) => {
    const [userInfo, setUserInfo] = useState(APS.userInfo)

    useEffect(
        () =>
            EventSystem.listen("APSUserInfoUpdate", () => {
                setUserInfo(APS.userInfo)
            }),
        []
    )

    if (!userInfo) {
        return <SynthesisIcons.QUESTION />
    } else {
        return <img src={userInfo.picture} className={`object-contain aspect-square ${className}`}></img>
    }
}

export default UserIcon
