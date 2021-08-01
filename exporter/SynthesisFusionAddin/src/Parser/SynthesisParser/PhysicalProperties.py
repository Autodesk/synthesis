""" Gets the Physical Data associated with a given item

    Takes:
     - BRepBody
     - Occurrence
     - Component

    Adds:
     - Density
     - Mass
     - Volume
     - COM
        - X
        - Y
        - Z

"""
import adsk, logging, traceback

from proto.proto_out import types_pb2
from typing import Union
from ...general_imports import INTERNAL_ID


def GetPhysicalProperties(
    fusionObject: Union[
        adsk.fusion.BRepBody, adsk.fusion.Occurrence, adsk.fusion.Component
    ],
    physicalProperties: types_pb2.PhysicalProperties,
    level=1,
):
    """Will populate a physical properties section of an exported file

    Args:
        fusionObject (Union[adsk.fusion.BRepBody, adsk.fusion.Occurrence, adsk.fusion.Component]): The base fusion object
        physicalProperties (any): Unity Joint object for now
        level (int): Level of accurracy
    """
    try:
        physical = fusionObject.getPhysicalProperties(level)

        physicalProperties.density = physical.density
        physicalProperties.mass = physical.mass
        physicalProperties.volume = physical.volume
        physicalProperties.area = physical.area

        _com = physicalProperties.com
        com = physical.centerOfMass.asVector()

        if com is not None:
            _com.x = com.x
            _com.y = com.y
            _com.z = com.z
    except:
        logging.getLogger(f"{INTERNAL_ID}.Parser.PhysicalProperties").error("Failed:\n{}".format(traceback.format_exc()))
