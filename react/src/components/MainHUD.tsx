import React from 'react';
import Button from './Button';
import { FaGear, FaPlus, FaHouse, FaMagnifyingGlass, FaXbox, FaPeopleGroup, FaDownload, FaCar } from 'react-icons/fa6';
import { useModalControlContext } from '../ModalContext';
import logo from '../assets/autodesk_logo.png'

type MainHUDProps = {

}

type ButtonProps = {
    value: string;
    icon: React.ReactNode;
    onClick?: () => void;
    larger?: boolean;
}

const MainHUDButton: React.FC<ButtonProps> = ({ value, icon, onClick, larger }) => {
    if (larger == null) larger = false;
    return (
        <div onClick={onClick}
            className={`relative flex flex-row cursor-pointer bg-black w-full m-auto px-2 py-1 text-white rounded-md ${larger ? "justify-center" : ""} items-center hover:backdrop-brightness-105`}>
            {larger && icon}
            {!larger && (
                <span onClick={onClick} className="absolute left-3">{icon}</span>
            )}
            <input type="button" className={`px-2 ${larger ? "py-2" : "py-1 ml-6"} text-white cursor-pointer`} value={value} onClick={onClick} />
        </div>
    )
}

const MainHUD: React.FC<MainHUDProps> = () => {
    const { openModal, closeModal } = useModalControlContext();

    return (
        <div className="fixed flex flex-col gap-2 bg-gradient-to-b from-orange-500 to-red-500 w-min p-4 rounded-3xl ml-4 top-1/2 -translate-y-1/2">
            <img src={logo} width={'80%'} />
            <MainHUDButton value={"Spawn Asset"} icon={<FaPlus />} larger={true} />
            <div className="flex flex-col gap-0 bg-black w-full rounded-3xl">
                <MainHUDButton value={"Configuration"} icon={<FaGear />} onClick={() => openModal("configuration")} />
                <MainHUDButton value={"View"} icon={<FaMagnifyingGlass />} onClick={() => openModal("view")} />
                <MainHUDButton value={"Controls"} icon={<FaXbox />} onClick={() => openModal("controls")} />
                <MainHUDButton value={"MultiBot"} icon={<FaPeopleGroup />} onClick={() => openModal("multibot")} />
            </div>
            <div className="flex flex-col gap-0 bg-black w-full rounded-3xl">
                <MainHUDButton value={"Download Asset"} icon={<FaDownload />} onClick={() => openModal("download-assets")} />
                <MainHUDButton value={"RoboRIO"} icon={<FaGear />} onClick={() => openModal("roborio")} />
                <MainHUDButton value={"Driver Station"} icon={<FaGear />} onClick={() => openModal("driverstation")} />
                <MainHUDButton value={"Drivetrain"} icon={<FaCar />} onClick={() => openModal("drivetrain")} />
            </div>
            <MainHUDButton value={"Home"} icon={<FaHouse />} larger={true} />
        </div>
    )
}

export default MainHUD;
