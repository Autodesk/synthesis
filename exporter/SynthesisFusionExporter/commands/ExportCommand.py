
import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects, item_id
from ..proto.synthesis_importbuf_pb2 import *
from google.protobuf.json_format import MessageToDict, MessageToJson
from ..utils.DebugHierarchy import printHierarchy

ATTR_GROUP_NAME = "SynthesisFusionExporter" # attribute group name for use with apper's item_id

def fillComponents(ao, components):
    for component in ao.design.allComponents:
        fillComponent(component, components.add())

def fillComponent(childComponent, component):
        component.header.uuid = item_id(childComponent, ATTR_GROUP_NAME)
        component.header.name = childComponent.name
        component.header.description = childComponent.description
        component.header.revisionId = childComponent.revisionId
        component.partNumber = childComponent.partNumber
        fillBoundingBoxes(childComponent.boundingBox, component.boundingBox)
        component.materialId = childComponent.material.id
        fillPhysicalProperties(childComponent.physicalProperties, component.physicalProperties)
        
        # ADD: fillMeshBodies ---> see method
        # for childMesh in childComponent.meshBodies:
        #     fillMeshBodies(childMesh, component.meshBodies.add())

def fillBoundingBoxes(fusionBoundingBox, protoBoundingBox):
    fillVector3D(fusionBoundingBox.maxPoint, protoBoundingBox.maxPoint)
    fillVector3D(fusionBoundingBox.minPoint, protoBoundingBox.minPoint)

def fillVector3D(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x
    protoVector3D.y = fusionVector3D.y
    protoVector3D.z = fusionVector3D.z

def fillPhysicalProperties(fusionPhysical, protoPhysical):
    protoPhysical.density = fusionPhysical.density
    protoPhysical.mass = fusionPhysical.mass
    protoPhysical.volume = fusionPhysical.volume
    protoPhysical.area = fusionPhysical.area
    fillVector3D(fusionPhysical.centerOfMass, protoPhysical.centerOfMass)

def fillMeshBodies(fusionMesh, protoMesh):
    protoMesh.header.uuid = item_id(fusionMesh, ATTR_GROUP_NAME)
    protoMesh.header.name = fusionMesh.name
    protoMesh.appearanceId = fusionMesh.appearance.id
    protoMesh.materialId = fusionMesh.material.id
    fillPhysicalProperties(fusionMesh.physicalProperties, protoMesh.physicalProperties)
    fillBoundingBoxes(fusionMesh.boundingBox, protoMesh.boundingBox)
    # ADD: triangleMesh

def getJointedOccurrenceUUID(fusionJoint, occur):
    if occur is None: 
        return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME) # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    return item_id(occur, ATTR_GROUP_NAME)

def getJointOrigin(fusionJoint):
    geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
    if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
        return geometryOrOrigin.origin
    else: # adsk::fusion::JointOrigin 
        origin = geometryOrOrigin.geometry.origin
        return Point3D.create( #todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
            origin.x + geometryOrOrigin.offsetX.value,
            origin.y + geometryOrOrigin.offsetY.value,
            origin.z + geometryOrOrigin.offsetZ.value)

def fillJoint(fusionJoint, protoJoint):
    protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
    protoJoint.header.name = fusionJoint.name
    fillVector3D(getJointOrigin(fusionJoint), protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked 
    protoJoint.isSuppressed = fusionJoint.isSuppressed 

    # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)

    #todo: fillJointMotion

def isJointCorrupted(fusionJoint):
    if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
        print("WARNING: Ignoring corrupted joint!")
        return True
    return False

def fillJoints(ao, protoJoints):
    for fusionJoint in ao.root_comp.allJoints:
        if isJointCorrupted(fusionJoint): continue
        fillJoint(fusionJoint, protoJoints.add())

def fillMaterials(ao, materials):
    for childMaterial in ao.design.materials:
        fillMaterial(childMaterial, materials.add())

def fillMaterial(childMaterial, materials):
    materials.id = childMaterial.id
    materials.name = childMaterial.name
    materials.appearanceId = childMaterial.appearance.id
    # add protobuf def: MaterialProperties properties 
    # fillMaterialsProperties()

def fillMaterialsProperties(fusionMaterials, protoMaterials):
    protoMaterials.density = fusionMaterials.density
    protoMaterials.yieldStrength = fusionMaterials.yieldStrength
    protoMaterials.tensileStrength = fusionMaterials.tensileStrength

def fillAppearances(ao, appearances):
    for childAppearance in ao.design.appearances:
        fillAppearance(childAppearance, appearances.add())

def fillAppearance(childAppearance, appearances):
    appearances.id = childAppearance.id
    appearances.name = childAppearance.name
    appearances.hasTexture = childAppearance.hasTexture
    # add protobuf def: AppearanceProperties properties

def fillMatrix3D(transform, protoTransform):
    protoTransform.cells.extend(transform.asArray())

def fillOccurrence(occur, protoOccur):
    protoOccur.header.uuid = item_id(occur, ATTR_GROUP_NAME)
    protoOccur.header.name = occur.name
    protoOccur.isGrounded = occur.isGrounded
    fillMatrix3D(occur.transform, protoOccur.transform)

    protoOccur.componentUUID = item_id(occur.component, ATTR_GROUP_NAME)

    for childOccur in occur.childOccurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())

def fillFakeRootOccurrence(rootComponent, protoOccur):
    protoOccur.header.uuid = item_id(rootComponent, ATTR_GROUP_NAME)
    protoOccur.header.name = rootComponent.name
    protoOccur.componentUUID = item_id(rootComponent, ATTR_GROUP_NAME)

    for childOccur in rootComponent.occurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())

def fillDesign(ao, protoDesign):
    fillComponents(ao, protoDesign.components)
    fillJoints(ao, protoDesign.joints)
    fillMaterials(ao, protoDesign.materials)
    fillAppearances(ao, protoDesign.appearances)
    fillFakeRootOccurrence(ao.root_comp, protoDesign.hierarchyRoot)

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

def fillDocument(ao, protoDocument):
    fillUserMeta(ao, protoDocument.userMeta)
    fillDocumentMeta(ao, protoDocument.documentMeta)
    fillDesign(ao, protoDocument.design)

def exportRobot():
    ao = AppObjects()

    # if ao.document.dataFile is None:
    #     print("Error: You must save your fusion document before exporting!")
    #     return

    protoDocument = Document()
    fillDocument(ao, protoDocument)
    protoDocumentAsDict = MessageToDict(protoDocument)
    # printHierarchy(ao.root_comp)
    print() # put breakpoint here


class ExportCommand(apper.Fusion360CommandBase):
    
    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()
