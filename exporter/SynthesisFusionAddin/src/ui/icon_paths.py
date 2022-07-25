from . import os_helper
import os

"""
Dictionaries that store all the icon paths in ConfigCommand. All path strings are OS-independent
"""
resources = os_helper.getOSPath(".", "src", "Resources") # str shortcut with primary directories to all images in file

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
    "add": resources + os.path.join("MousePreselectIcons", "mouse-add-icon.png"),
    "remove": resources + os.path.join("MousePreselectIcons", "mouse-remove-icon.png"),
}

massIcons = {
    "KG": resources + os.path.join("kg_icon"), # resource folder
    "LBS": resources + os.path.join("lbs_icon"), # resource folder
}

signalIcons = {
    "PWM": resources + os.path.join("PWM_icon"), # resource folder
    "CAN": resources + os.path.join("CAN_icon"), # resource folder
    "PASSIVE": resources + os.path.join("PASSIVE_icon"), # resource folder
}

stringIcons = {
    "calculate-enabled": resources + os.path.join("AutoCalcWeight_icon"), # resource folder
    "friction_override-enabled": resources + os.path.join("FrictionOverride_icon"), # resource folder
}
