from typing import Any, Callable, Dict, List, TypedDict, cast

import adsk.core
import adsk.fusion

from src import Logging, gm
from src.UI import IconPaths

logger = Logging.getLogger()


class DesignRule(TypedDict):
    name: str
    calculation: Callable[[], float]
    max_value: float


class DesignCheckTab:
    designCheckTab: adsk.core.TabCommandInput
    designCheckTable: adsk.core.TableCommandInput
    designRules: Dict[str, Any] = {}
    design_rules: List[DesignRule]

    @Logging.logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        self.designCheckTab = args.command.commandInputs.addTabCommandInput("designCheckTab", "Design Rule Check")
        designCheckTabInputs = self.designCheckTab.children

        # Create the table for design checks
        self.designCheckTable = designCheckTabInputs.addTableCommandInput(
            "designCheckTable", "Design Checks", 3, "3:2:2"
        )
        self.designCheckTable.tablePresentationStyle = (
            adsk.core.TablePresentationStyles.itemBorderTablePresentationStyle
        )

        # Define and add design rules to the table
        self.design_rules = [
            {
                "name": "Design Height",
                "calculation": self.fusion_design_height,
                "max_value": 106.0,  # cm
            },
            {
                "name": "Design Perimeter",
                "calculation": self.fusion_design_perimeter,
                "max_value": 304.0,  # cm
            },
        ]

        for i, rule in enumerate(self.design_rules):
            calculation = rule["calculation"]
            max_value: float = rule["max_value"]
            value: float = calculation()
            is_valid: bool = value <= max_value
            rule_name: str = str(rule["name"])
            rule_id: str = rule_name.replace(" ", "")

            name_input = designCheckTabInputs.addTextBoxCommandInput(f"{rule_id}Name", rule_name, rule_name, 1, True)
            value_input = designCheckTabInputs.addTextBoxCommandInput(
                f"{rule_id}Value", "Value", f"{value:.2f} cm", 1, True
            )
            icon_input = designCheckTabInputs.addImageCommandInput(
                f"{rule_id}StatusIcon",
                "",
                IconPaths.designCheckIcons["valid"] if is_valid else IconPaths.designCheckIcons["invalid"],
            )

            self.designCheckTable.addCommandInput(name_input, i, 0)
            self.designCheckTable.addCommandInput(value_input, i, 1)
            self.designCheckTable.addCommandInput(icon_input, i, 2)

    @property
    def isVisible(self) -> bool:
        return self.designCheckTab.isVisible or False

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.designCheckTab.isVisible = value

    @property
    def isActive(self) -> bool:
        return self.designCheckTab.isActive or False

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
