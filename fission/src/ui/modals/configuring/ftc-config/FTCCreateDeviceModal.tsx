import { TextField } from "@mui/material"
import type React from "react"
import { useEffect, useMemo, useState } from "react"
import type Driver from "@/systems/simulation/driver/Driver"
import FTCBrain from "@/systems/simulation/ftc_brain/FTCBrain"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import ScrollView from "@/ui/components/ScrollView"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

/**
 * Lists every Driver on the robot with a free-text hardwareMap name field next
 * to it, so names can be set to match whatever hardwareMap.get(..., "name")
 * calls the team's OpMode source uses. A single hardwareMap name can still
 * back multiple Drivers (e.g. two wheels ganged on one drivetrain side) --
 * just give them the same name. Wiring here is session-only, not persisted
 * through assembly.simConfigData -- see FTCBrain's doc comment.
 */
const FTCCreateDeviceModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen } = useUIContext()

    const { drivers, brain } = useMemo(() => {
        const miraObj = World.sceneRenderer.mirabufSceneObjects.getRobots()[0]
        if (miraObj == null) return { drivers: [] as Driver[], brain: undefined as FTCBrain | undefined }

        const simLayer = World.simulationSystem.getSimulationLayer(miraObj.mechanism)
        return {
            drivers: simLayer?.drivers ?? [],
            brain: simLayer?.brain instanceof FTCBrain ? simLayer.brain : undefined,
        }
    }, [])

    const [names, setNames] = useState<string[]>(() =>
        drivers.map(driver => {
            if (!brain) return ""
            for (const [deviceName, wired] of brain.motorWiring) {
                if (wired.includes(driver)) return deviceName
            }
            return ""
        })
    )

    useEffect(() => {
        const onBeforeAccept = () => {
            if (!brain) return
            Array.from(brain.motorWiring.keys()).forEach(deviceName => brain!.removeMotorWiring(deviceName))
            drivers.forEach((driver, idx) => {
                const deviceName = names[idx]?.trim()
                if (deviceName) brain!.addMotorWiring(deviceName, driver)
            })
        }

        configureScreen(modal!, { title: "Configure FTC Devices", acceptText: "Done" }, { onBeforeAccept })
    }, [brain, drivers, names, configureScreen, modal])

    return (
        <ScrollView>
            {drivers.map((driver, idx) => (
                <div className="flex items-center gap-2" key={`${driver.constructor.name}-${idx}`}>
                    <Label size="sm" className="flex-1">
                        {`${driver.constructor.name}${driver.info?.name ? ` (${driver.info.name})` : ""}`}
                    </Label>
                    <TextField
                        placeholder="e.g. leftFront"
                        value={names[idx] ?? ""}
                        onChange={e => {
                            const next = [...names]
                            next[idx] = e.target.value
                            setNames(next)
                        }}
                    />
                </div>
            ))}
        </ScrollView>
    )
}

export default FTCCreateDeviceModal
