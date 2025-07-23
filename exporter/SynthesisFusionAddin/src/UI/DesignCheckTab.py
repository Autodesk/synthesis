import adsk.core
import adsk.fusion

from src import Logging, gm
from src.UI import IconPaths


class DesignCheckTab:
    designCheckTab: adsk.core.TabCommandInput
    designCheckTable: adsk.core.TableCommandInput

    MAX_HEIGHT = 106.0  # cm
    MAX_PERIMETER = 304.0  # cm

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

        # Row 1: Design Height
        height = self.fusion_design_height
        is_height_valid = height <= self.MAX_HEIGHT

        height_name_input = designCheckTabInputs.addTextBoxCommandInput(
            "designHeightText", "Design Height", "Design Height", 1, True
        )
        height_value_input = designCheckTabInputs.addTextBoxCommandInput(
            "designHeightValue", "Value", f"{height:.2f} cm", 1, True
        )
        height_icon_input = designCheckTabInputs.addImageCommandInput(
            "heightStatusIcon",
            "",
            IconPaths.designCheckIcons["valid"] if is_height_valid else IconPaths.designCheckIcons["invalid"],
        )

        self.designCheckTable.addCommandInput(height_name_input, 0, 0)
        self.designCheckTable.addCommandInput(height_value_input, 0, 1)
        self.designCheckTable.addCommandInput(height_icon_input, 0, 2)

        # Row 2: Design Perimeter
        perimeter = self.fusion_design_perimeter
        is_perimeter_valid = perimeter <= self.MAX_PERIMETER

        perimeter_name_input = designCheckTabInputs.addTextBoxCommandInput(
            "designPerimeterText", "Design Perimeter", "Design Perimeter", 1, True
        )
        perimeter_value_input = designCheckTabInputs.addTextBoxCommandInput(
            "designPerimeterValue", "Value", f"{perimeter:.2f} cm", 1, True
        )
        perimeter_icon_input = designCheckTabInputs.addImageCommandInput(
            "perimeterStatusIcon",
            "",
            IconPaths.designCheckIcons["valid"] if is_perimeter_valid else IconPaths.designCheckIcons["invalid"],
        )

        self.designCheckTable.addCommandInput(perimeter_name_input, 1, 0)
        self.designCheckTable.addCommandInput(perimeter_value_input, 1, 1)
        self.designCheckTable.addCommandInput(perimeter_icon_input, 1, 2)

    @property
    def isVisible(self) -> bool:
        return self.designCheckTab.isVisible or False

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.designCheckTab.isVisible = value

    @property
    def isActive(self) -> bool:
        return self.designCheckTab.isActive or False

    @property
    def fusion_design_height(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.boundingBox
            return float(overall_bounding_box.maxPoint.z - overall_bounding_box.minPoint.z)
        return 0.0

    @property
    def fusion_design_perimeter(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.boundingBox
            width = overall_bounding_box.maxPoint.x - overall_bounding_box.minPoint.x
            length = overall_bounding_box.maxPoint.y - overall_bounding_box.minPoint.y
            return float(2 * (width + length))
        return 0.0
