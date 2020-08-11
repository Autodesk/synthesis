from typing import List

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