import adsk.core
import adsk.fusion
from adsk.fusion import Design

from src import gm
from src.Logging import getLogger, logFailure

logger = getLogger() # TODO: Remove


class DesignCheckTab:
    designCheckTab: adsk.core.TabCommandInput

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        self.designCheckTab = args.command.commandInputs.addTabCommandInput("designCheckTab", "Design Check")
        designCheckTabInputs = self.designCheckTab.children

        # add a height thing (maximum 106 cm)
        logger.info(f"{self.fusion_design_height}")

        # get the robot perimeter
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

    @logFailure
    def handleInputChanged(
        self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
    ) -> None:
        commandInput = args.input

    @property
    def fusion_design_height(self) -> float:
        design = Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return overall_bounding_box.height
        return 0.0

    @property
    def fusion_design_perimeter(self) -> float:
        design = Design.cast(gm.app.activeProduct)
        if design:
            overall_bounding_box = design.rootComponent.orientedMinimumBoundingBox
            return 2 * (overall_bounding_box.width + overall_bounding_box.length)
        return 0.0
