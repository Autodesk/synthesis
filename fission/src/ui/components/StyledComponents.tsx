import { Divider, styled } from "@mui/material"
import Label from "./Label"
import Button, { ButtonProps, ButtonSize } from "./Button"
import { IoCheckmark, IoPencil, IoTrashBin } from "react-icons/io5"
import { HiDownload } from "react-icons/hi"
import { AiOutlinePlus } from "react-icons/ai"
import { BiRefresh } from "react-icons/bi"
import { BsCodeSquare } from "react-icons/bs"

import { IoBasketball, IoBug, IoGameControllerOutline, IoPeople, IoTimer } from "react-icons/io5"

export const DeleteIconLarge = <IoTrashBin size={"1.25rem"} />
export const DownloadIconLarge = <HiDownload size={"1.25rem"} />
export const AddIconLarge = <AiOutlinePlus size={"1.25rem"} />
export const RefreshIconLarge = <BiRefresh size={"1.25rem"} />
export const SelectIconLarge = <IoCheckmark size={"1.25rem"} />
export const EditIconLarge = <IoPencil size={"1.25rem"} />

export const RefreshIcon = <BiRefresh />
export const BasketballIcon = <IoBasketball />
export const BugIcon = <IoBug />
export const ControllerIcon = <IoGameControllerOutline />
export const PeopleIcon = <IoPeople />
export const TimerIcon = <IoTimer />

export const CodeSquare = <BsCodeSquare />

export const SectionDivider = styled(Divider)({
    borderColor: "white",
})

export const SectionLabel = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

export const PositiveButton: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-accept-button hover:brightness-90"
        />
    )
}

export const DownloadButton = (onClick: () => void) => {
    return <PositiveButton value={DeleteIconLarge} onClick={onClick} />
}

export const AddButton = (onClick: () => void) => {
    return <PositiveButton value={DeleteIconLarge} onClick={onClick} />
}

export const SelectButton = (onClick: () => void) => {
    return <PositiveButton value={SelectIconLarge} onClick={onClick} />
}

export const EditButton = (onClick: () => void) => {
    return <PositiveButton value={EditIconLarge} onClick={onClick} />
}

export const NegativeButton: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-cancel-button hover:brightness-90"
        />
    )
}

export const DeleteButton = (onClick: () => void) => {
    return <NegativeButton value={DeleteIconLarge} onClick={onClick} />
}

export const ButtonIcon: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-[#00000000] hover:brightness-90"
            sizeOverrideClass="p-[0.25rem]"
        />
    )
}

export const RefreshButton = (onClick: () => void) => {
    return <ButtonIcon value={RefreshIconLarge} onClick={onClick} />
}

export const AddButtonInteractiveColor = (onClick: () => void) => {
    return <Button value={AddIconLarge} onClick={onClick} />
}
