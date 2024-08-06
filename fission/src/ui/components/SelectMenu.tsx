import React, { useEffect, useState } from "react"

import { Box, Button as MUIButton, styled, alpha, Divider } from "@mui/material"
import Label, { LabelSize } from "./Label"
import { FaArrowLeft } from "react-icons/fa6"
import Button from "./Button"
import { AddButtonInteractiveColor, DeleteButton, Spacer } from "./StyledComponents"

const LeftArrow = <FaArrowLeft size={"1.25rem"} />

const CustomButton = styled(MUIButton)({
    "borderStyle": "none",
    "borderWidth": "1px",
    "transition": "border-color 0.3s ease",
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
        borderColor: "grey",
    },
})

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    textWrap: "nowrap",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

/** Extend this to make a type that contains custom data */
export class SelectMenuOption {
    name: string
    constructor(name: string) {
        this.name = name
    }
}

interface OptionCardProps {
    value: SelectMenuOption
    index: number
    onSelected: (val: SelectMenuOption) => void
    onDelete?: () => void
}

const OptionCard: React.FC<OptionCardProps> = ({ value, index, onSelected, onDelete }) => {
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
                <LabelStyled
                    key={value.name + index}
                    size={LabelSize.Small}
                    className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                >
                    {value.name}
                </LabelStyled>
            </Box>

            {/* Button used for selecting a parent (shows up as an outline) */}
            <CustomButton
                fullWidth={true}
                onClick={() => {
                    onSelected(value)
                }}
                sx={{ borderColor: "#888888" }}
            />
            {onDelete && (
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
    defaultHeaderText: string
    indentation?: number
    onDelete?: (val: SelectMenuOption) => void | undefined
    onAddClicked?: () => void
}

const SelectMenu: React.FC<SelectMenuProps> = ({
    options,
    onOptionSelected,
    defaultHeaderText: headerText,
    indentation,
    onDelete,
    onAddClicked,
}) => {
    const [selectedOption, setSelectedOption] = useState<SelectMenuOption | undefined>(undefined)
    // useEffect(() => {
    //     if (selectedOption == undefined)
    //         return
    //
    //     if (!options.some(o => o.name == selectedOption.name)) setSelectedOption(undefined)
    // }, [options, selectedOption])

    useEffect(() => {
        if (!options.some(o => o.name === selectedOption?.name)) {
            setSelectedOption(undefined)
            onOptionSelected(undefined) // Notify parent if needed
        }
    }, [options])

    return (
        <>
            <Box display="flex" textAlign={"center"} minHeight={"30px"} key="selected-item">
                <Box width={`${20 * (indentation ?? 0)}px`} />
                {selectedOption != undefined ? (
                    <Button
                        value={LeftArrow}
                        onClick={() => {
                            setSelectedOption(undefined)
                            onOptionSelected(undefined)
                        }}
                        colorOverrideClass="bg-[#00000000] hover:brightness-90"
                        sizeOverrideClass="p-[0.25rem]"
                    />
                ) : null}
                <Box alignSelf={"center"} display="flex">
                    <Box width="8px" />
                    <LabelStyled size={LabelSize.Small} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        {selectedOption != undefined ? selectedOption.name : headerText}
                    </LabelStyled>
                </Box>
            </Box>
            <DividerStyled />
            {selectedOption == undefined ? (
                <>
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
                            />
                        )
                    })}
                    {onAddClicked && AddButtonInteractiveColor(onAddClicked)}
                </>
            ) : null}
        </>
    )
}

export default SelectMenu
