import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects
from ..proto.synthesis_importbuf_pb2 import *
from google.protobuf.json_format import MessageToDict, MessageToJson
from ..utils.DebugHierarchy import printHierarchy
import time

ATTR_GROUP_NAME = "SynthesisFusionExporter"  # attribute group name for use with apper's item_id


def exportRobot():
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    start = time.perf_counter()

    protoDocument = Document()
    fillDocument(ao, protoDocument)
    # protoDocumentAsDict = MessageToDict(protoDocument)

    filePath = '{0}{1}_{2}.{3}'.format('C:/temp/', protoDocument.documentMeta.name.replace(" ", "_"), protoDocument.documentMeta.exportTime, "synimport")

    file = open(filePath, 'wb')
    file.write(protoDocument.SerializeToString())
    file.close()

    end = time.perf_counter()
    finishedMessage = f"Export completed in {round(end - start, 1)} seconds\n" \
            f"File saved to {filePath}"
    print(finishedMessage)
    ao.ui.messageBox(finishedMessage)



class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()


# -----------Document-----------

def fillDocument(ao, protoDocument):
    fillUserMeta(ao.app.currentUser, protoDocument.userMeta)
    fillDocumentMeta(ao.document, protoDocument.documentMeta)
    fillDesign(ao.design, protoDocument.design)


def fillUserMeta(fusionCurrentUser, protoUserMeta):
    protoUserMeta.userName = fusionCurrentUser.userName
    protoUserMeta.id = fusionCurrentUser.userId
    protoUserMeta.displayName = fusionCurrentUser.displayName
    protoUserMeta.email = fusionCurrentUser.email


def fillDocumentMeta(fusionDocument, protoDocumentMeta):
    protoDocumentMeta.fusionVersion = fusionDocument.version
    protoDocumentMeta.name = fusionDocument.name
    protoDocumentMeta.versionNumber = fusionDocument.dataFile.versionNumber
    protoDocumentMeta.description = fusionDocument.dataFile.description
    protoDocumentMeta.id = fusionDocument.dataFile.id
    protoDocumentMeta.exportTime = int(time.time())

def fillDesign(fusionDesign, protoDesign):
    # start = time.perf_counter()

    fillComponents(fusionDesign.allComponents, protoDesign.components)

    # components = time.perf_counter()

    # fillJoints(fusionDesign.rootComponent.allJoints, protoDesign.joints)
    # fillMaterials(fusionDesign.materials, protoDesign.materials)
    # fillAppearances(fusionDesign.appearances, protoDesign.appearances)
    fillFakeRootOccurrence(fusionDesign.rootComponent, protoDesign.hierarchyRoot)

    # end = time.perf_counter()
    # ao = AppObjects()
    # ao.ui.messageBox(f"Components {components - start} seconds\n")
    # ao.ui.messageBox(f"Occurrences {end - components} seconds\n")


# -----------Occurrence Tree-----------

def fillFakeRootOccurrence(rootComponent, protoOccur):
    # protoOccur.header.uuid = item_id(rootComponent, ATTR_GROUP_NAME)
    protoOccur.header.name = rootComponent.name
    # protoOccur.componentUUID = item_id(rootComponent, ATTR_GROUP_NAME)
    protoOccur.componentUUID = rootComponent.revisionId

    for childOccur in rootComponent.occurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())


def fillOccurrence(occur, protoOccur):
    protoOccur.header.name = occur.name
    protoOccur.isGrounded = occur.isGrounded
    fillMatrix3D(occur.transform, protoOccur.transform)

    protoOccur.componentUUID = occur.component.revisionId

    for childOccur in occur.childOccurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())


# -----------Components-----------

def fillComponents(fusionComponents, protoComponents):
    for fusionComponent in fusionComponents:
        fillComponent(fusionComponent, protoComponents.add())


def fillComponent(fusionComponent, protoComponent):
    protoComponent.header.uuid = fusionComponent.revisionId
    protoComponent.header.name = fusionComponent.name
    protoComponent.header.description = fusionComponent.description
    protoComponent.header.revisionId = fusionComponent.revisionId
    protoComponent.partNumber = fusionComponent.partNumber
    # fillBoundingBox3D(fusionComponent.boundingBox, protoComponent.boundingBox)
    if fusionComponent.material is not None:
        protoComponent.materialId = fusionComponent.material.id
    # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

    for bRepBody in fusionComponent.bRepBodies:
        fillMeshBodyFromBrep(bRepBody, protoComponent.meshBodies.add())


def fillMeshBodyFromBrep(fusionBRepBody, protoMeshBody):
    protoMeshBody.header.name = fusionBRepBody.name
    if protoMeshBody.appearanceId is not None:
        protoMeshBody.appearanceId = fusionBRepBody.appearance.id
    if protoMeshBody.materialId is not None:
        protoMeshBody.materialId = fusionBRepBody.material.id
    # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)
    # fillBoundingBox3D(fusionBRepBody.boundingBox, protoMeshBody.boundingBox3D)
    fillTriangleMeshFromBrep(fusionBRepBody, protoMeshBody.triangleMesh)


def fillTriangleMeshFromBrep(fusionBRepBody, protoTriangleMesh):
    meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
    meshCalculator.setQuality(11)  # todo mesh quality settings
    mesh = meshCalculator.calculate()

    protoTriangleMesh.vertices.extend(mesh.nodeCoordinatesAsDouble)
    protoTriangleMesh.normals.extend(mesh.nodeCoordinatesAsDouble)
    protoTriangleMesh.indices.extend(mesh.nodeIndices)
    protoTriangleMesh.uvs.extend(mesh.nodeCoordinatesAsDouble)


def fillPhysicalProperties(fusionPhysical, protoPhysical):
    protoPhysical.density = fusionPhysical.density
    protoPhysical.mass = fusionPhysical.mass
    protoPhysical.volume = fusionPhysical.volume
    protoPhysical.area = fusionPhysical.area
    fillVector3D(fusionPhysical.centerOfMass, protoPhysical.centerOfMass)


# -----------Joints-----------

def fillJoints(fusionJoints, protoJoints):
    for fusionJoint in fusionJoints:
        if isJointInvalid(fusionJoint): continue
        fillJoint(fusionJoint, protoJoints.add())


def isJointInvalid(fusionJoint):
    if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
        print("WARNING: Ignoring joint with unknown occurrences!")  # todo: Show these messages to the user
        return True
    if fusionJoint.jointMotion.jointType not in range(6):
        print("WARNING: Ignoring joint with unknown type!")
        return True
    return False


def fillJoint(fusionJoint, protoJoint):
    # protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
    protoJoint.header.name = fusionJoint.name
    fillVector3D(getJointOrigin(fusionJoint), protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked
    protoJoint.isSuppressed = fusionJoint.isSuppressed

    # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)

    fillJointMotionFuncSwitcher = {
        0: fillRigidJointMotion,
        1: fillRevoluteJointMotion,
        2: fillSliderJointMotion,
        3: fillCylindricalJointMotion,
        4: fillPinSlotJointMotion,
        5: fillPlanarJointMotion,
        6: fillBallJointMotion,
    }

    fillJointMotionFunc = fillJointMotionFuncSwitcher.get(fusionJoint.jointMotion.jointType, lambda: None)
    fillJointMotionFunc(fusionJoint.jointMotion, protoJoint)


def getJointOrigin(fusionJoint):
    geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
    if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
        return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        return adsk.core.Point3D.create(  # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
            origin.x + geometryOrOrigin.offsetX.value,
            origin.y + geometryOrOrigin.offsetY.value,
            origin.z + geometryOrOrigin.offsetZ.value)


def getJointedOccurrenceUUID(fusionJoint, fusionOccur):
    # if fusionOccur is None:
    #     return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    # return item_id(fusionOccur, ATTR_GROUP_NAME)
    if fusionOccur is None:
        return ""  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    return fusionOccur.fullPathName


def fillRigidJointMotion(fusionJointMotion, protoJoint):
    protoJoint.rigidJointMotion.SetInParent()


def fillRevoluteJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.revoluteJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)


def fillSliderJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.sliderJointMotion

    fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)


def fillCylindricalJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.cylindricalJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)


def fillPinSlotJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.pinSlotJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)


def fillPlanarJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.planarJointMotion

    fillVector3D(fusionJointMotion.normalDirectionVector, protoJointMotion.normalDirectionVector)

    fillVector3D(fusionJointMotion.primarySlideDirectionVector, protoJointMotion.primarySlideDirectionVector)
    protoJointMotion.primarySlideValue = fusionJointMotion.primarySlideValue
    fillJointLimits(fusionJointMotion.primarySlideLimits, protoJointMotion.primarySlideLimits)

    fillVector3D(fusionJointMotion.secondarySlideDirectionVector, protoJointMotion.secondarySlideDirectionVector)
    protoJointMotion.secondarySlideValue = fusionJointMotion.secondarySlideValue
    fillJointLimits(fusionJointMotion.secondarySlideLimits, protoJointMotion.secondarySlideLimits)

    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)


def fillBallJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.ballJointMotion

    fillVector3D(fusionJointMotion.rollDirectionVector, protoJointMotion.rollDirectionVector)
    protoJointMotion.rollValue = fusionJointMotion.rollValue
    fillJointLimits(fusionJointMotion.rollLimits, protoJointMotion.rollLimits)

    fillVector3D(fusionJointMotion.pitchDirectionVector, protoJointMotion.pitchDirectionVector)
    protoJointMotion.pitchValue = fusionJointMotion.pitchValue
    fillJointLimits(fusionJointMotion.pitchLimits, protoJointMotion.pitchLimits)

    fillVector3D(fusionJointMotion.yawDirectionVector, protoJointMotion.yawDirectionVector)
    protoJointMotion.yawValue = fusionJointMotion.yawValue
    fillJointLimits(fusionJointMotion.yawLimits, protoJointMotion.yawLimits)


def fillJointLimits(fusionJointLimits, protoJointLimits):
    protoJointLimits.isMaximumValueEnabled = fusionJointLimits.isMaximumValueEnabled
    protoJointLimits.isMinimumValueEnabled = fusionJointLimits.isMinimumValueEnabled
    protoJointLimits.isRestValueEnabled = fusionJointLimits.isRestValueEnabled
    protoJointLimits.maximumValue = fusionJointLimits.maximumValue
    protoJointLimits.minimumValue = fusionJointLimits.minimumValue
    protoJointLimits.restValue = fusionJointLimits.restValue


# -----------Materials-----------

def fillMaterials(fusionMaterials, protoMaterials):
    for fusionMaterial in fusionMaterials:
        fillMaterial(fusionMaterial, protoMaterials.add())


def fillMaterial(childMaterial, protoMaterial):
    protoMaterial.id = childMaterial.id
    protoMaterial.name = childMaterial.name
    protoMaterial.appearanceId = childMaterial.appearance.id

    for prop in childMaterial.materialProperties:
        fillMaterialsProperties(prop, protoMaterial.properties)


def fillMaterialsProperties(fusionMaterials, protoMaterials):
    if (fusionMaterials.id == 'structural_Density') and (fusionMaterials.value is not None):
        protoMaterials.density = int(fusionMaterials.value)
    if (fusionMaterials.id == 'structural_Minimum_tensile_strength') and (fusionMaterials.value is not None):
        protoMaterials.yieldStrength = int(fusionMaterials.value)
    if (fusionMaterials.id == 'structural_Minimum_yield_stress') and (fusionMaterials.value is not None):
        protoMaterials.tensileStrength = int(fusionMaterials.value)


# -----------Appearances-----------

def fillAppearances(fusionAppearances, protoAppearances):
    for childAppearance in fusionAppearances:
        fillAppearance(childAppearance, protoAppearances.add())


def fillAppearance(fusionAppearance, protoAppearance):
    protoAppearance.id = fusionAppearance.id
    protoAppearance.name = fusionAppearance.name
    protoAppearance.hasTexture = fusionAppearance.hasTexture
    # todo add protobuf def: AppearanceProperties properties
    
    for prop in fusionAppearance.appearanceProperties:
        fillAppearanceProperties(prop, protoAppearance.properties)


def fillAppearanceProperties(fusionAppearanceProps, protoAppearanceProps):
    fillColor(fusionAppearanceProps, protoAppearanceProps.albedo)

    # todo
    # int32 glossiness
    # HighlightsMode highlights
    # int32 reflectivityDirect
    # int32 reflectivityOblique 
    # int32 transparency
    # int32 translucency
    # int32 refractiveIndex
    # Color selfIlluminationColor
    # int32 selfIlluminationLuminance
    # int32 selfIlluminationColorTemp

    # for debug to print appearanceProperties list
    # print("Name: "+fusionAppearanceProps.name+" Id: "+fusionAppearanceProps.id+" Object: "+fusionAppearanceProps.objectType)


# -----------Generic-----------

def fillColor(fusionColor, protoColor):
    if (fusionColor.name == 'Color') and (fusionColor.value is not None):
        protoColor.R = fusionColor.value.red
        protoColor.G = fusionColor.value.green
        protoColor.B = fusionColor.value.blue
        protoColor.A = fusionColor.value.opacity


def fillBoundingBox3D(fusionBoundingBox, protoBoundingBox):
    fillVector3D(fusionBoundingBox.maxPoint, protoBoundingBox.maxPoint)
    fillVector3D(fusionBoundingBox.minPoint, protoBoundingBox.minPoint)


def fillVector3D(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x
    protoVector3D.y = fusionVector3D.y
    protoVector3D.z = fusionVector3D.z


def fillMatrix3D(fusionTransform, protoTransform):
    assert len(protoTransform.cells) == 0  # Don't try to fill a matrix that's already full
    protoTransform.cells.extend(fusionTransform.asArray())
