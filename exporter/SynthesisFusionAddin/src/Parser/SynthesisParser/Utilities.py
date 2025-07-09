import math
from typing import Never
import uuid

import adsk.core
import adsk.fusion

from src.ErrorHandling import Err, ErrorSeverity, Ok, Result
from src.Proto import assembly_pb2, material_pb2, types_pb2


def guid_component(comp: adsk.fusion.Component) -> str:
    return f"{comp.entityToken}_{comp.id}"


def guid_occurrence(occ: adsk.fusion.Occurrence) -> str:
    return f"{occ.entityToken}_{guid_component(occ.component)}"


def guid_none(_: None) -> str:
    return str(uuid.uuid4())


def fill_info(
    proto_obj: assembly_pb2.Assembly | material_pb2.Materials,
    fus_object: adsk.core.Base,
    override_guid: str | None = None,
) -> Result[None]:
    return construct_info("", proto_obj, fus_object=fus_object, GUID=override_guid)


def construct_info(
    name: str,
    proto_obj: assembly_pb2.Assembly | material_pb2.Materials | material_pb2.PhysicalMaterial,
    version: int = 5,
    fus_object: adsk.core.Base | None = None,
    GUID: str | None = None,
) -> Result[None]:
    # TODO Fix out of date documentation
    """Constructs a info object from either a name or a fus_object

    Args:
        name (str): possible name
        version (int, optional): version. Defaults to 1.
        fus_object (adsk object, optional): Autodesk Object with name param. Defaults to None.
        GUID (str, optional): Preset GUID. Defaults to None.

    Raises:
        ValueError: If name and fus_object are none

    Returns:
        types_pb2.Info: Info object
    """

    proto_obj.info.version = version

    if fus_object is not None:
        proto_obj.info.name = fus_object.name
    elif name != "":
        proto_obj.info.name = name

    if GUID is not None:
        proto_obj.info.GUID = str(GUID)
    elif fus_object is not None and hasattr(fus_object, "entityToken"):
        proto_obj.info.GUID = fus_object.entityToken
    else:
        proto_obj.info.GUID = str(uuid.uuid4())

    return Ok(None)

