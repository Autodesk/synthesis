# This module is defined to help define the layout of the joints graph container
# Bascially take all of the parent-child relationships and turn that into a graph with discernable end

# Cannot import ParseOptions - something is very broken
# from ..ParseOptions import _Joint
from typing import List

from proto.proto_out import types_pb2, joint_pb2
from ...general_imports import logging, INTERNAL_ID, DEBUG

def createJointHierarchy(
    supplied_joints : list,
    joint_tree : types_pb2.GraphContainer
) -> None:

    # keep track of current nodes to link them
    node_map = dict({})

    # contains all of the static ground objects
    groundNode = types_pb2.Node()
    groundNode.value = "ground"

    node_map[groundNode.value] = groundNode

    # first iterate through to create the nodes
    for supplied_joint in supplied_joints:
        newNode = types_pb2.Node()
        newNode.value = supplied_joint.joint_token
        node_map[newNode.value] = newNode

    # second sort them
    for supplied_joint in supplied_joints:
        current_node = node_map[supplied_joint.joint_token]
        if (supplied_joint.parent == 0):
            node_map["ground"].children.append(node_map[supplied_joint.joint_token])
        elif node_map[supplied_joint.parent] is not None and node_map[supplied_joint.joint_token] is not None:
            node_map[supplied_joint.parent].children.append(node_map[supplied_joint.joint_token])
        else:
            logging.getLogger('JointHierarchy').error(f"Cannot construct hierarhcy because of detached tree at : {supplied_joint.joint_token}")

    for node in node_map.values():
        joint_tree.nodes.append(node)
    
    # only need to append ground