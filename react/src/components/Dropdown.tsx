import React, { useState } from 'react';
import { FaChevronDown, FaChevronUp } from 'react-icons/fa';

type DropdownProps = {
    label: string;
    options: string[];
}


const Dropdown: React.FC<DropdownProps> = ({ label, options }) => {
    const [expanded, setExpanded] = useState(false);
    const [optionList, setOptionList] = useState(options);

    type DropdownOptionProps = {
        value: string;
    }

    const DropdownOption: React.FC<DropdownOptionProps> = ({ children, value }) => (
        <span onClick={() => {
            const newOptions = options.filter(item => item !== value)
            newOptions.unshift(value)
            setOptionList(newOptions);
        }} className="block relative duration-100 hover:backdrop-brightness-90 w-full h-full px-2 py-2">{children}</span>
    );

    return (
        <div
            onClick={() => setExpanded(!expanded)}
            className="relative flex flex-col gap-2 select-none cursor-pointer bg-gradient-to-r from-red-500 to-orange-500 w-full h-full rounded-md">
            <DropdownOption value={optionList[0]}>
                {optionList[0]}
                <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-col h-1/2 content-center items-center">
                    <FaChevronUp className="h-1/2" />
                    <FaChevronDown className="h-1/2" />
                </div>
            </DropdownOption>
            {
                expanded &&
                optionList.slice(1).map(o => (<DropdownOption key={o} value={o}>{o}</DropdownOption>))
            }
        </div >
    );
}

export default Dropdown;
