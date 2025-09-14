import { Typography, type TypographyProps } from "@mui/material"
import type { Variant } from "@mui/material/styles/createTypography"
import type React from "react"
import type { PropsWithChildren } from "react"

type LabelSize = "sm" | "md" | "lg" | "xl"

interface LabelProps {
    size: LabelSize
}

const sizeToVariant: { [key in LabelSize]: Variant } = {
    sm: "body1",
    md: "h6",
    lg: "h4",
    xl: "h1",
}

const Label: React.FC<PropsWithChildren<LabelProps> & TypographyProps> = ({ children, size, ...props }) => {
    return (
        <Typography variant={sizeToVariant[size]} {...props}>
            {children}
        </Typography>
    )
}

export default Label
