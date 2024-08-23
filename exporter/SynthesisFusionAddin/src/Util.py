import os

import adsk.core
import adsk.fusion

from src.Types import FUSION_UNIT_SYSTEM, KG, LBS, UnitSystem


def getFusionUnitSystem() -> UnitSystem:
    fusDesign = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
    return FUSION_UNIT_SYSTEM.get(fusDesign.fusionUnitsManager.distanceDisplayUnits, UnitSystem.METRIC)


def convertMassUnitsFrom(input: KG | LBS) -> KG | LBS:
    """Converts stored Synthesis mass units into user selected Fusion units."""
    unitManager = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct).fusionUnitsManager
    toString = "kg" if getFusionUnitSystem() is UnitSystem.METRIC else "lbmass"
    return unitManager.convert(input, "kg", toString) or 0.0


def convertMassUnitsTo(input: KG | LBS) -> KG | LBS:
    """Converts user selected Fusion mass units into Synthesis units."""
    unitManager = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct).fusionUnitsManager
    fromString = "kg" if getFusionUnitSystem() is UnitSystem.METRIC else "lbmass"
    return unitManager.convert(input, fromString, "kg") or 0.0


def designMassCalculation() -> KG | LBS:
    """Calculates and returns the total mass of the active design in Fusion units."""
    app = adsk.core.Application.get()
    mass = 0.0
    for body in [x for x in app.activeDocument.design.rootComponent.bRepBodies if x.isLightBulbOn]:
        physical = body.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
        mass += physical.mass

    for occ in [x for x in app.activeDocument.design.rootComponent.allOccurrences if x.isLightBulbOn]:
        for body in [x for x in occ.component.bRepBodies if x.isLightBulbOn]:
            physical = body.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
            mass += physical.mass

    # Internally, Fusion always uses metric units, same as Synthesis
    return round(convertMassUnitsFrom(mass), 2)


def makeDirectories(directory: str) -> str:
    """Ensures than an input directory exists and attempts to create it if it doesn't."""
    os.makedirs(directory, exist_ok=True)
    return directory
