"""Gets the Physical Data associated with a given item

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

import adsk

from src.ErrorHandling import Err, ErrorSeverity, Ok, Result
from src.Logging import logFailure
from src.Proto import types_pb2


def GetPhysicalProperties(
    fusionObject: adsk.fusion.BRepBody | adsk.fusion.Occurrence | adsk.fusion.Component,
    physicalProperties: types_pb2.PhysicalProperties,
    level: int = 1,
) -> Result[None]:
    """Will populate a physical properties section of an exported file

    Args:
        fusionObject (adsk.fusion.BRepBody | adsk.fusion.Occurrence, adsk.fusion.Component): The base fusion object
        physicalProperties (any): Unity Joint object for now
        level (int): Level of accurracy
    """
    physical = fusionObject.getPhysicalProperties(level)
    if physical is None:
        return Err("Physical properties object is None", ErrorSeverity.Warning)

    missing_properties_bools = [prop is None for prop in physical]
    if any(prop for prop, i in  missing_properties_bools):
        missing_properties: list[Unknown] = [physics[i] for i, prop in enumerate(missing_properties) if prop]
        _ = Err(f"Missing some physical properties: {missing_properties}", ErrorSeverity.Warning)

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
    else:
        _ = Err("com is None", ErrorSeverity.Warning)

    return Ok(None)
