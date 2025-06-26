import math
import uuid

import adsk.core
import adsk.fusion

from src.ErrorHandling import Err, ErrorSeverity, Ok, Result
from src.Proto import assembly_pb2, types_pb2


def guid_component(comp: adsk.fusion.Component) -> str:
    return f"{comp.entityToken}_{comp.id}"


def guid_occurrence(occ: adsk.fusion.Occurrence) -> str:
    return f"{occ.entityToken}_{guid_component(occ.component)}"


def guid_none(_: None) -> str:
    return str(uuid.uuid4())


def fill_info(proto_obj: assembly_pb2.Assembly, fus_object: adsk.core.Base, override_guid: str | None = None) -> Result[None]:
    return construct_info("", proto_obj, fus_object=fus_object, GUID=override_guid)


def construct_info(
    name: str,
    proto_obj: assembly_pb2.Assembly,
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
    else:
        return Err("Attempted to set proto_obj.info.name to None", ErrorSeverity.Warning)

    if GUID is not None:
        proto_obj.info.GUID = str(GUID)
    elif fus_object is not None and hasattr(fus_object, "entityToken"):
        proto_obj.info.GUID = fus_object.entityToken
    else:
        proto_obj.info.GUID = str(uuid.uuid4())

    return Ok(None)




def rad_to_deg(rad):  # type: ignore
    """Converts radians to degrees

    Args:
        rad (float): radians unit

    Returns:
        float: degrees
    """
    return (rad * 180) / math.pi

def throwZero():  # type: ignore
    """Errors on incorrect quat values

    Raises:
        RuntimeError: Error describing the issue
    """
    raise RuntimeError("While computing the quaternion the trace was reported as 0 which is invalid")


def spatial_to_quaternion(mat):  # type: ignore
    """Takes a 1D Spatial Transform Matrix and derives rotational quaternion

    I wrote this however it is difficult to extensibly test so use with caution
    Args:
        mat (list): spatial transform matrix

    Raises:
        RuntimeError: matrix is not of the correct size

    Returns:
        x, y, z, w: float representation of quaternions
    """
    if len(mat) > 15:
        trace = mat[0] + mat[5] + mat[10]
        if trace > 0:
            s = math.sqrt(trace + 1.0) * 2
            if s == 0:
                throwZero()
            qw = 0.25 * s
            qx = (mat[9] - mat[6]) / s
            qy = (mat[2] - mat[8]) / s
            qz = (mat[4] - mat[1]) / s
        elif (mat[0] > mat[5]) and (mat[0] > mat[8]):
            s = math.sqrt(1.0 + mat[0] - mat[5] - mat[10]) * 2.0
            if s == 0:
                throwZero()
            qw = (mat[9] - mat[6]) / s
            qx = 0.25 * s
            qy = (mat[1] + mat[4]) / s
            qz = (mat[2] + mat[8]) / s
        elif mat[5] > mat[10]:
            s = math.sqrt(1.0 + mat[5] - mat[0] - mat[10]) * 2.0
            if s == 0:
                throwZero()
            qw = (mat[2] - mat[8]) / s
            qx = (mat[1] + mat[4]) / s
            qy = 0.25 * s
            qz = (mat[6] + mat[9]) / s
        else:
            s = math.sqrt(1.0 + mat[10] - mat[0] - mat[5]) * 2.0
            if s == 0:
                throwZero()
            qw = (mat[4] - mat[1]) / s
            qx = (mat[2] + mat[8]) / s
            qy = (mat[6] + mat[9]) / s
            qz = 0.25 * s

        # normalizes the value - as demanded by unity
        qx, qy, qz, qw = normalize_quaternion(qx, qy, qz, qw)

        # So these quat values need to be reversed? I have no idea why at the moment
        return round(qx, 13), round(-qy, 13), round(-qz, 13), round(qw, 13)

    else:
        raise RuntimeError("Supplied matrix to spatial_to_quaternion is not a 1D spatial matrix in size.")


def normalize_quaternion(x, y, z, w):  # type: ignore
    f = 1.0 / math.sqrt((x * x) + (y * y) + (z * z) + (w * w))
    return x * f, y * f, z * f, w * f


def _getAngleTo(vec_origin: list, vec_current: adsk.core.Vector3D) -> int:  # type: ignore
    origin = adsk.core.Vector3D.create(vec_origin[0], vec_origin[1], vec_origin[2])
    val = origin.angleTo(vec_current)
    deg = val * (180 / math.pi)
    return val  # type: ignore
