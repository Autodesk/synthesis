
import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects, item_id
from ..proto.synthesis_importbuf_pb2 import *

ATTR_GROUP_NAME = "SynthesisFusionExporter" # attribute group name for use with apper's item_id

def fillComponents(ao, components):
    pass #todo

def getJointedOccurrenceUUID(fusionJoint, occur):
    if occur is None: 
        return item_id(fusionJoint, ATTR_GROUP_NAME)
    return item_id(occur, ATTR_GROUP_NAME)

def getJointOrigin(fusionJoint):
    


def fillJoint(fusionJoint, protoJoint):
    protoJoint.header.uuid = item_id(occur, ATTR_GROUP_NAME)
    protoJoint.header.name = fusionJoint.name
    fillVector3D(fusionJoint, protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked 
    protoJoint.isSuppressed = fusionJoint.isSuppressed 

    # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)

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
    protoOccur.header.uuid = item_id(rootComponent, ATTR_GROUP_NAME)
    protoOccur.header.name = rootComponent.name
    protoOccur.componentUUID = item_id(rootComponent, ATTR_GROUP_NAME)
    #todo fill componentBuf here?

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

def printTabs(num):
    for i in range(num):
        print("    ", end ="")

def printOccurrence(occur, depth):
    printTabs(depth)
    print("[O] "+occur.name)
    printComponent(occur.component, depth)

def printComponent(comp, depth):
    printTabs(depth)
    print("[C] "+comp.name)
    if comp.joints.count > 0:
        printTabs(depth)
        print("Joints")
        for joint in comp.joints:
            printTabs(depth)
            print("| "+joint.name)
    for occur in comp.occurrences:
        printOccurrence(occur, depth+1)

def exportRobot():
    ao = AppObjects()

    # if ao.document.dataFile is None:
    #     print("Error: You must save your fusion document before exporting!")
    #     return

    protoDocument = Document()
    # fillDocument(ao, document)
    print("-------------------------------")
    printComponent(ao.root_comp, 0)
    print("")


class ExportCommand(apper.Fusion360CommandBase):
    
    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()
