
import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects, item_id
from ..proto.synthesis_importbuf_pb2 import *

ATTR_GROUP_NAME = "SynthesisFusionExporter" # attribute group name for use with apper's item_id

def fillComponents(ao, components):
    root = ao.design.rootComponent

    for childComponent in range(0, root.allOccurrences.count):
        # traverse through occurrences to get component
        occurrence = root.allOccurrences.item(childComponent)
        component = occurrence.component
        material = component.material

        # components.header.uuid = item_id(component, ATTR_GROUP_NAME)
        components = ao.design.allComponents
        components.partNumber = component.partNumber
        components.boundingBox = component.boundingBox
        # components.materialId = component.materialId
        #components.materialId = item_id(material.name, ATTR_GROUP_NAME)
        components.physicalProperties = component.physicalProperties
        components.meshbodies = component.meshBodies
        
        print(components)

def fillJoints(ao, joints):
    pass #todo

def fillMaterials(ao, materials):
    pass #todo

def fillAppearances(ao, appearances):
    pass #todo

def fillMatrix3D(transform, protoTransform):
    pass #todo

def fillOccurrence(occur, protoOccur):
    protoOccur.header.uuid = item_id(occur, ATTR_GROUP_NAME)
    protoOccur.header.name = occur.name
    protoOccur.isGrounded = occur.isGrounded
    fillMatrix3D(occur.transform, protoOccur.transform)

    protoOccur.componentUUID = item_id(occur.component, ATTR_GROUP_NAME)
    #todo fill componentBuf here?

    for childOccur in occur.childOccurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())

def fillFakeRootOccurrence(rootComponent, protoOccur):
    protoOccur.componentUUID = item_id(rootComponent, ATTR_GROUP_NAME)
    #todo fill componentBuf here?

    for childOccur in rootComponent.occurrences:
        fillOccurrence(childOccur, protoOccur.childOccurrences.add())

def fillDesign(ao, design):
    fillComponents(ao, design.components)
    fillJoints(ao, design.joints)
    fillMaterials(ao, design.materials)
    fillAppearances(ao, design.appearances)
    fillFakeRootOccurrence(ao.root_comp, design.hierarchyRoot)

def fillUserMeta(ao, userMeta):
    currentUser = ao.app.currentUser
    userMeta.userName = currentUser.userName
    userMeta.id = currentUser.userId
    userMeta.displayName = currentUser.displayName
    userMeta.email = currentUser.email

def fillDocumentMeta(ao, documentMeta):
    document = ao.document
    documentMeta.fusionVersion = document.version
    documentMeta.name = document.name
    documentMeta.versionNumber = document.dataFile.versionNumber
    documentMeta.description = document.dataFile.description
    documentMeta.id = document.dataFile.id

def fillDocument(ao, document):
    fillUserMeta(ao, document.userMeta)
    fillDocumentMeta(ao, document.documentMeta)
    fillDesign(ao, document.design)

def exportRobot():
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    document = Document()
    fillDocument(ao, document)

    print(document)


class ExportCommand(apper.Fusion360CommandBase):
    
    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()
