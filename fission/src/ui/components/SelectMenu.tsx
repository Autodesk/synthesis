import React, { useEffect, useState } from "react"

import { Box, Button as MUIButton, styled, alpha } from "@mui/material"
import Label, { LabelSize } from "./Label"
import {
    AddButtonInteractiveColor,
    ButtonIcon,
    CustomTooltip,
    DeleteButton,
    SectionDivider,
    SectionLabel,
    Spacer,
    SynthesisIcons,
} from "./StyledComponents"

// Select menu item button (appears as an outline when hovered over, the text is a separate component)
const CustomButton = styled(MUIButton)({
    "borderStyle": "none",
    "borderWidth": "1px",
    "transition": "border-color 0s",
    "outline": "none",
    "&:hover": {
        borderStyle: "solid",
        borderColor: "grey",
        backgroundColor: "transparent",
    },
    "position": "relative",
    "overflow": "hidden",
    "& .MuiTouchRipple-root span": {
        backgroundColor: alpha("#ffffff", 0.07),
        animationDuration: "300ms",
    },
    "&:focus": {
        borderColor: "grey",
        backgroundColor: "transparent",
        outline: "none",
    },
    "&:selected": {
        outline: "none",
        backgroundColor: "transparent",
        borderColor: "none",
    },
})

/** Extend this to make a type that contains custom data */
export class SelectMenuOption {
    name: string
    tooltipText?: string
    constructor(name: string, tooltipText?: string) {
        this.name = name
        this.tooltipText = tooltipText
    }
}

interface OptionCardProps {
    value: SelectMenuOption
    index: number
    onSelected: (val: SelectMenuOption) => void
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
const OptionCard: React.FC<OptionCardProps> = ({ value, index, onSelected, onDelete, includeDelete }) => {
    return (
        <Box
            display="flex"
            textAlign={"center"}
            key={value.name}
            minHeight={"30px"}
            overflow="hidden"
            position={"relative"}
        >
            {/* Box containing the label */}
            <Box position="absolute" alignSelf={"center"} display="flex">
                {/* Indentation before the name */}
                <Box width="8px" />
                {/* Label for joint index and type (grey if child) */}

                <SectionLabel
                    key={value.name + index}
                    size={LabelSize.Small}
                    className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                >
                    {value.name}
                </SectionLabel>
            </Box>

            {/* Button used for selecting a parent (shows up as an outline) */}
            <CustomButton
                fullWidth={true}
                onClick={() => {
                    onSelected(value)
                }}
                className={value.name}
                sx={{ borderColor: "#888888" }}
                id={`select-button-${value.name}`}
            />
            {value.tooltipText && CustomTooltip(value.tooltipText)}
            {/** Delete button only if onDelete is defined */}
            {onDelete && includeDelete && (
                <>
                    {Spacer(0, 10)}
                    {DeleteButton(onDelete != undefined ? onDelete : () => {}, "select-menu-delete-button")}
                </>
            )}
        </Box>
    )
}

interface SelectMenuProps {
    options: SelectMenuOption[]
    onOptionSelected: (val: SelectMenuOption | undefined) => void

    // Function to return a default value
    defaultSelectedOption?: (options: SelectMenuOption[]) => SelectMenuOption | undefined
    defaultHeaderText: string
    noOptionsText?: string
    indentation?: number
    onDelete?: (val: SelectMenuOption) => void | undefined

    // If false, this menu option will not have a delete button
    deleteCondition?: (val: SelectMenuOption) => boolean
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
 * @param {number} [props.indentation] - The number of indentations before the header text. Used to nest multiple select menus together.
 * @param {function} [props.onDelete] - Callback function to handle the deletion of an option. If undefined, delete buttons will not be included.
 * @param {function} [props.deleteCondition] - A function take in a specific option and return true if it's deletable.
 * @param {function} [props.onAddClicked] - Callback function to handle the addition of an option. If undefined, no add button will be included.
 *
 * @returns {JSX.Element} The rendered SelectMenu component.
 */
const SelectMenu: React.FC<SelectMenuProps> = ({
    options,
    onOptionSelected,
    defaultSelectedOption,
    defaultHeaderText,
    noOptionsText,
    indentation,
    onDelete,
    deleteCondition,
    onAddClicked,
}) => {
    const [selectedOption, setSelectedOption] = useState<SelectMenuOption | undefined>(defaultSelectedOption?.(options))

    // If the selected option no longer exists as an option, deselect it
    useEffect(() => {
        if (!options.some(o => o.name === selectedOption?.name)) {
            setSelectedOption(undefined)
            onOptionSelected(undefined)
        }
    }, [options, onOptionSelected, selectedOption])

    return (
        <>
            {/** Box containing the menu header */}
            <Box display="flex" textAlign={"center"} minHeight={"30px"} key="selected-item">
                <Box width={`${20 * (indentation ?? 0)}px`} />

                {/** Back arrow button when an option is selected */}
                {selectedOption != undefined && (
                    <ButtonIcon
                        value={SynthesisIcons.LeftArrowLarge}
                        onClick={() => {
                            setSelectedOption(undefined)
                            onOptionSelected(undefined)
                        }}
                        id="select-menu-back-button"
                    />
                )}

                {/** Label with either the header text, or the name of the selected option if an option is selected */}
                <Box alignSelf={"center"} display="flex">
                    <Box width="8px" />
                    <SectionLabel size={LabelSize.Small} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        {selectedOption != undefined ? selectedOption.name : defaultHeaderText}
                    </SectionLabel>
                </Box>
            </Box>
            <SectionDivider />

            {selectedOption == undefined && (
                <>
                    {/** List of options */}
                    {options.length > 0 ? (
                        options.map((option, i) => {
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
                                    includeDelete={deleteCondition == undefined || deleteCondition(option)}
                                />
                            )
                        })
                    ) : (
                        <>
                            {/** No options available text */}
                            <Label size={LabelSize.Small}>{noOptionsText ?? "No options available!"}</Label>
                        </>
                    )}
                    {/** Add button */}
                    {onAddClicked && AddButtonInteractiveColor(onAddClicked, "select-menu-add-button")}
                </>
            )}
        </>
    )
}

export default SelectMenu
