import React, { ReactNode, useState } from "react"
import Label, { LabelSize } from "./Label"
import { Select as BaseSelect, SelectProps, selectClasses, SelectRootSlotProps } from '@mui/base/Select'
import { Option as BaseOption, optionClasses } from '@mui/base/Option'
import { styled } from '@mui/system'
import { Button } from '@mui/base/Button'
import UnfoldMoreRoundedIcon from '@mui/icons-material/UnfoldMoreRounded'
import { SelectValue } from "@mui/base/useSelect"

const Select = React.forwardRef(function Select<
    TValue extends {},
    Multiple extends boolean,
>(props: SelectProps<TValue, Multiple>, ref: React.ForwardedRef<HTMLButtonElement>) {
    const slots: SelectProps<TValue, Multiple>['slots'] = {
        root: CustomButton,
        listbox: Listbox,
        popup: Popup,
        ...props.slots
    };

    // TODO: list options don't render at the same width as select root button
    return <BaseSelect {...props} ref={ref} slots={slots} slotProps={{ listbox: {}, popup: { disablePortal: true, } }} />;
}) as <TValue extends {}, Multiple extends boolean>(
    props: SelectProps<TValue, Multiple> & React.RefAttributes<HTMLButtonElement>,
) => JSX.Element;

type DropdownProps = {
    children?: ReactNode
    label?: string
    className?: string
    options: string[]
    onSelect: (opt: string) => void
}

const Dropdown: React.FC<DropdownProps> = ({
    label,
    className,
    options,
    onSelect,
}) => {
    const [optionList, setOptionList] = useState(options)

    type DropdownOptionProps = {
        value: string
        children?: ReactNode
        className?: string
    }

    return (
        <>
            {label && <Label size={LabelSize.Medium}>{label}</Label>}
            <div className="relative w-full">
                <Select defaultValue={optionList[0]} onChange={(_event: React.MouseEvent | React.KeyboardEvent | React.FocusEvent | null, value: any) => typeof(value) === 'string' && onSelect && onSelect(value)}>
                    {optionList.map(option => (
                        <Option value={option} key={option}>{option}</Option>
                    ))}
                </Select>
            </div>
        </>
    )
}

const CustomButton = React.forwardRef(function CustomButton<
    TValue extends {},
    Multiple extends boolean,
>(
    props: SelectRootSlotProps<TValue, Multiple>,
    ref: React.ForwardedRef<HTMLButtonElement>,
) {
    const { ownerState, ...other } = props;
    return (
        <StyledButton type="button" {...other} ref={ref}>
            {other.children}
            <UnfoldMoreRoundedIcon />
        </StyledButton>
    );
});

const StyledButton = styled(Button)`
    position: relative;
    text-align: left;
    width: 100%;
    background-image: linear-gradient(to right, var(--interactive-element-left), var(--interactive-element-right));
    border-radius: 0.375rem;
    border: none;
    outline: none;
    padding-left: calc(0.8em + 8px);

    &:hover, &:focus {
        outline: none;
    }

    & > svg {
        font-size: 1rem;
        position: absolute;
        height: 100%;
        top: 0;
        right: 10px;
    }
`;

const Listbox = styled('ul')`
    box-sizing: border-box;
    width: 100%;
    background-image: linear-gradient(to right, var(--interactive-element-right), var(--interactive-element-left));
    border-radius: 1rem;
    padding: 8px;
`;

const Option = styled(BaseOption)`
    list-style: none;
    cursor: default;
    padding: 0.6em 0.8em;
    border-radius: 1rem;
    cursor: pointer;
    &:hover {
        backdrop-filter: brightness(90%);
    }
    &:hover, &:focus {
        outline: none;
    }
`;

const Popup = styled('div')`
    position: relative;
    z-index: 1;
    width: 100%;
`;

// <div
//     onClick={() => setExpanded(!expanded)}
//     className={`relative flex flex-col gap-2 select-none cursor-pointer bg-gradient-to-r from-interactive-element-left to-interactive-element-right w-full rounded-md ${className}`}
// >
//     <DropdownOption value={optionList[0]}>
//         {optionList[0]}
//         {optionList.length > 1 && (
//             <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-col h-1/2 content-center items-center">
//                 <FaChevronUp className="h-1/2" />
//                 <FaChevronDown className="h-1/2" />
//             </div>
//         )}
//     </DropdownOption>
//     {expanded &&
//         optionList.slice(1).map(o => (
//             <DropdownOption key={o} value={o}>
//                 {o}
//             </DropdownOption>
//         ))}
// </div>

export default Dropdown
