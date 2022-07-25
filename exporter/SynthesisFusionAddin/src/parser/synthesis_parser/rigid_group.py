""" Module to define static methods to extract rigidGroup information from a given occurrence or component

    NOT CURRENTLY IN USE BECAUSE OF BUG

 - Will directly add it to the given Assembly Message

 Takes:
    - Occurrence
    - Component

 Returns:
    - Success
"""
import adsk.core, adsk.fusion, logging
from typing import *

from proto.proto_out import assembly_pb2


def ExportRigidGroups(
    fus_occ: Union[adsk.fusion.Occurrence, adsk.fusion.Component],
    hel_occ: assembly_pb2.Occurrence,
) -> None:
    """Takes a Fusion and Protobuf Occurrence and will assign Rigidbody data per the occurrence if any exist and are not surpressed.

    - Also is supplied by the root component for ease
    - Appears to have a bug, logged already

    Args:
        fus_occ (adsk.fusion.Occurrence): Fusion 360 Occurrence Reference
        hel_occ (Assembly_pb2.Occurrence): Protobuf Hellion Occurrence Reference
    """
    try:
        log = logging.getLogger("{INTERNAL_ID}.parser.RigidGroup")

        groups = fus_occ.rigidGroups

        if groups is None:
            return

        _rigidGroups = hel_occ.rigidgroups

        for group in groups:
            if not group.isSuppressed and group.isValid:
                _group = _rigidGroups.add()
                _group.name = group.name
                for occurrence in group.occurrences:
                    _group.occurrences.extend(occurrence.fullPathName)
    except:
        if log:
            log.error(f"Exception thrown while parsing rigidgroups in {fus_occ.name}")
