from ...general_imports import *
import adsk.core, adsk.fusion, traceback, logging, enum
from typing import *
from .. import parse_options
from .pd_message import PDMessage
from proto.proto_out import types_pb2, joint_pb2


# ____________________________ DATA TYPES __________________

# this is more of a tree - todo rewrite
class GraphNode:
    def __init__(self, data: any):
        self.data = data
        self.previous = None
        self.edges = list()

    def iter(self, filter_relationship=[]):
        """Generator for Node Iterator that does not have the given relationship

        Args:
            filter_relationship (list): list of all unwanted relationships

        Yields:
            GraphNode: node instance
        """
        yield self
        for edge in self.edges:
            if edge.relationship not in filter_relationship:
                yield from edge.node.iter(filter_relationship=filter_relationship)

    def __iter__(self):
        for edge in self.edges:
            yield edge.node

    def allChildren(self):
        nodes = [self]
        for edge in self.edges:
            nodes.extend(edge.node.allNodes())
        return nodes


class GraphEdge:
    def __init__(self, relationship: enum.Enum, node: GraphNode):
        """A GraphEdge representing a edge in the GraphNode

        Args:
            relationship (enum.Enum): Relationship that can be used as a filter
            node (GraphNode): Node to be linked
        """
        self.relationship = relationship
        self.node = node

    def print(self):
        print(f"Edge Containing {self.relationship.name} -> {self.node}")

    def __iter__(self):
        """Iterator for Edges within this edge

        Yields:
            GraphEdge: Edges in the node connected
        """
        return (edge for edge in self.node.edges)


class OccurrenceRelationship(enum.Enum):
    TRANSFORM = 1  # As in hierarchy parenting
    CONNECTION = 2  # As in a rigid joint or other designator
    GROUP = 3  # As in a Rigid Grouping
    NEXT = 4  # As in next_joint in list
    END = 5  # Orphaned child relationship


class JointRelationship(enum.Enum):
    GROUND = 1  # This currently has no bearing
    ROTATIONAL = 2  # This currently has no bearing


# ______________________ INDIVIDUAL JOINT CHAINS ____________________________


class DynamicOccurrenceNode(GraphNode):
    def __init__(self, occurrence: adsk.fusion.Occurrence, isGround=False, previous=None):
        super().__init__(occurrence)
        self.isGround = isGround
        self.name = occurrence.name

    def print(self):
        print(f"\n\t-------{self.data.name}-------")
        for edge in self.edges:
            edge.print()

    def getConnectedAxis(self) -> list:
        """Gets all Axis with the NEXT relationship

        Returns:
            list: list of Occurrences
        """
        nextItems = list()
        for edge in self.edges:
            if edge.relationship == OccurrenceRelationship.NEXT:
                nextItems.append(edge.node.data)
            else:
                nextItems.extend(edge.node.getConnectedAxis())
        return nextItems

    def getConnectedAxisTokens(self) -> list:
        """Gets all Axis with the NEXT relationship

        Returns:
            list: list of Occurrences
        """
        nextItems = list()
        for edge in self.edges:
            if edge.relationship == OccurrenceRelationship.NEXT:
                nextItems.append(edge.node.data.entityToken)
            else:
                nextItems.extend(edge.node.getConnectedAxisTokens())
        return nextItems


class DynamicEdge(GraphEdge):
    def __init__(self, relationship: OccurrenceRelationship, node: DynamicOccurrenceNode):
        super().__init__(relationship, node)

    # should print all in this class
    def print(self):
        print(f"\t\t - {self.relationship.name} -> {self.node.data.name}")
        self.node.print()


# ______________________ ENTIRE SIMULATION STRUCTURE _______________________


class SimulationNode(GraphNode):
    def __init__(
        self,
        dynamicJoint: DynamicOccurrenceNode,
        joint: adsk.fusion.Joint,
        grounded=False,
    ):
        super().__init__(dynamicJoint)
        self.joint = joint
        self.grounded = grounded

        if self.grounded:
            self.name = "Grounded"
        else:
            self.name = self.joint.name

    def print(self):
        print(f"Simulation Node for joint : {self.name} ")

    def printLink(self):
        if self.grounded:
            print(f"GROUND -- {self.data.data.name}")
        else:
            print(f"--> {self.data.data.name}")

        for edge in self.edges:
            edge.node.printLink()


class SimulationEdge(GraphEdge):
    def __init__(self, relationship: JointRelationship, node: SimulationNode):
        super().__init__(relationship, node)


# ______________________________ PARSER ___________________________________


class JointParser:
    def __init__(self, design):
        # Create hierarchy with just joint assembly
        # - Assembly
        #   - Grounded
        #   - Axis 1
        #   - Axis 2
        #     - Axis 3

        # 1. Find all Dynamic joint items to isolate                        [o]
        # 2. Find the grounded component                                    [x] (possible - not optimized)
        # 3. Populate tree with all items from each set of joints           [x] (done with grounding)
        # - 3. a) Each Child element with no joints                         [x]
        # - 3. b) Each Rigid Joint Connection                               [x]
        # 4. Link Joint trees by discovery from root                        [x]
        # 5. Record which trees have no children for creating end effectors [x] (next up) - this kinda already exists

        # Need to investigate creating an additional button for end effector possibly
        # It might be possible to have multiple end effectors
        # Total Number of final elements

        self.current = None
        self.previousJoint = None

        self.design = design

        # this can be dynamically assigned if we want
        self.grounded = searchForGrounded(design.rootComponent)

        if self.grounded is None:
            gm.ui.messageBox(
                "There is not currently a Grounded Component in the assembly, stopping kinematic export."
            )
            raise RuntimeWarning("There is no grounded component")
            return

        self.logger = logging.getLogger("{INTERNAL_ID}.parser.Joints.parser")

        self.currentTraversal = dict()
        self.groundedConnections = []

        # populate the rigidJoints connected to a given occurrence
        self.rigidJoints = dict()
        # populate all joints
        self.dynamicJoints = dict()

        self.simulationNodesRef = dict()

        # TODO: need to look through every single joint and find the starting point that is connected to ground
        # Next add that occurrence to the graph and then traverse down that path etc
        self.__getAllJoints()

        # dynamic joint node for grounded components and static components
        rootNode = self._populateNode(self.grounded, None, None, is_ground=True)
        self.groundSimNode = SimulationNode(rootNode, None, grounded=True)

        self.simulationNodesRef["GROUND"] = self.groundSimNode

        # combine all ground prior to this possibly
        self._lookForGroundedJoints()

        # creates the axis elements - adds all elements to axisNodes
        for key, value in self.dynamicJoints.items():
            self._populateAxis(key, value)

        self._linkAllAxis()

        # self.groundSimNode.printLink()

    def __getAllJoints(self):
        for joint in list(self.design.rootComponent.allJoints) + list(
            self.design.rootComponent.allAsBuiltJoints
        ):
            try:
                if joint and joint.occurrenceOne and joint.occurrenceTwo:
                    occurrenceOne = joint.occurrenceOne
                    occurrenceTwo = joint.occurrenceTwo
                else:
                    return None

                if occurrenceOne is None:
                    try:
                        occurrenceOne = (
                            joint.geometryOrOriginOne.entityOne.assemblyContext
                        )
                    except:
                        pass

                if occurrenceTwo is None:
                    try:
                        occurrenceTwo = (
                            joint.geometryOrOriginTwo.entityOne.assemblyContext
                        )
                    except:
                        pass

                oneEntityToken = ""
                twoEntityToken = ""

                try:
                    oneEntityToken = occurrenceOne.entityToken
                except:
                    oneEntityToken = occurrenceOne.name

                try:
                    twoEntityToken = occurrenceTwo.entityToken
                except:
                    twoEntityToken = occurrenceTwo.name

                typeJoint = joint.jointMotion.jointType

                if typeJoint != 0:
                    if oneEntityToken not in self.dynamicJoints.keys():
                        self.dynamicJoints[oneEntityToken] = joint

                    if occurrenceTwo is None and occurrenceOne is None:
                        self.logger.error(
                            f"Occurrences that connect joints could not be found\n\t1: {occurrenceOne}\n\t2: {occurrenceTwo}"
                        )
                        return None
                else:
                    if oneEntityToken == self.grounded.entityToken:
                        self.groundedConnections.append(occurrenceTwo)
                    elif twoEntityToken == self.grounded.entityToken:
                        self.groundedConnections.append(occurrenceOne)

            except:
                self.logger.error("Failed:\n{}".format(traceback.format_exc()))
                continue

    def _linkAllAxis(self):
        # looks through each simulation nood starting with ground and orders them using edges
        # self.groundSimNode is ground
        self._recurseLink(self.groundSimNode)

    def _recurseLink(self, simNode: SimulationNode):
        connectedAxisNodes = [
            self.simulationNodesRef.get(componentKeys, None)
            for componentKeys in simNode.data.getConnectedAxisTokens()
        ]
        for connectedAxis in connectedAxisNodes:
            # connected is the occurrence
            if connectedAxis is not None:
                edge = SimulationEdge(JointRelationship.GROUND, connectedAxis)
                simNode.edges.append(edge)
                self._recurseLink(connectedAxis)

    def _lookForGroundedJoints(self):
        grounded_token = self.grounded.entityToken
        rootDynamicJoint = self.groundSimNode.data

        for grounded_connect in self.groundedConnections:
            self.currentTraversal = dict()
            self._populateNode(
                grounded_connect,
                rootDynamicJoint,
                OccurrenceRelationship.CONNECTION,
                is_ground=False,
            )

    def _populateAxis(self, occ_token: str, joint: adsk.fusion.Joint):
        occ = self.design.findEntityByToken(occ_token)[0]

        if occ is None:
            return

        self.currentTraversal = dict()

        rootNode = self._populateNode(occ, None, None)

        if rootNode is not None:
            axisNode = SimulationNode(rootNode, joint)
            self.simulationNodesRef[occ_token] = axisNode

    def _populateNode(
        self,
        occ: adsk.fusion.Occurrence,
        prev: DynamicOccurrenceNode,
        relationship: OccurrenceRelationship,
        is_ground=False,
    ):
        if occ.isGrounded and not is_ground:
            return
        elif (relationship == OccurrenceRelationship.NEXT) and (prev is not None):
            node = DynamicOccurrenceNode(occ)
            edge = DynamicEdge(relationship, node)
            prev.edges.append(edge)
            return
        elif (
            (occ.entityToken in self.dynamicJoints.keys()) and (prev is not None)
        ) or self.currentTraversal.get(occ.entityToken) is not None:
            return

        node = DynamicOccurrenceNode(occ)

        self.currentTraversal[occ.entityToken] = True

        for occurrence in occ.childOccurrences:
            self._populateNode(
                occurrence, node, OccurrenceRelationship.TRANSFORM, is_ground=is_ground
            )

        # if not is_ground:  # THIS IS A BUG - OCCURRENCE ACCESS VIOLATION
        try:
            for joint in occ.joints:
                if joint and joint.occurrenceOne and joint.occurrenceTwo:
                    occurrenceOne = joint.occurrenceOne
                    occurrenceTwo = joint.occurrenceTwo
                    connection = None
                    rigid = joint.jointMotion.jointType == 0

                    if rigid:
                        if joint.occurrenceOne == occ:
                            connection = joint.occurrenceTwo
                        if joint.occurrenceTwo == occ:
                            connection = joint.occurrenceOne
                    else:
                        if joint.occurrenceOne != occ:
                            connection = joint.occurrenceOne

                    if connection is not None:
                        if (
                            prev is None
                            or connection.entityToken != prev.data.entityToken
                        ):
                            self._populateNode(
                                connection,
                                node,
                                (
                                    OccurrenceRelationship.CONNECTION
                                    if rigid
                                    else OccurrenceRelationship.NEXT
                                ),
                                is_ground=is_ground,
                            )
                else:
                    continue
        except:
            pass  # This is to temporarily bypass the bug

        if prev is not None:
            edge = DynamicEdge(relationship, node)
            prev.edges.append(edge)

        self.currentTraversal[occ.entityToken] = node
        return node


def searchForGrounded(occ: adsk.fusion.Occurrence) -> Union[adsk.fusion.Occurrence, None]:
    """Search for a grounded component or occurrence in the assembly

    Args:
        occ (adsk.fusion.Occurrence): start point

    Returns:
        Union(adsk.fusion.Occurrence, None): Either a grounded part or nothing
    """
    if occ.objectType == "adsk::fusion::Component":
        # this makes it possible to search an object twice (unoptimized)
        collection = occ.allOccurrences

        # components cannot be grounded technically

    else:  # Object is an occurrence
        if occ.isGrounded:
            return occ

        collection = occ.childOccurrences

    for occ in collection:
        searched = searchForGrounded(occ)

        if searched != None:
            return searched

    return None


# ________________________ Build implementation ______________________ #


def BuildJointPartHierarchy(
    design: adsk.fusion.Design,
    joints: joint_pb2.Joints,
    options: parse_options,
    progressDialog: PDMessage,
):
    try:
        progressDialog.currentMessage = f"Constructing Simulation Hierarchy"
        progressDialog.update()

        jointParser = JointParser(design)
        rootSimNode = jointParser.groundSimNode

        populateJoint(rootSimNode, joints, progressDialog)

        # 1. Get Node
        # 2. Get Transform of current Node
        # 3. Set Transform
        # 4. SimNode.data contains list of all affected bodies
        # 5. Occurrence Relationship indicates how this SimNode is connected (for instance is this a rigid connection or directly parented)
        # 6.
        # 4. For each child

        # now add each wheel to the root I believe

        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

    except Warning:
        return False
    except:
        logging.getLogger(f"{INTERNAL_ID}.JointHierarchy").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def populateJoint(simNode: SimulationNode, joints: joint_pb2.Joints, progressDialog):
    if progressDialog.wasCancelled():
        raise RuntimeError("User canceled export")

    if not simNode.joint:
        proto_joint = joints.joint_instances["grounded"]
    else:
        proto_joint = joints.joint_instances[simNode.joint.entityToken]

    progressDialog.currentMessage = f"Linking Parts to Joint: {proto_joint.info.name}"
    progressDialog.update()

    if not proto_joint:
        logging.getLogger(f"{INTERNAL_ID}.JointHierarchy").error(
            f"Could not find protobuf joint for {simNode.name}"
        )
        return

    root = types_pb2.Node()

    # if DEBUG:
    #     print(f"Configuring {proto_joint.info.name}")

    # construct body tree if possible
    createTreeParts(simNode.data, OccurrenceRelationship.CONNECTION, root, progressDialog)

    proto_joint.parts.nodes.append(root)

    # next in line to be populated
    for edge in simNode.edges:
        populateJoint(edge.node, joints, progressDialog)


def createTreeParts(
    dynNode: DynamicOccurrenceNode,
    relationship: OccurrenceRelationship,
    node: types_pb2.Node,
    progressDialog,
):
    if progressDialog.wasCancelled():
        raise RuntimeError("User canceled export")

    # if it's the next part just exit early for our own sanity
    if relationship == OccurrenceRelationship.NEXT or dynNode.data.isLightBulbOn == False:
        return

    # set the occurrence / component id to reference the part
    try:
        node.value = dynNode.data.entityToken
    except RuntimeError:
        node.value = dynNode.data.name

    # if DEBUG:
    #    print(f" -- {dynNode.data.name} + rel : {relationship}\n")

    # possibly add additional information for the type of connection made
    # recurse and add all children connections
    for edge in dynNode.edges:
        child_node = types_pb2.Node()
        createTreeParts(edge.node, edge.relationship, child_node, progressDialog)
        node.children.append(child_node)
