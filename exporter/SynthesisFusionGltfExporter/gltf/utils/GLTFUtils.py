from pygltflib import Node


def isEmptyLeafNode(node: Node) -> bool:
    return len(node.children) == 0 and node.mesh is None and node.camera is None and len(node.extensions) == 0 and len(node.extras) == 0