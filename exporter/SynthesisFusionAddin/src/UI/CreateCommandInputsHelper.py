import adsk.core

from ..Logging import logFailure


@logFailure
def createTableInput(
    id: str,
    name: str,
    inputs: adsk.core.CommandInputs,
    columns: int,
    ratio: str,
    minRows: int = 1,
    maxRows: int = 50,
    columnSpacing: int = 0,
    rowSpacing: int = 0,
) -> adsk.core.TableCommandInput:
    input = inputs.addTableCommandInput(id, name, columns, ratio)
    input.minimumVisibleRows = minRows
    input.maximumVisibleRows = maxRows
    input.columnSpacing = columnSpacing
    input.rowSpacing = rowSpacing

    return input


@logFailure
def createBooleanInput(
    id: str,
    name: str,
    inputs: adsk.core.CommandInputs,
    tooltip: str = "",
    tooltipadvanced: str = "",
    checked: bool = True,
    enabled: bool = True,
    isCheckBox: bool = True,
) -> adsk.core.BoolValueCommandInput:
    input = inputs.addBoolValueInput(id, name, isCheckBox)
    input.value = checked
    input.isEnabled = enabled
    input.tooltip = tooltip
    input.tooltipDescription = tooltipadvanced

    return input


@logFailure
def createTextBoxInput(
    id: str,
    name: str,
    inputs: adsk.core.CommandInputs,
    text: str,
    italics: bool = True,
    bold: bool = True,
    fontSize: int = 10,
    alignment: str = "center",
    rowCount: int = 1,
    read: bool = True,
    background: str = "whitesmoke",
    tooltip: str = "",
    advanced_tooltip: str = "",
) -> adsk.core.TextBoxCommandInput:
    if bold:
        text = f"<b>{text}</b>"

    if italics:
        text = f"<i>{text}</i>"

    outputText = f"""<body style='background-color:{background};'>
        <div align='{alignment}'>
        <p style='font-size:{fontSize}px'>
        {text}
        </p>
        </body>
    """

    input = inputs.addTextBoxCommandInput(id, name, outputText, rowCount, read)
    input.tooltip = tooltip
    input.tooltipDescription = advanced_tooltip

    return input
