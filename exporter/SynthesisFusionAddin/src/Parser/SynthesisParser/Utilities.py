import math, uuid
from adsk.core import Vector3D

# from proto.proto_out import types_pb2


def fill_info(proto_obj, fus_object, override_guid=None) -> None:
    construct_info("", proto_obj, fus_object=fus_object, GUID=override_guid)


def construct_info(name: str, proto_obj, version=4, fus_object=None, GUID=None) -> None:
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
    elif name is not None:
        proto_obj.info.name = name
    else:
        raise ValueError("Cannot construct info from no name or fus_object")

    if GUID is not None:
        proto_obj.info.GUID = str(GUID)
    else:
        try:
            # attempt to get entity token
            proto_obj.info.GUID = fus_object.entityToken
        except:
            # fails and gets new uuid
            proto_obj.info.GUID = str(uuid.uuid4())


# My previous function was alot more optimized however now I realize the bug was this doesn't work well with degrees
def euler_to_quaternion(r):
    (yaw, pitch, roll) = (r[0], r[1], r[2])
    qx = math.sin(roll / 2) * math.cos(pitch / 2) * math.cos(yaw / 2) - math.cos(
        roll / 2
    ) * math.sin(pitch / 2) * math.sin(yaw / 2)
    qy = math.cos(roll / 2) * math.sin(pitch / 2) * math.cos(yaw / 2) + math.sin(
        roll / 2
    ) * math.cos(pitch / 2) * math.sin(yaw / 2)
    qz = math.cos(roll / 2) * math.cos(pitch / 2) * math.sin(yaw / 2) - math.sin(
        roll / 2
    ) * math.sin(pitch / 2) * math.cos(yaw / 2)
    qw = math.cos(roll / 2) * math.cos(pitch / 2) * math.cos(yaw / 2) + math.sin(
        roll / 2
    ) * math.sin(pitch / 2) * math.sin(yaw / 2)
    return [qx, qy, qz, qw]


def rad_to_deg(rad):
    """Very simple method to convert Radians to degrees

    Args:
        rad (float): radians unit

    Returns:
        float: degrees
    """
    return (rad * 180) / math.pi


def quaternion_to_euler(qx, qy, qz, qw):
    """Takes in quat values and converts to degrees

    - roll is x axis - atan2(2(qwqy + qzqw), 1-2(qy^2 + qz^2))
    - pitch is y axis - asin(2(qxqz - qwqy))
    - yaw is z axis  - atan2(2(qxqw + qyqz), 1-2(qz^2+qw^3))

    Args:
        qx (float): quat_x
        qy (float): quat_y
        qz (float): quat_z
        qw (float): quat_w

    Returns:
        roll: x value in degrees
        pitch: y value in degrees
        yaw: z value in degrees
    """
    # roll
    sr_cp = 2 * ((qw * qx) + (qy * qz))
    cr_cp = 1 - (2 * ((qx * qx) + (qy * qy)))
    roll = math.atan2(sr_cp, cr_cp)
    # pitch
    sp = 2 * ((qw * qy) - (qz * qx))
    if abs(sp) >= 1:
        pitch = math.copysign(math.pi / 2, sp)
    else:
        pitch = math.asin(sp)
    # yaw
    sy_cp = 2 * ((qw * qz) + (qx * qy))
    cy_cp = 1 - (2 * ((qy * qy) + (qz * qz)))
    yaw = math.atan2(sy_cp, cy_cp)
    # convert to degrees
    roll = rad_to_deg(roll)
    pitch = rad_to_deg(pitch)
    yaw = rad_to_deg(yaw)
    # round and return
    return round(roll, 4), round(pitch, 4), round(yaw, 4)


def throwZero():
    """Simple function to report incorrect quat values

    Raises:
        RuntimeError: Error describing the issue
    """
    raise RuntimeError(
        "While computing the quaternion the trace was reported as 0 which is invalid"
    )


def spatial_to_quaternion(mat):
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
        raise RuntimeError(
            "Supplied matrix to spatial_to_quaternion is not a 1D spatial matrix in size."
        )


def normalize_quaternion(x, y, z, w):
    f = 1.0 / math.sqrt((x * x) + (y * y) + (z * z) + (w * w))
    return x * f, y * f, z * f, w * f


def _getAngleTo(vec_origin: list, vec_current: Vector3D) -> int:
    origin = Vector3D.create(vec_origin[0], vec_origin[1], vec_origin[2])
    val = origin.angleTo(vec_current)
    deg = val * (180 / math.pi)
    return val
