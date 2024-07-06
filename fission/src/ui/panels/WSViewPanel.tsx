import Panel, { PanelPropsImpl } from "@/components/Panel"
import { PWM_UPDATE_EVENT_KEY } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { Typography } from "@mui/material"
import { useCallback, useEffect } from "react"
import { GrConnect } from "react-icons/gr"
import { IoPeople } from "react-icons/io5"

const WSViewPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {

    const onPwmUpdate = useCallback((_: Event) => {

    }, [])

    useEffect(() => {
        window.addEventListener(PWM_UPDATE_EVENT_KEY, onPwmUpdate)

        return () => {
            window.removeEventListener(PWM_UPDATE_EVENT_KEY, onPwmUpdate)
        }
    }, [onPwmUpdate])

    return (
        <Panel
            name={"WS View Panel"}
            icon={<GrConnect />}
            panelId={panelId}
            openLocation="right"
            sidePadding={4}
        >
            <Typography
                sx={{
                    fontFamily: "Artifakt Legend",
                    fontWeight: 300
                }}
            >
                Sup
            </Typography>
        </Panel>
    )
}

export default WSViewPanel
