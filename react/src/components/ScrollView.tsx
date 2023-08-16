import React, { ReactNode } from "react"

type ScrollViewProps = {
    children?: ReactNode
    className?: string
}

const ScrollView: React.FC<ScrollViewProps> = ({ className, children }) => {
    return (
        <div className={`w-max ${className} overflow-y-scroll`}>{children}</div>
    )
}

export default ScrollView
