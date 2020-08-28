import adsk
import adsk.core
import adsk.fusion
from typing import List

from gltf.extras.proto.gltf_extras_pb2 import PhysicalProperties

def combinePhysicalProperties(physicalPropertiesList: List[PhysicalProperties]):
    combined = PhysicalProperties()  # type: PhysicalProperties
    weightedSumX = 0
    weightedSumY = 0
    weightedSumZ = 0
    for physicalProperty in physicalPropertiesList:
        combined.mass += physicalProperty.mass
        combined.volume += physicalProperty.volume
        combined.area += physicalProperty.area
        weightedSumX += physicalProperty.centerOfMass.x * physicalProperty.mass
        weightedSumY += physicalProperty.centerOfMass.y * physicalProperty.mass
        weightedSumZ += physicalProperty.centerOfMass.z * physicalProperty.mass
    combined.centerOfMass.x = weightedSumX / combined.mass
    combined.centerOfMass.y = weightedSumY / combined.mass
    combined.centerOfMass.z = weightedSumZ / combined.mass
    return combined

def exportPhysicalProperties(physicalProperties: adsk.fusion.PhysicalProperties):
    protoPhysicalProperties = PhysicalProperties()  # type: PhysicalProperties
    protoPhysicalProperties.mass = physicalProperties.mass
    protoPhysicalProperties.area = physicalProperties.area/(100**2) # cm^2 -> m^2
    protoPhysicalProperties.volume = physicalProperties.volume/(100**3) # cm^3 -> m^3
    fillPoint3DConvertUnits(physicalProperties.centerOfMass, protoPhysicalProperties.centerOfMass)
    return protoPhysicalProperties

# TODO: merge duplicates
def fillVector3D(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x
    protoVector3D.y = fusionVector3D.y
    protoVector3D.z = fusionVector3D.z

def fillPoint3DConvertUnits(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x/100
    protoVector3D.y = fusionVector3D.y/100
    protoVector3D.z = fusionVector3D.z/100
