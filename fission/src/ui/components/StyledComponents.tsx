import { Divider, styled } from "@mui/material"
import Label from "./Label"
import Button, { ButtonProps, ButtonSize } from "./Button"
import { IoCheckmark, IoPencil, IoTrashBin } from "react-icons/io5"
import { HiDownload } from "react-icons/hi"
import { AiOutlinePlus } from "react-icons/ai"
import { BiRefresh } from "react-icons/bi"

export const DeleteIcon = <IoTrashBin size={"1.25rem"} />
export const DownloadIcon = <HiDownload size={"1.25rem"} />
export const AddIcon = <AiOutlinePlus size={"1.25rem"} />
export const RefreshIcon = <BiRefresh size={"1.25rem"} />
export const SelectIcon = <IoCheckmark size={"1.25rem"} />
export const EditIcon = <IoPencil size={"1.25rem"} />

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
    return <PositiveButton value={DeleteIcon} onClick={onClick} />
}

export const AddButton = (onClick: () => void) => {
    return <PositiveButton value={DeleteIcon} onClick={onClick} />
}

export const SelectButton = (onClick: () => void) => {
    return <PositiveButton value={SelectIcon} onClick={onClick} />
}

export const EditButton = (onClick: () => void) => {
    return <PositiveButton value={EditIcon} onClick={onClick} />
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
    return <NegativeButton value={DeleteIcon} onClick={onClick} />
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
    return <ButtonIcon value={RefreshIcon} onClick={onClick} />
}

export const AddButtonInteractiveColor = (onClick: () => void) => {
    return <Button value={AddIcon} onClick={onClick} />
}
