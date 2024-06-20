# This is a module intended to be a formatter for the progress dialog message
# Mainly to make it more convienent to read and get messages from
# Need additional info on Joints if possible

import adsk.core


class PDMessage:
    def __init__(
        self,
        assemblyName: str,
        componentCount: int,
        occurrenceCount: int,
        materialCount: int,
        appearanceCount: int,
        progressDialog: adsk.core.ProgressDialog,
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

        self.finalValue = (
            self.componentCount
            + self.occurrenceCount
            + self.materialCount
            + self.appearanceCount
        )
        self.currentValue = 0

        self.progressDialog = progressDialog

    def _format(self):
        # USE FORMATTING TO CENTER THESE BAD BOIS
        # TABS DO NOTHING HALP
        out = f"{self.assemblyName} parsing:\n"
        out += f"\t Components: \t[ {self.currentCompCount} / {self.componentCount} ]\n"
        out += f"\t Occurrences: \t[ {self.currentOccCount} / {self.occurrenceCount} ]\n"
        out += f"\t Materials: \t[ {self.currentMatCount} / {self.materialCount} ]\n"
        out += f"\t Appearances: \t[ {self.currentAppCount} / {self.appearanceCount} ]\n"
        out += f"{self.currentMessage}"

        return out

    def addComponent(self, name=None):
        self.currentValue += 1
        self.currentCompCount += 1
        self.currentMessage = f"Exporting Component {name}"
        self.update()

    def addOccurrence(self, name=None):
        self.currentValue += 1
        self.currentOccCount += 1
        self.currentMessage = f"Exporting Occurrence {name}"
        self.update()

    def addMaterial(self, name=None):
        self.currentValue += 1
        self.currentMatCount += 1
        self.currentMessage = f"Exporting Physical Material {name}"
        self.update()

    def addAppearance(self, name=None):
        self.currentValue += 1
        self.currentAppCount += 1
        self.currentMessage = f"Exporting Appearance Material {name}"
        self.update()

    def addJoint(self, name=None):
        self.currentMessage = f"Connecting Joints {name}"
        self.update()

    def update(self):
        self.progressDialog.message = self._format()
        self.progressDialog.progressValue = self.currentValue
        self.value = self.currentValue

    def wasCancelled(self) -> bool:
        return self.progressDialog.wasCancelled

    def __str__(self):
        return self._format()

    def __repr__(self):
        return self._format()
