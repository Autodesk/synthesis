import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import React, { useMemo, useState } from "react"
import ConfigureGamepiecePickupInterface from "./ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./ConfigureShotTrajectoryInterface"
import { ConfigurationSavedEvent } from "../ConfigurePanel"
import World from "@/systems/World"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import Driver from "@/systems/simulation/driver/Driver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import SubsystemRowInterface from "./SubsystemRowInterface"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import { DefaultSequentialConfig, SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"

enum ConfigMode {
    NONE,
    INTAKE,
    EJECTOR,
}

class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode
    driver?: Driver
    sequential?: SequentialBehaviorPreferences

    constructor(name: string, configMode: ConfigMode, driver?: Driver, sequential?: SequentialBehaviorPreferences) {
        super(name)
        this.configMode = configMode
        this.driver = driver
        this.sequential = sequential
    }
}

interface ConfigSubsystemProps {
    selectedRobot: MirabufSceneObject
}

interface ConfigInterfaceProps {
    configModeOption: ConfigModeSelectionOption
    selectedRobot: MirabufSceneObject
    saveBehaviors: () => void
}

const ConfigInterface: React.FC<ConfigInterfaceProps> = ({ configModeOption, selectedRobot, saveBehaviors }) => {
    switch (configModeOption.configMode) {
        case ConfigMode.INTAKE:
            return <ConfigureGamepiecePickupInterface selectedRobot={selectedRobot} />
        case ConfigMode.EJECTOR:
            return <ConfigureShotTrajectoryInterface selectedRobot={selectedRobot} />
        case ConfigMode.NONE:
            return (
                <SubsystemRowInterface
                    driver={configModeOption.driver!}
                    robot={selectedRobot}
                    sequentialBehavior={configModeOption.sequential}
                    saveBehaviors={saveBehaviors}
                />
            )
        default:
            throw new Error(`Config mode ${configModeOption.configMode} has no associated interface`)
    }
}

const ConfigureSubsystemsInterface: React.FC<ConfigSubsystemProps> = ({ selectedRobot }) => {
    const [selectedConfigMode, setSelectedConfigMode] = useState<ConfigModeSelectionOption | undefined>(undefined)

    const behaviors = useMemo<SequentialBehaviorPreferences[]>(
        () =>
            PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName)?.sequentialConfig ??
            (selectedRobot.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => DefaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator")),
        []
    )

    const drivers = useMemo(() => {
        return World.SimulationSystem.GetSimulationLayer(selectedRobot.mechanism)?.drivers
    }, [selectedRobot])

    const getConfigModes = () => {
        const modes = [
            new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE),
            new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR),
        ]

        if (drivers == undefined) return modes

        modes.push(
            new ConfigModeSelectionOption(
                "Drivetrain",
                ConfigMode.NONE,
                drivers.filter(x => x instanceof WheelDriver)[0]
            )
        )

        let jointIndex = 0

        drivers
            .filter(x => x instanceof HingeDriver)
            .forEach(d => {
                modes.push(
                    new ConfigModeSelectionOption(
                        d.info?.name ?? "UnnamedMotor",
                        ConfigMode.NONE,
                        d,
                        behaviors[jointIndex]
                    )
                )
                jointIndex++
            })

        drivers
            .filter(x => x instanceof SliderDriver)
            .forEach(d => {
                modes.push(
                    new ConfigModeSelectionOption(
                        d.info?.name ?? "UnnamedMotor",
                        ConfigMode.NONE,
                        d,
                        behaviors[jointIndex]
                    )
                )
                jointIndex++
            })

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
                <ConfigInterface
                    configModeOption={selectedConfigMode}
                    selectedRobot={selectedRobot}
                    saveBehaviors={() => {
                        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName).sequentialConfig = behaviors
                        PreferencesSystem.savePreferences()
                        console.log("behaviors saved!")
                    }}
                />
            )}
        </>
    )
}

export default ConfigureSubsystemsInterface
