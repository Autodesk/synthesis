import React from 'react';

type ButtonProps = {
    value: string;
    onClick?: () => void;
}

const Button: React.FC<ButtonProps> = ({ value, onClick }) => (
    <input type="button" value={value} onClick={onClick} className="bg-gradient-to-r from-orange-500 to-red-500 w-min px-8 py-2 rounded-sm font-semibold" />
);

export default Button;
