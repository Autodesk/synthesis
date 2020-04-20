# Author: Autodesk
# Description: Robot and Mechanical Exporter for Synthesis.
# Note to self: Make the joint field repeated because occurrences will have multiple joints

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
        node.isDynamic = True
        node.nodeID = id
        node.parentNode = -1
        node.mass += float(phys.mass)# No idea if this is correct. Will test once we start dynamic objects

        # Everything is relative to the center of the editor
        node.position.x = 0
        node.position.y = 0
        node.position.z = 0

        bodies = occ.bRepBodies
        for body in bodies:
            # if (phys.volume > 20):# Might add to filter out some small objects
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

        # Give the nodes a default. I think you can do this in the protobuf but I don't know how
        # node.joint.type = JointType.NoJoint

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

        # Turns out a component and occurence share a ton of the same attributes
        self.field.nodes.append(self.GetNode(self.__rootComponent, 0))
        self.field.nodes[0].joints.append(JointInfo(type = JointType.NoJoint))
        for occ in self.__predefinedNodes:
            n = self.GetNode(occ, index)
            #n.joint.type = JointType.Fixed
            #n.joint.companionID = 0
            self.field.nodes.append(n)
            index = index + 1

        # Loads joint info
        loadedJoints = [('', '')]
        for x in range(len(self.__predefinedNodes)):
            if len(self.__predefinedNodes[x].joints) < 1:
                protoJoint = JointInfo()
                protoJoint.type = JointType.Fixed
                protoJoint.companionID = 0
                self.field.nodes[x + 1].joints.append(protoJoint)
            else:
                for joint in self.__predefinedNodes[x].joints:

                    item = (joint.occurrenceOne.name, joint.occurrenceTwo.name)
                    if item not in loadedJoints:
                        loadedJoints.append(item)

                        motion = joint.jointMotion

                        protoJoint = JointInfo()

                        jointGeometry = joint.geometryOrOriginOne
                        origin = jointGeometry.origin
                        self.__ui.messageBox('Xo:{}, Yo:{}, Zo:{}'.format(origin.x / 100, origin.z / 100, origin.y / 100))
                        
                        # Check for joint type
                        if type(motion) is adsk.fusion.RevoluteJointMotion:
                            protoJoint.type = JointType.Hinge
                            directionVector = motion.rotationAxisVector
                            protoJoint.direction.x = directionVector.x# ProtoVector3(x = , y = directionVector.z / 100, z = directionVector.y / 100)
                            protoJoint.direction.y = directionVector.z
                            protoJoint.direction.z = directionVector.y
                            protoJoint.origin.x = origin.x / 100# ProtoVector3(x = origin.x / 100, y = origin.z / 100, z = origin.y / 100)
                            protoJoint.origin.y = origin.z / 100
                            protoJoint.origin.z = origin.y / 100
                            if joint.occurrenceOne == self.__predefinedNodes[x]:
                                protoJoint.companionID = self.__predefinedNodes.index(joint.occurrenceTwo) + 1
                            else:
                                protoJoint.companionID = self.__predefinedNodes.index(joint.occurrenceOne) + 1
                            # self.__ui.messageBox('x:{}, y:{}, z:{}'.format(directionVector.x, directionVector.z, directionVector.y))
                        else:
                            protoJoint.type = JointType.Fixed
                            protoJoint.companionID = 0

                        self.field.nodes[x + 1].joints.append(protoJoint)

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
