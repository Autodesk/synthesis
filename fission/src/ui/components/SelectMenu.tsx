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
                sx={{ borderColor: "#888888" }}
            />
            {value.tooltipText && CustomTooltip(value.tooltipText)}
            {/** Delete button only if onDelete is defined */}
            {onDelete && includeDelete && (
                <>
                    {Spacer(0, 10)}
                    {DeleteButton(onDelete != undefined ? onDelete : () => {})}
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
                    {onAddClicked && AddButtonInteractiveColor(onAddClicked)}
                </>
            )}
        </>
    )
}

export default SelectMenu
