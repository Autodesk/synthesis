# Author: Autodesk
# Description: Robot and Mechanical Exporter for Synthesis.

import adsk.core, adsk.fusion, adsk.cam, traceback
import os, sys

my_addin_path = os.path.dirname(os.path.realpath(__file__))

try:

    # Adds the current addin directory to the path
    if not my_addin_path in sys.path:
        sys.path.append(my_addin_path)

    # Adds the required packages to work
    required_packages = my_addin_path + '\\site-packages'
    if not required_packages in sys.path:
        sys.path.append(required_packages)

    # Import the generated protobuf objects
    from protocols.ProtoRobot_pb2 import *

except:
    file = open(my_addin_path + '\\ErrorLog.txt', 'w')
    file.write(traceback.format_exc())
    file.flush()
    file.close()

# Class for exporting fields
class FieldData:

    def __init__(self, rootComponent, name, ui):
        self.field = ProtoField()
        self.field.FieldName = name
        self.__rootComponent = rootComponent
        self.__ui = ui
        self.__predefinedNodes = []

    def GetProtoBody(self, node, brep):
        obj = ProtoObject()

        # All Vertices are relative to the center of the editor
        obj.position.x = 0.0
        obj.position.y = 0.0
        obj.position.z = 0.0
        calculator = brep.meshManager.createMeshCalculator()
        calculator.setQuality(11)
        mesh = calculator.calculate()

        # Verts (also placeholder UV data)
        for point in mesh.nodeCoordinates:
            obj.verts.append(ProtoVector3(x = point.x / 100, y = point.z / 100, z = point.y / 100))
            obj.uv.append(ProtoVector2(x = 0, y = 1))
        
        # Triangles
        for i in mesh.nodeIndices:
            obj.tris.append(i)
        
        node.bodies.append(obj)

    def GetNode(self, occ, id):
        node = ProtoNode()
        node.mass = 0.0

        phys = occ.getPhysicalProperties(1)

        # Load meta data
        node.name = occ.name###
        node.isDynamic = False
        node.nodeID = id
        node.parentNode = -1
        node.mass += float(phys.mass)# No idea if this is correct. Will test once we start dynamic objects

        # Everything is relative to the center of the editor
        node.position.x = 0
        node.position.y = 0
        node.position.z = 0

        bodies = occ.bRepBodies
        for body in bodies:
            #if (phys.volume > 20):# Might add to filter out some small objects
            self.GetProtoBody(node, body)

        adsk.doEvents()# Gives process back to fusion for an update. Prevents crashes

        occs = None

        if type(occ) is adsk.fusion.Component:
            occs = occ.occurrences
        else:
            occs = occ.childOccurrences

        for child in occs:
            if (child.name[:5] != 'NODE_'):
                n = self.GetNode(child, 0)
                for body in n.bodies:
                    node.bodies.append(body)
                node.mass += n.mass

        return node

    def GetAllOccurrencesFromRoot(self, comp):
        occurrenceAccum = []
        occurrencesToSearch = []
        for x in comp.occurrences:
            occurrencesToSearch.append(x)

        while len(occurrencesToSearch) > 0:
            searchListCopy = occurrencesToSearch.copy()
            occurrencesToSearch.clear()
            for occ in searchListCopy:
                occurrenceAccum.append(occ)
                for child in occ.childOccurrences:
                    occurrencesToSearch.append(child)

        return occurrenceAccum

    def LoadField(self):
        index = 1
        occs = self.GetAllOccurrencesFromRoot(self.__rootComponent)

        self.__ui.messageBox('Gathered all Occurrences')

        for occ in occs:
            if (occ.name[:5] == 'NODE_'):
                self.__predefinedNodes.append(occ)

        self.__ui.messageBox('Identified all Nodes')

        self.field.nodes.append(self.GetNode(self.__rootComponent, 0))
        for occ in self.__predefinedNodes:
            self.field.nodes.append(self.GetNode(occ, index))
            index = index + 1

    def SaveField(self, path):
        file = open(path, 'wb')
        file.write(self.field.SerializeToString())
        file.flush()
        file.close()

def run(context):
    ui = None
    try:
        app = adsk.core.Application.get()
        ui  = app.userInterface
        design = app.activeProduct
        ui.messageBox('Starting Synthesis Exporter')

        comps = app.activeProduct.allComponents

        # Save a field to a file
        fieldData = FieldData(design.activeComponent, 'Test_Field', ui)
        fieldData.LoadField()
        fieldData.SaveField(my_addin_path + "\\output\\" + fieldData.field.FieldName + '.syn')

        ui.messageBox('Exported Field')

    except:
        if ui:
            ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

def stop(context):
    ui = None
    try:
        app = adsk.core.Application.get()
        ui  = app.userInterface
        ui.messageBox('Stopping Synthesis Exporter')

    except:
        if ui:
            ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
