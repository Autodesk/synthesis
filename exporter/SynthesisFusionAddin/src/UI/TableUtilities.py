import adsk.fusion, traceback
from ..general_imports import *
from . ConfigCommand import *

def removeWheelFromTable(index: int, _wheels, wheelTableInput) -> None:
    wheel = _wheels[index]

    def removePreselections(child_occurrences):
        for occ in child_occurrences:
            onSelect.allWheelPreselections.remove(occ.entityToken)
            
            if occ.childOccurrences:
                removePreselections(occ.childOccurrences)

    try:
        if wheel.childOccurrences:
            removePreselections(wheel.childOccurrences)
        else:
            onSelect.allWheelPreselections.remove(wheel.entityToken)

        del _wheels[index]
        wheelTableInput.deleteRow(index + 1)
    except IndexError:
        pass
    except:
        logging.getLogger("{INTERNAL_ID}.UI.TableUtilities").error(
            "Failed:\n{}".format(traceback.format_exc())
        )