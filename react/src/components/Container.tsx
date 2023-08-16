import React, { ReactNode } from "react"

type ContainerProps = {
    children?: ReactNode
    className?: string
}

const Container: React.FC<ContainerProps> = ({
    className,
    children,
}) => {
    return (
        <div
            className={className}
        >
            {children}
        </div>
    )
}

export default Container
