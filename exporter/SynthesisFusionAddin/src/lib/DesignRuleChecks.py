from typing import Callable, List, TypedDict

import adsk.core
import adsk.fusion

from src import Logging, gm


class DesignRule(TypedDict):
    name: str
    calculation: Callable[[], float]
    max_value: float


class DesignRuleChecks:
    designRules: List[DesignRule]

    @Logging.logFailure
    def __init__(self) -> None:
        self.designRules = [
            {
                "name": "Design Height",
                "calculation": self.fusion_design_height(),
                "max_value": 106.0,  # cm
            },
            {
                "name": "Design Perimeter",
                "calculation": self.fusion_design_perimeter(),
                "max_value": 304.0,  # cm
            },
        ]

    def getDesignRules(self) -> List[DesignRule]:
        return self.designRules

    @Logging.logFailure
    def fusion_design_height(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return float(overall_bounding_box.width)
        return 0.0

    @Logging.logFailure
    def fusion_design_perimeter(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return float(2 * (overall_bounding_box.height + overall_bounding_box.length))
        return 0.0
