import React from 'react';

type ButtonProps = {
    value: string;
    onClick?: () => void;
}

const Button: React.FC<ButtonProps> = ({ value, onClick }) => (
    <input type="button" value={value} onClick={onClick}
        className="bg-gradient-to-r from-orange-500 via-red-500 to-orange-500 bg-[length:200%_100%] w-min px-8 py-2 rounded-sm font-semibold cursor-pointer duration-200 active:bg-right" />
);

export default Button;
