using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class FieldNodeGroup
{
    /// <summary>
    /// The string ID for the FieldNodeGroup.
    /// </summary>
    public string NodeGroupID;

    /// <summary>
    /// A list containing each child FieldNode.
    /// </summary>
    private Dictionary<string, FieldNode> childNodes;

    /// <summary>
    /// A list containing each child FieldNodeGroup.
    /// </summary>
    private Dictionary<string, FieldNodeGroup> childNodeGroups;

    /// <summary>
    /// Used for defining how a child will be processed while iterating through a node path.
    /// </summary>
    /// <param name="childName"></param>
    /// <param name="childPath"></param>
    /// <param name="lastChild"></param>
    /// <returns></returns>
    private delegate FieldNode HandleChildInfo(string childName, string childPath, bool lastChild);

    /// <summary>
    /// Used for setting or getting a FieldNode from the path supplied.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public FieldNode this[string path]
    {
        set
        {
            ProcessNextChild(path,
                new HandleChildInfo((string childName, string childPath, bool lastChild) =>
                {
                    if (lastChild)
                    {
                        FieldNode node = GetNode(childName);

                        if (node == null)
                            AddNode(value);
                        else
                            node = value;
                    }
                    else
                    {
                        FieldNodeGroup nodeGroup = GetNodeGroup(childName);

                        if (nodeGroup == null)
                            nodeGroup = AddNodeGroup(childName);

                        nodeGroup[childPath] = value;
                    }

                    return null;
                }));
        }

        get
        {
            return ProcessNextChild(path,
                new HandleChildInfo((string childName, string childPath, bool lastChild) =>
                {
                    if (lastChild)
                    {
                        return GetNode(childName);
                    }
                    else
                    {
                        FieldNodeGroup nodeGroup = GetNodeGroup(childName);

                        if (nodeGroup == null)
                            return null;

                        return nodeGroup[childPath];
                    }
                }));
        }
    }

    /// <summary>
    /// Constructs a new instance of the FieldNodeGroup class.
    /// </summary>
    /// <param name="nodeGroupID"></param>
    public FieldNodeGroup(string nodeGroupID)
    {
        NodeGroupID = nodeGroupID;
        childNodes = new Dictionary<string, FieldNode>();
        childNodeGroups = new Dictionary<string, FieldNodeGroup>();
    }

    /// <summary>
    /// Adds a new node with the given ID and physicsGroupID to the childNodes Dictionary.
    /// </summary>
    /// <param name="nodeID"></param>
    public FieldNode AddNode(string nodeID, string physicsGroupID = BXDFProperties.BXDF_DEFAULT_NAME)
    {
        if (!childNodes.ContainsKey(nodeID))
        {
            FieldNode node = new FieldNode(nodeID, physicsGroupID);
            childNodes.Add(nodeID, node);

            return node;
        }

        return null;
    }

    /// <summary>
    /// Adds the given FieldNode to the ChildNodes Dictionary.
    /// </summary>
    /// <param name="node"></param>
    public void AddNode(FieldNode node)
    {
        if (!childNodes.ContainsKey(node.NodeID))
            childNodes.Add(node.NodeID, node);
    }

    /// <summary>
    /// Returns the child FieldNode with the given nodeID.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns>The FieldNode.</returns>
    public FieldNode GetNode(string nodeID)
    {
        if (childNodes.ContainsKey(nodeID))
            return childNodes[nodeID];

        return null;
    }
    
    /// <summary>
    /// Adds a new FieldNodeGroup with the given ID.
    /// </summary>
    /// <param name="nodeGroupID"></param>
    public FieldNodeGroup AddNodeGroup(string nodeGroupID)
    {
        if (!childNodeGroups.ContainsKey(nodeGroupID))
        {
            FieldNodeGroup nodeGroup = new FieldNodeGroup(nodeGroupID);
            childNodeGroups.Add(nodeGroupID, nodeGroup);

            return nodeGroup;
        }

        return null;
    }

    /// <summary>
    /// Adds the given FieldNodeGroup to the childNodeGroups Dictionary.
    /// </summary>
    /// <param name="nodeGroup"></param>
    public void AddNodeGroup(FieldNodeGroup nodeGroup)
    {
        if (!childNodeGroups.ContainsKey(nodeGroup.NodeGroupID))
            childNodeGroups.Add(nodeGroup.NodeGroupID, nodeGroup);
    }

    /// <summary>
    /// Returns the FieldNodeGroup with the given nodeGroupID.
    /// </summary>
    /// <param name="nodeGroupID"></param>
    /// <returns>The FieldNodeGroup.</returns>
    public FieldNodeGroup GetNodeGroup(string nodeGroupID)
    {
        if (childNodeGroups.ContainsKey(nodeGroupID))
            return childNodeGroups[nodeGroupID];

        return null;
    }

    /// <summary>
    /// Enumerates through all direct child FieldNodes.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldNode> EnumerateFieldNodes()
    {
        foreach (KeyValuePair<string, FieldNode> node in childNodes)
        {
            yield return node.Value;
        }
    }

    /// <summary>
    /// Enumerates through all direct child FieldNodeGroups.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldNodeGroup> EnumerateFieldNodeGroups()
    {
        foreach (KeyValuePair<string, FieldNodeGroup> nodeGroup in childNodeGroups)
        {
            yield return nodeGroup.Value;
        }
    }

    /// <summary>
    /// Enumerates through each child FieldNode in all child FieldNodeGroups.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldNode> EnumerateAllLeafFieldNodes()
    {
        foreach (FieldNode node in EnumerateFieldNodes())
        {
            yield return node;
        }

        foreach (FieldNodeGroup nodeGroup in EnumerateFieldNodeGroups())
        {
            foreach (FieldNode node in nodeGroup.EnumerateAllLeafFieldNodes())
            {
                yield return node;
            }
        }
    }

    /// <summary>
    /// Used for processing the next child in a path string.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="Handle"></param>
    /// <returns></returns>
    private FieldNode ProcessNextChild(string path, HandleChildInfo Handle)
    {
        if (path.Contains('/'))
            return Handle(path.Substring(0, path.IndexOf('/')), path.Substring(path.IndexOf('/') + 1), false);
        else
            return Handle(path, path, true);
    }
}