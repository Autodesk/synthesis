import React, { ReactNode } from "react"

type ScrollViewProps = {
    children?: ReactNode
    className?: string
    maxHeight?: string
}

const ScrollView: React.FC<ScrollViewProps> = ({ className, maxHeight, children }) => {
    return (
        <div
            className={`bg-background-secondary p-4 rounded-md ${maxHeight ? maxHeight : "max-h-70vh"} w-full overflow-y-scroll ${className}`}
        >
            {children}
        </div>
    )
}

export default ScrollView
