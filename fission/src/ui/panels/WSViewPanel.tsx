import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SIM_MAP_UPDATE_EVENT, canMotorMap, pwmMap, simDeviceMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { styled, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { GrConnect } from "react-icons/gr"
import { IoPeople } from "react-icons/io5"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
    color: "white"
})

function generateTableBody() {
    return (
        <TableBody>
            {[...pwmMap.entries()].filter(x => x[1]["<init"] == true).map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>PWM</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            })}
            {[...simDeviceMap.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>SimDevice</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            })}
            {[...canMotorMap.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>CAN Motor</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            })}
        </TableBody>
    )
}

const WSViewPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {

    const [tb, setTb] = useState(generateTableBody())

    const onSimMapUpdate = useCallback((_: Event) => {
        setTb(generateTableBody())
    }, [])

    useEffect(() => {
        window.addEventListener(SIM_MAP_UPDATE_EVENT, onSimMapUpdate)

        return () => {
            window.removeEventListener(SIM_MAP_UPDATE_EVENT, onSimMapUpdate)
        }
    }, [onSimMapUpdate])

    return (
        <Panel
            name={"WS View Panel"}
            icon={<GrConnect />}
            panelId={panelId}
            openLocation="right"
            sidePadding={4}
        >
            <TableContainer
                sx={{
                    maxWidth: '80vw',
                    maxHeight: '80vh',
                }}
            >
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell><TypoStyled>Type</TypoStyled></TableCell>
                            <TableCell><TypoStyled>Device</TypoStyled></TableCell>
                            <TableCell><TypoStyled>Data</TypoStyled></TableCell>
                        </TableRow>
                    </TableHead>
                    {tb}
                </Table>
            </TableContainer>
        </Panel>
    )
}

export default WSViewPanel
