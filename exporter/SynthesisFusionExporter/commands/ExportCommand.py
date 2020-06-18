
import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects
from ..proto.synthesis_importbuf_pb2 import *

def fillDesign(ao, design):
    pass

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



        #
        #
        #
        # app = adsk.core.Application.get()
        # ui = app.userInterface
        #
        # title = 'Synthesis Exporter'
        #
        # design = app.activeDocument.design
        # uuid = apper.Fusion360Utilities.get_a_uuid()
        # user = app.currentUser.displayName
        #
        # currentDesignData = 'Current design data of: ' + design.parentDocument.name + '\n' + 'GUID: ' + uuid + '\n' + 'User: ' + user + '\n'
        # ui.messageBox(currentDesignData)
        #
        # ui = None
        # try:
        #     app = adsk.core.Application.get()
        #     ui = app.userInterface
        #     product = app.activeProduct
        #
        #     design = adsk.fusion.Design.cast(product)
        #     if not design:
        #         ui.messageBox('No active Fusion design', 'No Design')
        #         return
        #
        #     # get root
        #     rootComp = design.rootComponent
        #
        #     # traverse assembly recursively + print in message box
        #     resultString = 'Assembly structure of ' + design.parentDocument.name + '\n'
        #     resultString = getComponents(rootComp.occurrences.asList, 1, resultString)
        #     ui.messageBox(resultString)
        # except:
        #     if ui:
        #         ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
        #

# def getComponents(occurrences, level, input):
#     for i in range(0, occurrences.count):
#         occurence = occurrences.item(i)

#         input += 'Name: ' + occurence.name + '\n'

#         if occurence.childOccurrences:
#             input = getComponents(occurence.childOccurrences, level + 1, input)

#     return input
        