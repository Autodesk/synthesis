import type React from "react"
import { useCallback } from "react"
import { Button, Spacer } from "@/components/StyledComponents.tsx"
import { FaFileDownload } from "react-icons/fa"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { mirabuf } from "@/proto/mirabuf"

interface ConfigModeSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const AssemblyExportButton: React.FC<ConfigModeSelectionProps> = ({ selectedAssembly }) => {
    const exportHandler = useCallback(() => {
        selectedAssembly.savePreferencesToMirabuf()
        const assembly = selectedAssembly.mirabufInstance.parser.assembly

        try {
            const encoded = mirabuf.Assembly.encode(assembly).finish()
            const blob = new Blob([encoded.buffer as ArrayBuffer], {
                type: "application/octet-stream",
            })
            const url = URL.createObjectURL(blob)

            const filename = `${assembly.info?.name ?? "unknown"}.mira`

            const a = document.createElement("a")
            a.href = url
            a.download = filename
            document.body.appendChild(a)
            a.click()
            setTimeout(() => {
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
            }, 0)
            globalAddToast?.("info", "Exported", `Exported ${filename}`)
        } catch (_e) {
            globalAddToast?.("error", "Export Error", "Failed to export.")
        }
    }, [selectedAssembly])
    return (
        <Button className={"w-full"} color={"secondary"} onClick={exportHandler}>
            Export
            {Spacer(0, 5)}
            <FaFileDownload />
        </Button>
    )
}

export default AssemblyExportButton
