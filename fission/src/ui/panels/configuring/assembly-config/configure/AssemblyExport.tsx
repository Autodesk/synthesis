import type React from "react"
import { useCallback } from "react"
import { Button, Spacer } from "@/components/StyledComponents.tsx"
import { FaFileDownload } from "react-icons/fa"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"

interface ConfigModeSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const AssemblyExportButton: React.FC<ConfigModeSelectionProps> = ({ selectedAssembly }) => {
    const exportHandler = useCallback(() => {
        console.log(selectedAssembly.assemblyHash)
    }, [selectedAssembly.assemblyHash])
    return (
        <Button className={"w-full"} color={"secondary"} onClick={exportHandler}>
            Export
            {Spacer(0, 5)}
            <FaFileDownload />
        </Button>
    )
}

export default AssemblyExportButton
