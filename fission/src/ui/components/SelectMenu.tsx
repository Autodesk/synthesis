import { Divider, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import Label from "./Label"
import { Button, CustomTooltip, IconButton, Spacer, SynthesisIcons } from "./StyledComponents"

/** Extend this to make a type that contains custom data */
export class SelectMenuOption {
    id: string
    name: string
    disabled: boolean
    tooltipText?: string

    constructor(id: string, name: string, tooltipText?: string, disabled: boolean = false) {
        this.id = id
        this.name = name
        this.disabled = disabled
        this.tooltipText = tooltipText
    }
}

interface OptionCardProps<OptionType extends SelectMenuOption> {
    value: OptionType
    index: number
    onSelected: (val: OptionType) => void
    onDelete?: () => void
    includeDelete: boolean
}

/**
 * An option for the select menu that contains a specific index, name, and can store a custom value through a SelectMenuOption instance.
 *
 * @param {OptionCardProps} props - The properties object.
 * @param {number} props.index - The index of this option in the menu that's used to generate a unique key.
 * @param {function} props.onSelected - Callback function to handle selection of this option.
 * @param {function} [props.onDelete] - Callback function to handle deletion of this option.
 * @param {boolean} props.includeDelete - A boolean to determine if this specific option is able to be deleted.
 *
 * @returns {JSX.Element} The rendered OptionCard component.
 */
const OptionCard = <OptionType extends SelectMenuOption>({
    value,
    index,
    onSelected,
    onDelete,
    includeDelete,
}: OptionCardProps<OptionType>): React.ReactElement => {
    return (
        <Stack
            direction="row"
            textAlign="center"
            key={value.name}
            minHeight="30px"
            overflow="hidden"
            position="relative"
        >
            {/* Box containing the label */}
            <Button
                fullWidth={true}
                color="secondary"
                variant="outlined"
                disabled={value.disabled}
                onClick={() => {
                    onSelected(value)
                }}
                className={value.name}
                sx={{
                    borderColor: "#888888",
                    textTransform: "none",
                    justifyContent: "flex-start",
                }}
                id={`select-button-${value.name}`}
            >
                <Label key={value.name + index} size="sm" className="text-left mt-[4pt] mb-[2pt] mx-[5%]">
                    {value.name}
                </Label>
            </Button>

            {/* Button used for selecting a parent (shows up as an outline) */}
            {value.tooltipText && <CustomTooltip text={value.tooltipText} />}
            {/** Delete button only if onDelete is defined */}
            {onDelete && includeDelete && !value.disabled && (
                <>
                    <Spacer width={10} />
                    {/*DeleteButton(onDelete !== undefined ? onDelete : () => {}, "select-menu-delete-button")&*/}
                    <Button color="error" onClick={() => onDelete?.()} id="select-menu-delete-button">
                        <SynthesisIcons.DELETE_LARGE />
                    </Button>
                </>
            )}
        </Stack>
    )
}

interface SelectMenuProps<OptionType extends SelectMenuOption> {
    options: OptionType[]
    onOptionSelected: (val: OptionType | undefined) => void

    // Function to return a default value
    defaultSelectedOption?: OptionType | undefined
    defaultHeaderText: string
    noOptionsText?: string
    // TODO: indentation?: number
    onDelete?: (val: OptionType) => void | undefined

    // If false, this menu option will not have a delete button
    deleteCondition?: (val: OptionType) => boolean
    onAddClicked?: () => void
}

/**
 * A menu with multiple options. When an option is selected, it is displayed as the header and a back button appears to select a different item.
 *
 * @param {SelectMenuProps} props - The properties object.
 * @param {SelectMenuOption[]} props.options - The available options in this menu.
 * @param {function} props.onOptionSelected - Callback function to handle an option being selected. Called with undefined when no option is selected.
 * @param {string} props.defaultHeaderText - The text displayed in the header if no option is selected.
 * @param {string} [props.noOptionsText] - The text displayed if there are no available options.
 * @param {function} [props.onDelete] - Callback function to handle the deletion of an option. If undefined, delete buttons will not be included.
 * @param {function} [props.deleteCondition] - A function take in a specific option and return true if it's deletable.
 * @param {function} [props.onAddClicked] - Callback function to handle the addition of an option. If undefined, no add button will be included.
 *
 * @returns {JSX.Element} The rendered SelectMenu component.
 */
const SelectMenu = <OptionType extends SelectMenuOption>({
    options,
    onOptionSelected,
    defaultSelectedOption,
    defaultHeaderText,
    noOptionsText,
    onDelete,
    deleteCondition,
    onAddClicked,
}: SelectMenuProps<OptionType>): React.ReactElement => {
    const [selectedOption, setSelectedOption] = useState<SelectMenuOption | undefined>(defaultSelectedOption)

    // I have no idea why, but this would actually update state to default selection.
    useEffect(() => {
        setSelectedOption(defaultSelectedOption)
    }, [defaultSelectedOption])

    // If the selected option no longer exists as an option, deselect it
    useEffect(() => {
        if (selectedOption && !options.some(o => o.id === selectedOption.id)) {
            setSelectedOption(undefined)
            onOptionSelected(undefined)
        }
    }, [options, onOptionSelected, selectedOption])

    return (
        <>
            {/** Box containing the menu header */}
            <SelectMenuHeader
                showBackButton={selectedOption !== undefined}
                onBackButton={() => {
                    setSelectedOption(undefined)
                    onOptionSelected(undefined)
                }}
                label={selectedOption !== undefined ? selectedOption.name : defaultHeaderText}
            />
            <Divider />
            <Spacer height={12} />

            {selectedOption === undefined && (
                <>
                    {/** List of options */}
                    <Stack gap={2}>
                        {options.length <= 0 && <Label size="sm">{noOptionsText ?? "No options available!"}</Label>}
                        {options.map((option, i) => {
                            return (
                                <OptionCard
                                    value={option}
                                    index={i}
                                    onSelected={val => {
                                        setSelectedOption(val)
                                        onOptionSelected(val)
                                    }}
                                    key={option.name + i}
                                    onDelete={onDelete ? () => onDelete(option) : undefined}
                                    includeDelete={deleteCondition === undefined || deleteCondition(option)}
                                />
                            )
                        })}
                        {/** Add button */}
                        {onAddClicked && (
                            <Button
                                variant="outlined"
                                color="success"
                                onClick={onAddClicked}
                                id="select-menu-add-button"
                            >
                                <SynthesisIcons.ADD_LARGE />
                            </Button>
                        )}
                    </Stack>
                </>
            )}
        </>
    )
}

interface SelectMenuHeaderProps {
    showBackButton?: boolean
    onBackButton: () => void
    label: string
}

export const SelectMenuHeader = ({
    showBackButton,
    onBackButton,
    label,
}: SelectMenuHeaderProps): React.ReactElement => {
    return (
        <Stack direction="row" textAlign="center" minHeight="30px" key="selected-item" gap={1}>
            {/** Back arrow button when an option is selected */}
            {showBackButton && (
                <IconButton onClick={onBackButton} id="select-menu-back-button" sx={{ mr: 1 }}>
                    <SynthesisIcons.LEFT_ARROW_LARGE />
                </IconButton>
            )}

            {/** Label with either the header text, or the name of the selected option if an option is selected */}
            <Stack alignSelf="center">
                <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {label}
                </Label>
            </Stack>
        </Stack>
    )
}

export default SelectMenu
