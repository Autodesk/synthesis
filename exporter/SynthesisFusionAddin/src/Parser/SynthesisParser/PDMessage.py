# This is a module intended to be a formatter for the progress dialog message
# Mainly to make it more convienent to read and get messages from
# Need additional info on Joints if possible


class PDMessage:
    def __init__(
        self,
        assemblyName: str,
        componentCount: int,
        occurrenceCount: int,
        materialCount: int,
        appearanceCount: int,
    ):

        self.assemblyName = assemblyName
        self.componentCount = componentCount
        self.occurrenceCount = occurrenceCount
        self.materialCount = materialCount
        self.appearanceCount = appearanceCount

        self.currentCompCount = 0
        self.currentOccCount = 0
        self.currentMatCount = 0
        self.currentAppCount = 0

        self.currentMessage = "working..."

    def _format(self):
        # USE FORMATTING TO CENTER THESE BAD BOIS
        out = f"{self.assemblyName} parsing: (PROTOTYPE TEST OUTPUT)\n"
        out += f" - Components: [ {self.currentCompCount} / {self.componentCount} ]\n"
        out += f" - Occurrences: [ {self.currentOccCount} / {self.occurrenceCount} ]\n"
        out += f" - Materials: [ {self.currentMatCount} / {self.materialCount} ]\n"
        out += f" - Appearances: [ {self.currentAppCount} / {self.appearanceCount} ]\n"
        out += f"{self.currentMessage}"

        return out

    def __str__(self):
        return self._format()

    def __repr__(self):
        return self._format()
