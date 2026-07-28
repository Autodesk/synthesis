import EventSystem from "@/systems/EventSystem.ts"
import type { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
import { random } from "@/util/Random"
import PreferencesSystem from "../preferences/PreferencesSystem"
import DefaultInputs from "./DefaultInputs"
import InputSystem from "./InputSystem"
import { type InputScheme, type InputSchemeAvailability, InputSchemeUseType, type KeyDescriptor } from "./InputTypes"
import AxisInput from "./inputs/AxisInput"
import ButtonInput from "./inputs/ButtonInput"
import type Input from "./inputs/Input"

class InputSchemeManager {
    // References to the current custom schemes to avoid parsing every time they are requested
    private static _customSchemes: InputScheme[] | undefined

    /** Fetches custom input schemes from preferences manager */
    public static get customInputSchemes(): InputScheme[] {
        if (this._customSchemes) return this._customSchemes

        // Load schemes from preferences and parse into objects
        this._customSchemes = PreferencesSystem.getUserPreference("InputSchemes")
        this._customSchemes.forEach(scheme => this.parseScheme(scheme))

        return this._customSchemes
    }

    /** Registers a new custom scheme */
    public static addCustomScheme(scheme: InputScheme, panelId?: string) {
        this.customInputSchemes.push(scheme)
        EventSystem.dispatch("InputSchemeChanged", { panelId })
    }

    /** Parses a schemes inputs into working Input instances */
    private static parseScheme(rawInputs: InputScheme) {
        for (let i = 0; i < rawInputs.inputs.length; i++) {
            const rawInput = rawInputs.inputs[i]
            let parsedInput: Input

            if ((rawInput as ButtonInput).keyCode != undefined) {
                const rawButton = rawInput as ButtonInput

                parsedInput = new ButtonInput(
                    rawButton.inputName,
                    rawButton.keyCode,
                    rawButton.gamepadButton,
                    rawButton.keyModifiers
                )
            } else {
                const rawAxis = rawInput as AxisInput

                parsedInput = new AxisInput(
                    rawAxis.inputName,
                    rawAxis.posKeyCode,
                    rawAxis.negKeyCode,
                    rawAxis.gamepadAxisNumber,
                    rawAxis.joystickInverted,
                    rawAxis.useGamepadButtons,
                    rawAxis.posGamepadButton,
                    rawAxis.negGamepadButton,
                    rawAxis.touchControlAxis,
                    rawAxis.posKeyModifiers,
                    rawAxis.negKeyModifiers
                )
            }

            rawInputs.inputs[i] = parsedInput
        }
    }

    private static _defaultInputSchemes: InputScheme[] | undefined

    public static get defaultInputSchemes(): InputScheme[] {
        if (!this._defaultInputSchemes) {
            this._defaultInputSchemes = DefaultInputs.defaultInputCopies
        }
        return this._defaultInputSchemes
    }

    public static resetDefaultSchemes(panelId?: string) {
        this._defaultInputSchemes = DefaultInputs.defaultInputCopies
        this._customSchemes = undefined
        EventSystem.dispatch("InputSchemeChanged", { panelId })
    }

    public static rebindOldBrainSchemes() {
        const schemesByName = new Map(this.allInputSchemes.map(s => [s.schemeName, s] as const))
        for (const [brainIndex, scheme] of InputSystem.brainIndexSchemeMap) {
            const reverted = schemesByName.get(scheme.schemeName)
            if (reverted && scheme.customized) {
                InputSystem.brainIndexSchemeMap.set(brainIndex, reverted)
            }
        }
    }

    /** Creates an array of every input scheme that is either a default or customized by the user. Custom themes will appear on top. */
    public static get allInputSchemes(): InputScheme[] {
        // Start with custom input schemes
        const allSchemes: InputScheme[] = []

        this.customInputSchemes.forEach(s => allSchemes.push(s))

        // Add default schemes if they have not been customized
        this.defaultInputSchemes.forEach(defaultScheme => {
            if (allSchemes.some(s => s.schemeName === defaultScheme.schemeName)) return
            allSchemes.push(defaultScheme)
        })

        return allSchemes
    }

    /**
     * Computes the availability of every scheme.
     *
     * Gamepad schemes are shareable: one only counts as in-use for a candidate assignment when the
     * *same layout* is already running on the *same controller slot* the candidate robot would use.
     * Keyboard/touch schemes remain fully in-use once bound to any robot, and still conflict on shared keys.
     *
     * @param candidateBrainIndex - The brain being configured, excluded from its own in-use calculation.
     * @param candidateSlot - The controller slot the candidate robot would use; drives the gamepad in-use rule.
     */
    private static computeAvailableSchemes(
        candidateBrainIndex?: number,
        candidateSlot?: number
    ): InputSchemeAvailability[] {
        const allSchemes = this.allInputSchemes

        const usedKeyMap = new Map<KeyDescriptor, string[]>()
        // Controller slots each gamepad layout is already assigned to on other robots.
        const gamepadSlotsByScheme = new Map<string, Set<number>>()
        const result: Record<string, InputSchemeAvailability> = {}

        for (const [brainIndex, scheme] of InputSystem.brainIndexSchemeMap) {
            if (scheme.usesGamepad) {
                // A robot only occupies a gamepad layout on the specific controller slot it's assigned to.
                if (brainIndex === candidateBrainIndex) continue
                const slots = gamepadSlotsByScheme.get(scheme.schemeName) ?? new Set<number>()
                slots.add(InputSystem.getPlayerSlot(brainIndex))
                gamepadSlotsByScheme.set(scheme.schemeName, slots)
                continue
            }

            result[scheme.schemeName] = {
                scheme,
                status: InputSchemeUseType.IN_USE,
            }
            scheme?.inputs?.forEach(input => {
                input
                    .keysUsed()
                    .filter(key => key != null)
                    .forEach(key => {
                        const entry = usedKeyMap.get(key)
                        if (entry != null) {
                            entry.push(scheme.schemeName)
                        } else {
                            usedKeyMap.set(key, [scheme.schemeName])
                        }
                    })
            })
        }

        allSchemes.forEach(scheme => {
            if (scheme.usesGamepad) {
                // In-use only if the controller slot we'd assign is already running this same layout.
                const slotTaken = candidateSlot != null && (gamepadSlotsByScheme.get(scheme.schemeName)?.has(candidateSlot) ?? false)
                result[scheme.schemeName] ??= {
                    scheme,
                    status: slotTaken ? InputSchemeUseType.IN_USE : InputSchemeUseType.AVAILABLE,
                }
                return
            }

            const conflictingSchemes = scheme.inputs.flatMap(input =>
                input.keysUsed().flatMap(key => usedKeyMap.get(key) ?? [])
            )
            if (conflictingSchemes.length > 0) {
                result[scheme.schemeName] ??= {
                    scheme,
                    status: InputSchemeUseType.CONFLICT,
                    conflictingSchemeNames: [...new Set(conflictingSchemes)].join(", "),
                }
            } else {
                result[scheme.schemeName] ??= {
                    scheme,
                    status: InputSchemeUseType.AVAILABLE,
                }
            }
        })
        return Object.values(result)
    }

    /** Creates an array of every input scheme, annotated with availability for the given controller slot. */
    public static availableInputSchemesByType(
        driveType?: DriveType,
        candidateBrainIndex?: number,
        candidateSlot?: number
    ): InputSchemeAvailability[] {
        const allSchemes = this.computeAvailableSchemes(candidateBrainIndex, candidateSlot)
        if (driveType == null) {
            return allSchemes
        }
        return allSchemes.filter(entry => entry.scheme.supportedDrivetrains.includes(driveType))
    }

    /** Creates an array of every input scheme, annotated with availability for the given brain's controller slot. */
    public static availableInputSchemesByBrain(brainIndex: number): InputSchemeAvailability[] {
        const driveType = SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType
        return this.availableInputSchemesByType(driveType, brainIndex, InputSystem.getPlayerSlot(brainIndex))
    }

    /**
     * Ensures the brain has an input scheme compatible with its current drivetrain.
     *
     * @returns the scheme now bound to the brain, or undefined if no compatible scheme is available.
     */
    public static applyCompatibleScheme(brainIndex: number): InputScheme | undefined {
        const driveType = SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType
        const current = InputSystem.brainIndexSchemeMap.get(brainIndex)
        if (current && (driveType == null || current.supportedDrivetrains.includes(driveType))) {
            return current
        }

        // Unbind the outgoing scheme before evaluating availability. Otherwise it still counts as in-use.
        InputSystem.brainIndexSchemeMap.delete(brainIndex)

        const next = this.availableInputSchemesByBrain(brainIndex).find(
            entry => entry.status === InputSchemeUseType.AVAILABLE
        )?.scheme
        if (next) InputSystem.setBrainIndexSchemeMapping(brainIndex, next)
        return next
    }

    /** @returns a random available robot name */
    public static get randomAvailableName(): string {
        const usedNames = this.allInputSchemes.map(s => s.schemeName)

        const randomName = () => {
            const index = Math.floor(random() * DefaultInputs.NAMES.length)
            return DefaultInputs.NAMES[index]
        }

        let name = randomName()
        while (usedNames.includes(name)) name = randomName()

        return name
    }

    /** Save all schemes that have been customized to local storage via preferences */
    public static saveSchemes(panelId?: string) {
        const customizedSchemes = this.allInputSchemes.filter(s => {
            return s.customized
        })

        PreferencesSystem.setUserPreference("InputSchemes", customizedSchemes)
        PreferencesSystem.savePreferences()
        EventSystem.dispatch("InputSchemeChanged", { panelId })
    }
}

export default InputSchemeManager
