import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import React, { useMemo, useState } from "react"
import ConfigureGamepiecePickupInterface from "./ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./ConfigureShotTrajectoryInterface"
import SequentialBehaviorsInterface from "./SequentialBehaviorsInterface"
import { ConfigurationSavedEvent } from "../ConfigurePanel"
import World from "@/systems/World"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import Driver from "@/systems/simulation/driver/Driver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import { SubsystemRow } from "./ConfigureSubsystemsInterfaceOLD"

enum ConfigMode {
    NONE,
    INTAKE,
    EJECTOR,
    SUBSYSTEMS,
    SEQUENTIAL,
}

class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode
    driver?: Driver

    constructor(name: string, configMode: ConfigMode, driver?: Driver) {
        super(name)
        this.configMode = configMode
        this.driver = driver
    }
}

interface ConfigSubsystemProps {
    selectedRobot: MirabufSceneObject
}

interface ConfigInterfaceProps {
    configModeOption: ConfigModeSelectionOption
    selectedRobot: MirabufSceneObject
}

const ConfigInterface: React.FC<ConfigInterfaceProps> = ({ configModeOption, selectedRobot }) => {
    switch (configModeOption.configMode) {
        case ConfigMode.INTAKE:
            return <ConfigureGamepiecePickupInterface selectedRobot={selectedRobot} />
        case ConfigMode.EJECTOR:
            return <ConfigureShotTrajectoryInterface selectedRobot={selectedRobot} />
        case ConfigMode.SEQUENTIAL:
            return <SequentialBehaviorsInterface selectedRobot={selectedRobot} />
        case ConfigMode.SUBSYSTEMS:
            return <ConfigureSubsystemsInterface selectedRobot={selectedRobot} />
        case ConfigMode.NONE:
            return <SubsystemRow driver={configModeOption.driver!} robot={selectedRobot} />
        default:
            throw new Error(`Config mode ${configModeOption.configMode} has no associated interface`)
    }
}

const ConfigureSubsystemsInterface: React.FC<ConfigSubsystemProps> = ({ selectedRobot }) => {
    const [selectedConfigMode, setSelectedConfigMode] = useState<ConfigModeSelectionOption | undefined>(undefined)

    const drivers = useMemo(() => {
        return World.SimulationSystem.GetSimulationLayer(selectedRobot.mechanism)?.drivers
    }, [selectedRobot])

    const getConfigModes = () => {
        const modes = [
            new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE),
            new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR),
            //new ConfigModeSelectionOption("Subsystems", ConfigMode.SUBSYSTEMS),
        ]

        if (drivers == undefined) return modes

        modes.push(
            new ConfigModeSelectionOption(
                "Drivetrain",
                ConfigMode.NONE,
                drivers.filter(x => x instanceof WheelDriver)[0]
            )
        )

        drivers
            .filter(x => x instanceof SliderDriver || x instanceof HingeDriver)
            .forEach(d => {
                modes.push(new ConfigModeSelectionOption(d.info?.name ?? "UnnamedMotor", ConfigMode.NONE, d))
            })

        modes.push(new ConfigModeSelectionOption("Sequence Joints", ConfigMode.SEQUENTIAL))

        return modes
    }

    return (
        <>
            <SelectMenu
                options={getConfigModes()}
                onOptionSelected={val => {
                    if (val != undefined) new ConfigurationSavedEvent()
                    setSelectedConfigMode(val as ConfigModeSelectionOption)
                }}
                defaultHeaderText="Select a Configuration Mode"
                indentation={2}
            />
            {selectedConfigMode != undefined && (
                <ConfigInterface configModeOption={selectedConfigMode} selectedRobot={selectedRobot} />
            )}
        </>
    )
}

export default ConfigureSubsystemsInterface
