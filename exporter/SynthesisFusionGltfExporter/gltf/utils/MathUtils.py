from typing import List
import numpy as np
from pyquaternion import Quaternion

IDENTITY_MATRIX_3D = (
    1, 0, 0, 0,
    0, 1, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1,
)

def isIdentityMatrix3D(flatMatrix: List[float], tolerance: float = 0.00001) -> bool:
    """Determines whether the input matrix is equal to the 4x4 identity matrix.

    Args:
        flatMatrix: The flat Matrix3D to compare.
        tolerance: The maximum distance from the true identity matrix to tolerate.


    Returns: True if the given matrix is equal to the identity matrix within the tolerance.
    """
    for i in range(len(IDENTITY_MATRIX_3D)):

        if abs(flatMatrix[i] - IDENTITY_MATRIX_3D[i]) > tolerance:
            return False
    return True

def transposeFlatMatrix3D(flatMatrix: List[float]):
    return [flatMatrix[i + j * 4] for i in range(4) for j in range(4)]


def normalized(a, axis=-1, order=2):
    l2 = np.atleast_1d(np.linalg.norm(a, order, axis))
    l2[l2 == 0] = 1
    return (a / np.expand_dims(l2, axis))[0]

def forwardUpVectorsToRotation(forward, up):
    zAxis = normalized(forward)
    xAxis = normalized(np.cross(up, forward))
    yAxis = normalized(np.cross(forward, xAxis))

    m1 = np.array([
        xAxis.tolist(),
        yAxis.tolist(),
        zAxis.tolist(),
    ])

    return Quaternion(matrix=m1.transpose()).unit

def gltfQuatToPy(rotation):
    return Quaternion(rotation[3:4] + rotation[0:3])

def pyQuatToGltf(pyQuat):
    normalized = pyQuat.normalised.elements.tolist()
    return normalized[1:4] + normalized[0:1]