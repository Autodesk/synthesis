import adsk.core
import adsk.fusion

from src import gm, Logging

logger = Logging.getLogger()


class DesignCheckTab:
    designCheckTab: adsk.core.TabCommandInput

    @Logging.logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        self.designCheckTab = args.command.commandInputs.addTabCommandInput("designCheckTab", "Design Rule Check")
        designCheckTabInputs = self.designCheckTab.children

        # add a height thing (maximum 106 cm)
        self.designCheckTab.children.addTextBoxCommandInput(
            "designHeightText", "Design Height", f"{self.fusion_design_height:.2f} cm", 1, True
        )

        # get the robot perimeter
        self.designCheckTab.children.addTextBoxCommandInput(
            "designPerimeterText", "Design Perimeter", f"{self.fusion_design_perimeter:.2f} cm", 1, True
        )

        logger.info(f"{self.fusion_design_perimeter}")

    @property
    def isVisible(self) -> bool:
        return self.designCheckTab.isVisible or False

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.designCheckTab.isVisible = value

    @property
    def isActive(self) -> bool:
        return self.designCheckTab.isActive or False

    # @Logging.logFailure
    # def handleInputChanged(
    #     self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
    # ) -> None:
    #     commandInput = args.input

    @property
    def fusion_design_height(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return float(overall_bounding_box.height)
        return 0.0

    @property
    def fusion_design_perimeter(self) -> float:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return float(2 * (overall_bounding_box.width + overall_bounding_box.length))
        return 0.0
