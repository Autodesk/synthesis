from . import OsHelper
import os

resources = OsHelper.getOSPath(".", "src", "Resources") # str shortcut with primary directories to all images in file

wheelIcons = {
    "omni": resources + os.path.join("WheelIcons", "omni-wheel-preview190x24.png"),
    "standard": resources + os.path.join("WheelIcons", "standard-wheel-preview190x24.png"),
    "mecanum": resources + os.path.join("WheelIcons", "mecanum-wheel-preview190x24.png"),
}

jointIcons = {
    "rigid": resources + os.path.join("JointIcons", "JointRigid", "rigid190x24.png"),
    "revolute": resources + os.path.join("JointIcons", "JointRev", "revolute190x24.png"),
    "slider": resources + os.path.join("JointIcons", "JointSlider", "slider190x24.png"),
    "cylindrical": resources + os.path.join("JointIcons", "JointCyl", "cylindrical190x24.png"),
    "pin_slot": resources + os.path.join("JointIcons", "JointPinSlot", "pin_slot190x24.png"),
    "planar": resources + os.path.join("JointIcons", "JointPlanar", "planar190x24.png"),
    "ball": resources + os.path.join("JointIcons", "JointBall", "ball190x24.png"),
}

gamepieceIcons = {
    "blank": resources + "blank-preview16x16.png",
}

mouseIcons = {
    "add": resources + os.path.join("MousePreselectIcons" + "mouse-add-icon.png"),
    "remove": resources + os.path.join("MousePreselectIcons" + "mouse-remove-icon.png"),
}

massIcons = {
    "KG": resources + os.path.join("kg_icon" + "16x16-normal.png"),
    "LBS": resources + os.path.join("lbs_icon" + "16x16-normal.png"),
}

signalIcons = {
    "PWM": resources + os.path.join("PWM_icon" + "16x16-normal.png"),
    "CAN": resources + os.path.join("CAN_icon" + "16x16-normal.png"),
    "PASSIVE": resources + os.path.join("PASSIVE_icon" + "16x16-normal.png"),
}

stringIcons = {
    "calculate-enabled": resources + os.path.join("AutoCalcWeight_icon" + "16x16-normal.png"),
    "calculate-disabled": resources + os.path.join("AutoCalcWeight_icon" + "16x16-disabled.png"),
    "friction_override-enabled": resources + os.path.join("FrictionOverride_icon" + "16x16-normal.png"),
    "friction_override-disabled": resources + os.path.join("FrictionOverride_icon" + "16x16-disabled.png"),
}