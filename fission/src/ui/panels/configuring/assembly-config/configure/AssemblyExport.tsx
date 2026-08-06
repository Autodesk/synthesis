import type React from "react"
import { useCallback } from "react"
import { Button, Spacer } from "@/components/StyledComponents.tsx"
import { FaFileDownload } from "react-icons/fa"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { zeroGamePieceAssemblyPosition } from "@/mirabuf/MirabufParser"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { mirabuf } from "@/proto/mirabuf"
import { downloadBlob } from "@/util/Utility.ts"

interface ConfigModeSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const AssemblyExportButton: React.FC<ConfigModeSelectionProps> = ({ selectedAssembly }) => {
    const exportHandler = useCallback(() => {
        selectedAssembly.savePreferencesToMirabuf()
        const assembly = selectedAssembly.mirabufInstance.parser.assembly

        if (selectedAssembly.miraType === MiraType.PIECE) {
            zeroGamePieceAssemblyPosition(assembly)
        }

        const filename = `${assembly.info?.name ?? "unknown"}.mira`
        try {
            const encoded = mirabuf.Assembly.encode(assembly).finish()

            downloadBlob(filename, encoded.buffer as ArrayBuffer)
            globalAddToast?.("info", "Exported", `Exported ${filename}`)
        } catch (_e) {
            globalAddToast?.("error", "Export Error", "Failed to export.")
        }
    }, [selectedAssembly])
    return (
        <Button className={"w-full"} color={"secondary"} onClick={exportHandler}>
            Export
            <Spacer width={5} />
            <FaFileDownload />
        </Button>
    )
}

export default AssemblyExportButton
