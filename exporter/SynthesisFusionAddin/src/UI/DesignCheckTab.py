import adsk.core
import adsk.fusion

from src.Logging import getLogger, logFailure

logger = getLogger # TODO: Remove


class DesignCheckTab:
    designCheckTab: adsk.core.TabCommandInput

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        self.designCheckTab = args.command.commandInputs.addTabCommandInput("designCheckTab", "Design Check")
        designCheckTabInputs = self.designCheckTab.children

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
