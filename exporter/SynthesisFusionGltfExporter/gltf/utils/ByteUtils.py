import io

def calculateAlignmentNumPadding(currentSize: int, byteAlignment: int = 4) -> int:
    """Calculates the number of bytes needed to pad a data structure to the given alignment.

    Args:
        currentSize: The current size of the data structure that needs alignment.
        byteAlignment: The required byte alignment, e.g. 4 for 32 bit alignment.

    Returns: The number of bytes of padding which need to be added to the end of the structure.
    """
    if currentSize % byteAlignment == 0:
        return 0
    return byteAlignment - currentSize % byteAlignment

def calculateAlignment(currentSize: int, byteAlignment: int = 4) -> int:
    """Calculate the new length of the a data structure after it has been aligned.

    Args:
        currentSize: The current length of the data structure in bytes.
        byteAlignment: The required byte alignment, e.g. 4 for 32 bit alignment.

    Returns: The new length of the data structure in bytes.
    """
    return currentSize + calculateAlignmentNumPadding(currentSize, byteAlignment)

def alignBytesIOToBoundary(stream: io.BytesIO, byteAlignment: int = 4) -> None:
    stream.write(b'\x00\x00\x00'[0:calculateAlignmentNumPadding(stream.seek(0, io.SEEK_END), byteAlignment)])
    assert stream.tell() % byteAlignment == 0

def alignByteArrayToBoundary(byteArray: bytearray, byteAlignment: int = 4) -> None:
    byteArray.extend(b'   '[0:calculateAlignmentNumPadding(len(byteArray), byteAlignment)])
    assert len(byteArray) % byteAlignment == 0