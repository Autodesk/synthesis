import React, { ReactNode } from "react"

type ScrollViewProps = {
    children?: ReactNode
    className?: string
}

const ScrollView: React.FC<ScrollViewProps> = ({ className, children }) => {
    return (
        <div className={`bg-background-secondary p-4 rounded-md max-h-full w-full ${className} overflow-y-scroll`}>{children}</div>
    )
}

export default ScrollView
