import React, { useEffect, useState } from "react"

import { Box, Button as MUIButton, styled, alpha, Divider } from "@mui/material"
import Label, { LabelSize } from "./Label"
import { FaArrowLeft } from "react-icons/fa6"
import Button from "./Button"

const LeftArrow = <FaArrowLeft size={"1.25rem"} />

const CustomButton = styled(MUIButton)({
    "borderStyle": "solid",
    "borderWidth": "1px",
    "transition": "border-color 0.3s ease",
    "&:hover": {
        borderColor: "white",
        backgroundColor: alpha("#33333333", 0.3),
    },
    "position": "relative",
    "overflow": "hidden",
    "& .MuiTouchRipple-root span": {
        backgroundColor: alpha("#ffffff", 0.3),
        animationDuration: "300ms",
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
    onSelected: (val: SelectMenuOption) => void
}

const OptionCard: React.FC<OptionCardProps> = ({ value, onSelected }) => {
    return (
        /* Box containing the entire card */
        <Box display="flex" textAlign={"center"} key={value.name} minHeight={"30px"}>
            {/* Box containing the label */}
            <Box position="absolute" alignSelf={"center"} display="flex">
                {/* Indentation before the name */}
                <Box width="8px" />
                {/* Label for joint index and type (grey if child) */}
                <LabelStyled
                    key={`arm-nodes-notation ${value}`}
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
        </Box>
    )
}

interface SelectMenuProps {
    options: SelectMenuOption[]
    onOptionSelected: (val: SelectMenuOption | undefined) => void
    headerText: string
    indentation?: number
}

const SelectMenu: React.FC<SelectMenuProps> = ({ options, onOptionSelected, headerText, indentation }) => {
    const [selectedOption, setSelectedOption] = useState<SelectMenuOption | undefined>(undefined)

    useEffect(() => {
        if (selectedOption == undefined) return

        if (!options.some(o => o.name == selectedOption.name)) setSelectedOption(undefined)
    }, [options])

    return (
        <>
            <Box
                display="flex"
                textAlign={"center"}
                minHeight={"30px"}
                // border="solid"
                // borderColor={"white"}
                key="selected-item"
            >
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
                    {options.map(option => {
                        return (
                            <OptionCard
                                value={option}
                                onSelected={val => {
                                    setSelectedOption(val)
                                    onOptionSelected(val)
                                }}
                                key={option.name}
                            />
                        )
                    })}
                </>
            ) : null}
        </>
    )
}

export default SelectMenu
