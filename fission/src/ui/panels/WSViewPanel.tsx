import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SIM_MAP_UPDATE_EVENT, simMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { styled, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { GrConnect } from "react-icons/gr"

const TypoStyled = styled(Typography)({
    fontFamily: "Artifakt Legend",
    fontWeight: 300,
    color: "white"
})

function generateTableBody() {
    return (
        <TableBody>
            {simMap.has('pwm') ? [...simMap.get('pwm')!.entries()].filter(x => x[1]["<init"] == true).map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>PWM</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
            {simMap.has('simdevice') ? [...simMap.get('simdevice')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>SimDevice</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }): <></>}
            {simMap.has('canmotor') ? [...simMap.get('canmotor')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>CAN Motor</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
            {simMap.has('canencoder') ? [...simMap.get('canencoder')!.entries()].map(x => {
                return (
                    <TableRow key={x[0]}>
                        <TableCell><TypoStyled>CAN Encoder</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{x[0]}</TypoStyled></TableCell>
                        <TableCell><TypoStyled>{JSON.stringify(x[1])}</TypoStyled></TableCell>
                    </TableRow>
                )
            }) : <></>}
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
