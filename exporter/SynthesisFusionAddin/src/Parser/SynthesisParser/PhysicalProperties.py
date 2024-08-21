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

from typing import Union

import adsk

from src.Logging import logFailure
from src.Proto import types_pb2


@logFailure
def GetPhysicalProperties(
    fusionObject: Union[adsk.fusion.BRepBody, adsk.fusion.Occurrence, adsk.fusion.Component],
    physicalProperties: types_pb2.PhysicalProperties,
    level: int = 1,
) -> None:
    """Will populate a physical properties section of an exported file

    Args:
        fusionObject (Union[adsk.fusion.BRepBody, adsk.fusion.Occurrence, adsk.fusion.Component]): The base fusion object
        physicalProperties (any): Unity Joint object for now
        level (int): Level of accurracy
    """
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
