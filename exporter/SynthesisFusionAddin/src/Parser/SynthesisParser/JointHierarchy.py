import enum
import sys
from logging import ERROR
from os import error
from typing import Any, Iterator, cast

import adsk.core
import adsk.fusion

from src import gm
from src.ErrorHandling import Err, ErrorSeverity, Ok, Result, handle_err_top
from src.Logging import getLogger
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.PDMessage import PDMessage
from src.Parser.SynthesisParser.Utilities import guid_component, guid_occurrence
from src.Proto import joint_pb2, types_pb2

logger = getLogger()

# ____________________________ DATA TYPES __________________


# this is more of a tree - todo rewrite
class GraphNode:
    def __init__(self, data: Any) -> None:
        self.data = data
        self.previous = None
        self.edges: list[GraphEdge] = list()

    def iter(self, filter_relationship: list[enum.Enum] = []) -> Iterator["GraphNode"]:
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

    def __iter__(self) -> Iterator["GraphNode"]:
        for edge in self.edges:
            yield edge.node

    def allChildren(self) -> list["GraphNode"]:
        nodes = [self]
        for edge in self.edges:
            nodes.extend(edge.node.allChildren())
        return nodes


class RelationshipBase(enum.Enum): ...


class OccurrenceRelationship(RelationshipBase):
    TRANSFORM = 1  # As in hierarchy parenting
    CONNECTION = 2  # As in a rigid joint or other designator
    GROUP = 3  # As in a Rigid Grouping
    NEXT = 4  # As in next_joint in list
    END = 5  # Orphaned child relationship


class JointRelationship(RelationshipBase):
    GROUND = 1  # This currently has no bearing
    ROTATIONAL = 2  # This currently has no bearing


class GraphEdge:
    def __init__(self, relationship: RelationshipBase | None, node: GraphNode) -> None:
        """A GraphEdge representing a edge in the GraphNode

        Args:
            relationship (enum.Enum): Relationship that can be used as a filter
            node (GraphNode): Node to be linked
        """
        self.relationship = relationship
        self.node = node

    def print(self) -> None:
        if self.relationship is None:
            name = "None"
        else:
            name = self.relationship.name

        print(f"Edge Containing {name} -> {self.node}")

    def __iter__(self) -> Iterator["GraphEdge"]:
        """Iterator for Edges within this edge

        Yields:
            GraphEdge: Edges in the node connected
        """
        return (edge for edge in self.node.edges)


# ______________________ INDIVIDUAL JOINT CHAINS ____________________________


class DynamicOccurrenceNode(GraphNode):
    def __init__(self, occurrence: adsk.fusion.Occurrence, isGround: bool = False, previous: GraphNode | None = None):
        super().__init__(occurrence)
        self.isGround = isGround
        self.name = occurrence.name

    def print(self) -> None:
        print(f"\n\t-------{self.data.name}-------")
        for edge in self.edges:
            edge.print()

    def getConnectedAxis(self) -> list[Any]:
        """Gets all Axis with the NEXT relationship

        Returns:
            list: list of Occurrences
        """
        nextItems = list()
        for edge in self.edges:
            if edge.relationship == OccurrenceRelationship.NEXT:
                nextItems.append(edge.node.data)
            else:
                nextItems.extend(cast(DynamicOccurrenceNode, edge.node).getConnectedAxis())
        return nextItems

    def getConnectedAxisTokens(self) -> list[str]:
        """Gets all Axis with the NEXT relationship

        Returns:
            list: list of Occurrences
        """
        nextItems = list()
        for edge in self.edges:
            if edge.relationship == OccurrenceRelationship.NEXT:
                nextItems.append(edge.node.data.entityToken)
            else:
                nextItems.extend(cast(DynamicOccurrenceNode, edge.node).getConnectedAxisTokens())
        return nextItems


class DynamicEdge(GraphEdge):
    # should print all in this class
    def print(self) -> None:
        if self.relationship is None:
            name = "None"
        else:
            name = self.relationship.name

        print(f"\t\t - {name} -> {self.node.data.name}")
        cast(DynamicOccurrenceNode, self.node).print()


# ______________________ ENTIRE SIMULATION STRUCTURE _______________________


class SimulationNode(GraphNode):
    def __init__(
        self,
        dynamicJoint: DynamicOccurrenceNode | None,
        joint: adsk.fusion.Joint,
        grounded: bool = False,
    ) -> None:
        super().__init__(dynamicJoint)
        self.joint = joint
        self.grounded = grounded

        if self.grounded:
            self.name = "Grounded"
        else:
            self.name = self.joint.name

    def print(self) -> None:
        print(f"Simulation Node for joint : {self.name} ")

    def printLink(self) -> None:
        if self.grounded:
            print(f"GROUND -- {self.data.data.name}")
        else:
            print(f"--> {self.data.data.name}")

        for edge in self.edges:
            cast(SimulationNode, edge.node).printLink()


class SimulationEdge(GraphEdge): ...


# ______________________________ PARSER ___________________________________


class JointParser:
    grounded: adsk.fusion.Occurrence

    # NOTE This function cannot under the value-based error handling system, since it's an  __init__ function
    def __init__(self, design: adsk.fusion.Design) -> None:
        """Create hierarchy with just joint assembly
        - Assembly
          - Grounded
          - Axis 1
          - Axis 2
            - Axis 3

        1. Find all Dynamic joint items to isolate                        [o]
        2. Find the grounded component                                    [x] (possible - not optimized)
        3. Populate tree with all items from each set of joints           [x] (done with grounding)
        - 3. a) Each Child element with no joints                         [x]
        - 3. b) Each Rigid Joint Connection                               [x]
        4. Link Joint trees by discovery from root                        [x]
        5. Record which trees have no children for creating end effectors [x] (next up) - this kinda already exists

        Need to investigate creating an additional button for end effector possibly
        It might be possible to have multiple end effectors
        Total Number of final elements"""

        self.current = None
        self.previousJoint = None

        self.design = design

        # this can be dynamically assigned if we want
        self.grounded = searchForGrounded(design.rootComponent)

        if self.grounded is None:
            message = "There is not a pinned component in this assembly, aborting kinematic export."
            # gm.ui.messageBox(message)
            _____: Err[None] = Err(message, ErrorSeverity.Fatal)
            raise RuntimeError(message)

        self.currentTraversal: dict[str, DynamicOccurrenceNode | bool] = dict()
        self.groundedConnections: list[adsk.fusion.Occurrence] = []

        # populate the rigidJoints connected to a given occurrence
        # Transition: AARD-1765
        # self.rigidJoints = dict()
        # populate all joints
        self.dynamicJoints: dict[str, adsk.fusion.Joint] = dict()

        # Maps an occurrence's entityToken to every joint touching it, along with that joint's two
        # resolved occurrences. Populated once in __getAllJoints() and consulted from _populateNode()
        # Avoids using `occ.joints` accessor, which is a documented Fusion API crash
        # (native access violation) on certain occurrences (e.g. large/derived components).
        self.occurrenceJoints: dict[
            str, list[tuple[adsk.fusion.Joint, adsk.fusion.Occurrence, adsk.fusion.Occurrence]]
        ] = dict()

        self.simulationNodesRef: dict[str, SimulationNode] = dict()

        # TODO: need to look through every single joint and find the starting point that is connected to ground
        # Next add that occurrence to the graph and then traverse down that path etc
        self.__getAllJoints()

        # dynamic joint node for grounded components and static components
        logger.log(10, f"Populating node tree for grounded occurrence '{self.grounded.name}'")
        populate_node_result = self._populateNode(self.grounded, None, None, is_ground=True)
        if populate_node_result.is_err():  # We need the value to proceed
            message = populate_node_result.unwrap_err()[0]
            gm.ui.messageBox(message)
            ____: Err[None] = Err(message, ErrorSeverity.Fatal)
            raise RuntimeError(message)

        rootNode = populate_node_result.unwrap()
        self.groundSimNode = SimulationNode(rootNode, None, grounded=True)

        self.simulationNodesRef["GROUND"] = self.groundSimNode

        # combine all ground prior to this possibly
        logger.log(10, f"Looking for grounded joints ({len(self.groundedConnections)} grounded connection(s))")
        _ = self._lookForGroundedJoints()

        # creates the axis elements - adds all elements to axisNodes
        logger.log(10, f"Populating {len(self.dynamicJoints)} axis/axes")
        for key, value in self.dynamicJoints.items():
            populate_axis_result = self._populateAxis(key, value)
            if populate_axis_result.is_err():
                message = populate_axis_result.unwrap_err()[0]
                gm.ui.messageBox(message)
                ___: Err[None] = Err(message, ErrorSeverity.Fatal)
                raise RuntimeError()

        logger.log(10, "Linking all axis/axes")
        __ = self._linkAllAxis()

        logger.log(10, "JointParser initialization complete")
        # self.groundSimNode.printLink()

    def _resolveOccurrenceFromGeometry(self, entity: Any, jointName: str, label: str) -> adsk.fusion.Occurrence | None:
        try:
            resolved = entity.assemblyContext if entity is not None else None
        except Exception as e:
            resolved = None
            _: Err[None] = Err(f"Exception resolving {label} for joint '{jointName}': {e}", ErrorSeverity.Warning)

        if resolved is None:
            __: Err[None] = Err(
                f"Could not resolve {label} for joint '{jointName}' "
                "(geometry/origin reference is broken or orphaned, e.g. from a copy-paste)",
                ErrorSeverity.Warning,
            )

        return resolved

    def __getAllJoints(self) -> Result[None]:
        logger.log(10, "Getting Joints")
        allJoints = list(self.design.rootComponent.allJoints) + list(self.design.rootComponent.allAsBuiltJoints)
        logger.log(10, f"Found {len(allJoints)} total joints/as-built-joints in design")

        for index, joint in enumerate(allJoints):
            jointName = joint.name if joint else "None"
            logger.log(10, f"[{index + 1}/{len(allJoints)}] Resolving occurrences for joint '{jointName}'")

            occurrenceOne: adsk.fusion.Occurrence | None = None
            occurrenceTwo: adsk.fusion.Occurrence | None = None

            if joint and joint.occurrenceOne and joint.occurrenceTwo:
                occurrenceOne = joint.occurrenceOne
                occurrenceTwo = joint.occurrenceTwo
            else:
                # Non-fatal since it's recovered in the next two statements
                _ = Err(f"Joint '{jointName}' found without two occurrences", ErrorSeverity.Warning)

            if occurrenceOne is None:
                try:
                    geometryOrOriginOne = joint.geometryOrOriginOne
                    entityOne = geometryOrOriginOne.entityOne if geometryOrOriginOne is not None else None
                except Exception as e:
                    entityOne = None
                    _ = Err(
                        f"Exception accessing geometryOrOriginOne for joint '{jointName}': {e}", ErrorSeverity.Warning
                    )
                occurrenceOne = self._resolveOccurrenceFromGeometry(entityOne, jointName, "occurrenceOne")

            if occurrenceTwo is None:
                try:
                    geometryOrOriginTwo = joint.geometryOrOriginTwo
                    entityTwo = geometryOrOriginTwo.entityTwo if geometryOrOriginTwo is not None else None
                except Exception as e:
                    entityTwo = None
                    _ = Err(
                        f"Exception accessing geometryOrOriginTwo for joint '{jointName}': {e}", ErrorSeverity.Warning
                    )
                occurrenceTwo = self._resolveOccurrenceFromGeometry(entityTwo, jointName, "occurrenceTwo")

            oneEntityToken = ""
            twoEntityToken = ""

            # TODO: Fix change to if statement with Result returning
            if occurrenceOne is not None:
                try:
                    oneEntityToken = occurrenceOne.entityToken
                except Exception:
                    oneEntityToken = occurrenceOne.name

            if occurrenceTwo is not None:
                try:
                    twoEntityToken = occurrenceTwo.entityToken
                except Exception:
                    twoEntityToken = occurrenceTwo.name

            if occurrenceOne is None or occurrenceTwo is None:
                # Already logged above (either missing both occurrences, or an unresolvable geometry/origin
                # reference) - skip this joint rather than aborting the whole export
                continue

            self.occurrenceJoints.setdefault(oneEntityToken, []).append((joint, occurrenceOne, occurrenceTwo))
            if twoEntityToken != oneEntityToken:
                self.occurrenceJoints.setdefault(twoEntityToken, []).append((joint, occurrenceOne, occurrenceTwo))

            typeJoint = joint.jointMotion.jointType

            if typeJoint != 0:
                if oneEntityToken not in self.dynamicJoints.keys():
                    self.dynamicJoints[oneEntityToken] = joint
            else:
                if oneEntityToken == self.grounded.entityToken:
                    self.groundedConnections.append(occurrenceTwo)
                elif twoEntityToken == self.grounded.entityToken:
                    self.groundedConnections.append(occurrenceOne)
        logger.log(
            10,
            f"Finished Getting Joints: {len(self.dynamicJoints)} dynamic joint(s), "
            f"{len(self.groundedConnections)} grounded connection(s)",
        )
        return Ok(None)

    def _linkAllAxis(self) -> Result[None]:
        # looks through each simulation nood starting with ground and orders them using edges
        # self.groundSimNode is ground
        return self._recurseLink(self.groundSimNode)

    def _recurseLink(self, simNode: SimulationNode) -> Result[None]:
        connectedAxisNodes = [
            self.simulationNodesRef.get(componentKeys, None) for componentKeys in simNode.data.getConnectedAxisTokens()
        ]
        if any([node is None for node in connectedAxisNodes]):
            return Err(f"Found None Connected Access Node", ErrorSeverity.Fatal)

        for connectedAxis in connectedAxisNodes:
            # connected is the occurrence
            if connectedAxis is not None:
                edge = SimulationEdge(JointRelationship.GROUND, connectedAxis)
                simNode.edges.append(edge)

                recurse_result = self._recurseLink(connectedAxis)
                if recurse_result.is_fatal():
                    return recurse_result
        return Ok(None)

    def _lookForGroundedJoints(self) -> Result[None]:
        # grounded_token = self.grounded.entityToken
        rootDynamicJoint = self.groundSimNode.data
        if rootDynamicJoint is None:
            return Err("Found None rootDynamicJoint", ErrorSeverity.Fatal)

        for grounded_connect in self.groundedConnections:
            self.currentTraversal = dict()
            _ = self._populateNode(
                grounded_connect,
                rootDynamicJoint,
                OccurrenceRelationship.CONNECTION,
                is_ground=False,
            )
        return Ok(None)

    def _populateAxis(self, occ_token: str, joint: adsk.fusion.Joint) -> Result[None]:
        occ = self.design.findEntityByToken(occ_token)[0]
        if occ is None:
            return Ok(None)

        self.currentTraversal = dict()

        populate_node_result = self._populateNode(occ, None, None)
        if populate_node_result.is_err():  # We need the value to proceed
            unwrapped = populate_node_result.unwrap_err()
            return Err(unwrapped[0], unwrapped[1])

        rootNode = populate_node_result.unwrap()
        if rootNode is not None:
            axisNode = SimulationNode(rootNode, joint)
            self.simulationNodesRef[occ_token] = axisNode

        return Ok(None)

    # TODO: Verify that this works after the Result-refactor :skull:
    def _populateNode(
        self,
        occ: adsk.fusion.Occurrence,
        prev: DynamicOccurrenceNode | None,
        relationship: OccurrenceRelationship | None,
        is_ground: bool = False,
    ) -> Result[DynamicOccurrenceNode | None]:
        if occ.isGrounded and not is_ground:
            return Ok(None)
        elif (relationship == OccurrenceRelationship.NEXT) and (prev is not None):
            node = DynamicOccurrenceNode(occ)
            edge = DynamicEdge(relationship, node)
            prev.edges.append(edge)
            return Ok(None)
        elif ((occ.entityToken in self.dynamicJoints.keys()) and (prev is not None)) or self.currentTraversal.get(
            occ.entityToken
        ) is not None:
            return Ok(None)

        node = DynamicOccurrenceNode(occ)

        self.currentTraversal[occ.entityToken] = True

        for occurrence in occ.childOccurrences:
            populate_result = self._populateNode(
                occurrence, node, OccurrenceRelationship.TRANSFORM, is_ground=is_ground
            )
            if populate_result.is_fatal():
                return populate_result

        # Deliberately not using the `occ.joints` accessor here as it is a documented Fusion API
        # crash. Instead look up joints touching occurrences from the map built once in __getAllJoints()
        occJoints = self.occurrenceJoints.get(occ.entityToken, [])
        for joint, occurrenceOne, occurrenceTwo in occJoints:
            connection = None
            rigid = joint.jointMotion.jointType == 0

            if rigid:
                if occurrenceOne.entityToken == occ.entityToken:
                    connection = occurrenceTwo
                if occurrenceTwo.entityToken == occ.entityToken:
                    connection = occurrenceOne
            else:
                if occurrenceOne.entityToken != occ.entityToken:
                    connection = occurrenceOne

            if connection is not None:
                if prev is None or connection.entityToken != prev.data.entityToken:
                    populate_result = self._populateNode(
                        connection,
                        node,
                        (OccurrenceRelationship.CONNECTION if rigid else OccurrenceRelationship.NEXT),
                        is_ground=is_ground,
                    )
                    if populate_result.is_fatal():
                        return populate_result

        if prev is not None:
            edge = DynamicEdge(relationship, node)
            prev.edges.append(edge)

        self.currentTraversal[occ.entityToken] = node
        return Ok(node)


def searchForGrounded(
    occ: adsk.fusion.Occurrence,
) -> adsk.fusion.Occurrence | None:
    """Search for a grounded component or occurrence in the assembly

    Args:
        occ (adsk.fusion.Occurrence): start point

    Returns:
        adsk.fusion.Occurrence | None: Either a grounded part or nothing
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


@handle_err_top
def buildJointPartHierarchy(
    design: adsk.fusion.Design,
    joints: joint_pb2.Joints,
    options: ExporterOptions,
    progressDialog: PDMessage,
) -> Result[None]:
    # This try-catch is necessary because the JointParser __init__ functon is fallible and throws a RuntimeWarning (__init__ functions cannot return values)
    try:
        progressDialog.currentMessage = f"Constructing Simulation Hierarchy"
        progressDialog.update()

        jointParser = JointParser(design)
        rootSimNode = jointParser.groundSimNode

        populate_joint_result = populateJoint(rootSimNode, joints, progressDialog)
        if populate_joint_result.is_fatal():
            return populate_joint_result

        # 1. Get Node
        # 2. Get Transform of current Node
        # 3. Set Transform
        # 4. SimNode.data contains list of all affected bodies
        # 5. Occurrence Relationship indicates how this SimNode is connected (for instance is this a rigid connection or directly parented)
        # 6.
        # 4. For each child

        # now add each wheel to the root I believe

        if progressDialog.wasCancelled():
            return Err("User canceled export", ErrorSeverity.Fatal)

        return Ok(None)

    except RuntimeError as e:
        progressDialog.progressDialog.hide()
        raise e


def populateJoint(simNode: SimulationNode, joints: joint_pb2.Joints, progressDialog: PDMessage) -> Result[None]:
    if progressDialog.wasCancelled():
        return Err("User canceled export", ErrorSeverity.Fatal)

    if not simNode.joint:
        proto_joint = joints.joint_instances["grounded"]
    else:
        proto_joint = joints.joint_instances[simNode.joint.entityToken]

    progressDialog.currentMessage = f"Linking Parts to Joint: {proto_joint.info.name}"
    progressDialog.update()

    if not proto_joint:
        return Err(f"Could not find protobuf joint for {simNode.name}", ErrorSeverity.Fatal)

    root = types_pb2.Node()

    # construct body tree if possible
    tree_parts_result = createTreeParts(simNode.data, OccurrenceRelationship.CONNECTION, root, progressDialog)
    if tree_parts_result.is_fatal():
        return tree_parts_result

    proto_joint.parts.nodes.append(root)

    # next in line to be populated
    for edge in simNode.edges:
        populate_joint_result = populateJoint(cast(SimulationNode, edge.node), joints, progressDialog)
        if populate_joint_result.is_fatal():
            return populate_joint_result
    return Ok(None)


def createTreeParts(
    dynNode: DynamicOccurrenceNode,
    relationship: RelationshipBase | None,
    node: types_pb2.Node,
    progressDialog: PDMessage,
) -> Result[None]:
    if progressDialog.wasCancelled():
        return Err("User canceled export", ErrorSeverity.Fatal)

    # if it's the next part just exit early for our own sanity
    # This shouldn't be fatal nor even an error
    if relationship == OccurrenceRelationship.NEXT or dynNode.data.isLightBulbOn == False:
        return Ok(None)

    # set the occurrence / component id to reference the part

    # Fine way to use try-excepts in this language
    if dynNode.data.objectType is None:
        _: Err[None] = Err("Found None object type", ErrorSeverity.Warning)
        objectType = ""
    else:
        objectType = dynNode.data.objectType

    if objectType == "adsk::fusion::Occurrence":
        node.value = guid_occurrence(dynNode.data)
    elif objectType == "adsk::fusion::Component":
        node.value = guid_component(dynNode.data)
    else:
        if dynNode.data.entityToken is None:
            __: Err[None] = Err("Found None EntityToken", ErrorSeverity.Warning)
            node.value = dynNode.data.name
        else:
            node.value = dynNode.data.entityToken

    # possibly add additional information for the type of connection made
    # recurse and add all children connections
    for edge in dynNode.edges:
        child_node = types_pb2.Node()
        tree_parts_result = createTreeParts(
            cast(DynamicOccurrenceNode, edge.node), edge.relationship, child_node, progressDialog
        )
        if tree_parts_result.is_fatal():
            return tree_parts_result
        node.children.append(child_node)

    return Ok(None)
