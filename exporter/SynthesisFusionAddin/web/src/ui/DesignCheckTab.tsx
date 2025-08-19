import { Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from "@mui/material"
import { useEffect, useState } from "react"
import { type DesignRule, sendData } from "../lib"

function DesignCheckTab() {
    const [isWindowLoaded, setIsWindowLoaded] = useState<boolean>()
    const [rules, setRules] = useState<DesignRule[]>([])

    useEffect(() => {
        if (!isWindowLoaded) return

        const fetchRules = async () => {
            const data = await sendData("designRules", {})
            if (data) {
                setRules(data)
                console.log(data[0].calculation)
            }
        }

        fetchRules()
    }, [isWindowLoaded])

    // biome-ignore lint/correctness/useExhaustiveDependencies: onWindowLoad is stable
    useEffect(() => {
        if (typeof window.adsk === "undefined") {
            requestAnimationFrame(onWindowLoad)
            return
        }
    }, [])

    function onWindowLoad(): void {
        setIsWindowLoaded(true)
    }

    function isDesignValid(): string {
        rules.forEach(rule => {
            if (rule.calculation > rule.max_value) {
                return "Invalid"
            }
        })

        return "Valid"
    }

    return (
        <>
            <h4>Checks Passing: {isDesignValid()}</h4>
            <TableContainer component={Paper} elevation={6}>
                <Table sx={{ minWidth: 650 }} aria-label="simple table" size={"small"}>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ width: "28%" }} align="center">
                                Component
                            </TableCell>
                            <TableCell sx={{ width: "28%" }} align="center">
                                Calculation
                            </TableCell>
                            <TableCell sx={{ width: "40%" }} align="center">
                                Is Valid
                            </TableCell>
                            <TableCell sx={{ width: "4%" }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rules.map(rule => (
                            <TableRow key={rule.name} sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                                <TableCell align="center">{rule.name}</TableCell>
                                <TableCell align="center">{Math.round(rule.calculation * 100) / 100} cm</TableCell>
                                <TableCell align="center">
                                    {rule.calculation <= rule.max_value ? "Valid" : "Invalid"}
                                </TableCell>
                                <TableCell />
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </>
    )
}

export default DesignCheckTab
