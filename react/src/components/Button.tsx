import React from "react";

enum ButtonSize {
    Small,
    Medium,
    Large,
    XL
}

type ButtonProps = {
    value: string;
    size?: ButtonSize;
    onClick?: () => void;
};

const Button: React.FC<ButtonProps> = ({ value, size, onClick }) => {
    let sizeClassNames;

    if (!size) size = ButtonSize.Medium;

    switch (size) {
        case ButtonSize.Small:
            sizeClassNames = "px-2 py-1";
            break;
        case ButtonSize.Medium:
            sizeClassNames = "px-4 py-1";
            break;
        case ButtonSize.Large:
            sizeClassNames = "px-8 py-2";
            break;
        case ButtonSize.XL:
            sizeClassNames = "px-10 py-2";
            break;
    }

    return (
        <input
            type="button"
            value={value}
            onClick={onClick}
            className={`bg-gradient-to-r from-orange-500 via-red-500 to-orange-500 bg-[length:200%_100%] w-min ${sizeClassNames} rounded-sm font-semibold cursor-pointer duration-200 active:bg-right`}
        />
    );
}

export default Button;
