import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects, item_id
from ..proto.synthesis_importbuf_pb2 import *
from google.protobuf.json_format import MessageToDict, MessageToJson
from ..utils.DebugHierarchy import printHierarchy

ATTR_GROUP_NAME = "SynthesisFusionExporter"  # attribute group name for use with apper's item_id


def exportRobot():
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    protoDocument = Document()
    fillDocument(ao, protoDocument)
    protoDocumentAsDict = MessageToDict(protoDocument)
    # printHierarchy(ao.root_comp)
    print()  # put breakpoint here and view the protoDocumentAsDict local variable


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()


# -----------Document-----------

def fillDocument(ao, protoDocument):
    fillUserMeta(ao, protoDocument.userMeta)
    fillDocumentMeta(ao, protoDocument.documentMeta)
    fillDesign(ao, protoDocument.design)


def fillUserMeta(ao, protoUserMeta):
    currentUser = ao.app.currentUser
    protoUserMeta.userName = currentUser.userName
    protoUserMeta.id = currentUser.userId
    protoUserMeta.displayName = currentUser.displayName
    protoUserMeta.email = currentUser.email


def fillDocumentMeta(ao, protoDocumentMeta):
    document = ao.document
    protoDocumentMeta.fusionVersion = document.version
    protoDocumentMeta.name = document.name
    protoDocumentMeta.versionNumber = document.dataFile.versionNumber
    protoDocumentMeta.description = document.dataFile.description
    protoDocumentMeta.id = document.dataFile.id


def fillDesign(ao, protoDesign):
    fillComponents(ao, protoDesign.components)
    fillJoints(ao, protoDesign.joints)
    fillMaterials(ao, protoDesign.materials)
    fillAppearances(ao, protoDesign.appearances)
    fillFakeRootOccurrence(ao.root_comp, protoDesign.hierarchyRoot)


# -----------Occurrence Tree-----------

def fillFakeRootOccurrence(rootComponent, protoOccur):
    protoOccur.header.uuid = item_id(rootComponent, ATTR_GROUP_NAME)
    protoOccur.header.name = rootComponent.name
    protoOccur.componentUUID = item_id(rootComponent, ATTR_GROUP_NAME)

    for childOccur in rootComponent.occurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())


def fillOccurrence(occur, protoOccur):
    protoOccur.header.uuid = item_id(occur, ATTR_GROUP_NAME)
    protoOccur.header.name = occur.name
    protoOccur.isGrounded = occur.isGrounded
    fillMatrix3D(occur.transform, protoOccur.transform)

    protoOccur.componentUUID = item_id(occur.component, ATTR_GROUP_NAME)

    for childOccur in occur.childOccurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())


# -----------Components-----------

def fillComponents(ao, protoComponents):
    for fusionComponent in ao.design.allComponents:
        fillComponent(ao, fusionComponent, protoComponents.add())


def fillComponent(ao, fusionComponent, protoComponent):
    protoComponent.header.uuid = item_id(fusionComponent, ATTR_GROUP_NAME)
    protoComponent.header.name = fusionComponent.name
    protoComponent.header.description = fusionComponent.description
    protoComponent.header.revisionId = fusionComponent.revisionId
    protoComponent.partNumber = fusionComponent.partNumber
    fillBoundingBox3D(fusionComponent.boundingBox, protoComponent.boundingBox)
    protoComponent.materialId = fusionComponent.material.id
    fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

    for brepBody in fusionComponent.bRepBodies:
        fillMeshBodyFromBrep(brepBody, protoComponent.meshBodies.add())


def fillMeshBodyFromBrep(fusionBrepBody, protoMeshBody):
    protoMeshBody.header.uuid = item_id(fusionBrepBody, ATTR_GROUP_NAME)
    protoMeshBody.header.name = fusionBrepBody.name
    protoMeshBody.appearanceId = fusionBrepBody.appearance.id
    protoMeshBody.materialId = fusionBrepBody.material.id
    fillPhysicalProperties(fusionBrepBody.physicalProperties, protoMeshBody.physicalProperties)
    fillBoundingBox3D(fusionBrepBody.boundingBox, protoMeshBody.boundingBox3D)
    fillTriangleMesh(fusionBrepBody, protoMeshBody.triangleMesh)

def fillTriangleMesh(fusionTriangleMesh, protoTriangleMesh):
    # calculate triangle mesh
    meshManager = fusionTriangleMesh.meshManager
    calculator = meshManager.createMeshCalculator()
    mesh = calculator.calculate()

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

def fillJoints(ao, protoJoints):
    for fusionJoint in ao.root_comp.allJoints:
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
    protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
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
    if fusionOccur is None:
        return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    return item_id(fusionOccur, ATTR_GROUP_NAME)


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

def fillMaterials(ao, protoMaterials):
    for childMaterial in ao.design.materials:
        fillMaterial(childMaterial, protoMaterials.add())


def fillMaterial(childMaterial, protoMaterial):
    protoMaterial.id = childMaterial.id
    protoMaterial.name = childMaterial.name
    protoMaterial.appearanceId = childMaterial.appearance.id
    # todo add protobuf def: MaterialProperties properties
    # fillMaterialsProperties()

    for x in childMaterial.materialProperties:

        print("Name: "+x.name+" Id: "+x.id+" Value: "+str(x.value))
        fillMaterialsProperties(x, protoMaterial.properties)


def fillMaterialsProperties(fusionMaterials, protoMaterials):
    #print("Name: "+fusionMaterials.name+" Id: "+fusionMaterials.id+" Value: "+str(fusionMaterials.value))
    if (fusionMaterials.id == 'structural_Density') and (fusionMaterials.value is not None):
        protoMaterials.density = int(fusionMaterials.value)
    if (fusionMaterials.id == 'structural_Minimum_tensile_strength') and (fusionMaterials.value is not None):
        protoMaterials.yieldStrength = int(fusionMaterials.value)
    if (fusionMaterials.id == 'structural_Minimum_yield_stress') and (fusionMaterials.value is not None):
        protoMaterials.tensileStrength = int(fusionMaterials.value)
    #protoMaterials.density = fusionMaterials.id
    #protoMaterials.density = fusionMaterials.itemById(structural_Density.value)
    # protoMaterials.density = fusionMaterials.density
    # protoMaterials.yieldStrength = fusionMaterials.yieldStrength
    # protoMaterials.tensileStrength = fusionMaterials.tensileStrength


# -----------Appearances-----------

def fillAppearances(ao, protoAppearances):
    for childAppearance in ao.design.appearances:
        fillAppearance(childAppearance, protoAppearances.add())


def fillAppearance(fusionAppearance, protoAppearance):
    protoAppearance.id = fusionAppearance.id
    protoAppearance.name = fusionAppearance.name
    protoAppearance.hasTexture = fusionAppearance.hasTexture
    # todo add protobuf def: AppearanceProperties properties
    
    # for x in fusionAppearance.appearanceProperties:
    #     if x.value is not None:
    #         print("Appearance\n")
    #         print("Name: "+x.name)
            # print("Name: "+x.name+" Id: "+x.id+" Value: "+str(x.value))
        # [print("Name: "+x.name+" Id: "+x.id+" Value: "+str(x.value)) for x in ao.root_comp.material.materialProperties]


def fillAppearanceProperties(fusionAppearanceProps, protoAppearanceProps):
    pass  # todo


# -----------Generic-----------

def fillColor(fusionColor, protoColor):
    pass  # todo


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
