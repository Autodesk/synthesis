import revoluteIcon from "../../../src/Resources/JointIcons/JointRev/32x32.png"
import sliderIcon from "../../../src/Resources/JointIcons/JointSlider/32x32.png"
import { sendData } from "./index.ts"
import { type Joint, JointParentType, JointType, SignalType, WheelType } from "./types.ts"

export const jointInfo: Partial<
    Record<JointType, { icon: string; name: string; speedUnits: string; defaultSpeed: number }>
> = {
    [JointType.RevoluteJointType]: {
        icon: revoluteIcon,
        name: "Revolute",
        defaultSpeed: Math.PI,
        speedUnits: "rad/s",
    },
    [JointType.SliderJointType]: {
        icon: sliderIcon,
        name: "Slider",
        defaultSpeed: 100,
        speedUnits: "cm/s",
    },
}

export const signalInfo: Record<SignalType, { bg: string; outline: string; fg: string }> = {
    [SignalType.PWM]: { bg: "#ffa779", outline: "#ff894a", fg: "#ce5d21" },
    [SignalType.CAN]: { bg: "#79ff84", outline: "#77ff4a", fg: "#00bb19" },
    [SignalType.PASSIVE]: { bg: "#cbcbcb", outline: "#8d8d8d", fg: "#5d5d5d" },
}

export function createJoint(fusionJoint: FusionJoint): Joint {
    return {
        jointToken: fusionJoint.entityToken,
        name: fusionJoint.name,
        type: fusionJoint.jointType,
        parent: JointParentType.ROOT,
        signalType: SignalType.PWM,
        speed: jointInfo[fusionJoint.jointType]?.defaultSpeed ?? 0,
        force: 0.05,
        isWheel: false,
        wheelType: WheelType.STANDARD,
    }
}

export interface FusionJoint {
    entityToken: string
    name: string
    jointType: JointType
}

export async function selectJoint(): Promise<FusionJoint | undefined> {
    if (import.meta.env.DEV && typeof window.adsk === "undefined") {
        return new Promise<FusionJoint>(resolve => {
            setTimeout(() => {
                const jointType = Math.round(1 + Math.random())
                const token = Math.random().toString(36).substring(2, 15)
                resolve({
                    entityToken: token,
                    name: `${jointType === 1 ? "Revolute" : "Slider"} ${token.substring(0, 2).toUpperCase()}`,
                    jointType: jointType,
                })
            }, 2000)
        })
    }
    return await sendData("selectJoint", {})
}
