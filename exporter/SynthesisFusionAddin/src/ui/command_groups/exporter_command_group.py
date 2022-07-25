from ..config_command import *


class ExporterCommandGroup(CommandGroup):

    def __init__(self, parent):
        super().__init__(parent)

    def configure(self):
        exporter_setings = self.parent.advanced_input.addGroupCommandInput(
            "exporter_settings", "Exporter Settings"
        )
        exporter_setings.isExpanded = True
        exporter_setings.isEnabled = True
        exporter_setings.tooltip = "tooltip"  # TODO: update tooltip
        exporter_settings = exporter_setings.children

        self.parent.create_boolean_input(  # algorithm wheel selection checkbox.
            "algorithmic_selection",
            "Algorithmic Wheel Selection",
            exporter_settings,
            checked=True,
            tooltip="Automatically select the entire wheel component.",
            tooltipadvanced="<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                            "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                            "<ul>" +
                            "<tt>" +
                            "<li>Jointing the wheel differently, or</li><br>" +
                            "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                            "<li>Disabling Algorithmic Selection.</li>" +
                            "</tt>" +
                            "</ul>",
            enabled=True,
        )

        self.parent.create_boolean_input(
            "compress",
            "Compress Output",
            exporter_settings,
            checked=compress,
            tooltip="Compress the output file for a smaller file size.",
            tooltipadvanced="<hr>Use the GZIP compression system to compress the resulting file which will be opened in the simulator, perfect if you want to share the file.<br>",
            enabled=True
        )

        self.parent.create_boolean_input(  # open synthesis checkbox
            "open_synthesis",
            "Open Synthesis",
            exporter_settings,
            checked=True,
            tooltip="Open Synthesis after the export is complete.",
            enabled=True,
        )
