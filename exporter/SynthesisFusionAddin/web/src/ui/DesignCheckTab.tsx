import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
} from "@mui/material"

interface DesignCheckTabProps { }

function DesignCheckTab({}: DesignCheckTabProps) {
    return (
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

                </TableBody>
            </Table>
        </TableContainer>
    )
}

export default DesignCheckTab
