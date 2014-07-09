using System;
using System.Collections.Generic;
using System.IO;

public class BXDJSkeleton
{
    /// <summary>
    /// Writes out the skeleton file for the skeleton with the base provided to the path provided.
    /// </summary>
    /// <param name="path">The output file path</param>
    /// <param name="baseNode">The base node of the skeleton</param>
    /// <param name="bxdaOutputPath">Returns a mapping of node to expected output path of the BXDA file</param>
    public static void WriteSkeleton(String path, RigidNode_Base baseNode, out Dictionary<RigidNode_Base, string> bxdaOutputPath)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        // Determine the parent and driver IDs for each node in the list.
        List<JointDriver> jointDrivers = new List<JointDriver>();
        int[] parentID = new int[nodes.Count];
        int[] driverID = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].GetParent() != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].GetParent());
                if (parentID[i] < 0)
                {
                    throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
                }
            }
            else
            {
                parentID[i] = -1;
            }
            if (nodes[i].GetSkeletalJoint() != null && nodes[i].GetSkeletalJoint().cDriver != null)
            {
                driverID[i] = jointDrivers.Count;
                jointDrivers.Add(nodes[i].GetSkeletalJoint().cDriver);
            }
            else
            {
                driverID[i] = -1;
            }
        }

        // Begin IO
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));

        writer.Write(BXDIO.FORMAT_VERSION);

        bxdaOutputPath = new Dictionary<RigidNode_Base, string>(); // Prepare output paths

        // Write node values
        writer.Write(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
        {
            writer.Write(parentID[i]);
            nodes[i].SetModelFileName(FileUtilities.SanatizeFileName("node_" + i + ".bxda"));

            writer.Write(nodes[i].GetModelFileName());
            writer.Write(nodes[i].GetModelID());
            bxdaOutputPath.Add(nodes[i], Directory.GetParent(path) + "\\" + nodes[i].GetModelFileName());
            if (parentID[i] >= 0)
            {
                writer.Write(driverID[i]);

                writer.Write((byte)((int)nodes[i].GetSkeletalJoint().GetJointType()));
                nodes[i].GetSkeletalJoint().WriteJoint(writer);
            }
        }

        // Write driver values
        writer.Write(jointDrivers.Count);
        foreach (JointDriver d in jointDrivers)
        {
            d.WriteData(writer);
        }
        writer.Close();
    }

    /// <summary>
    /// Reads the skeleton contained in the BXDJ file specified and returns the root node for that skeleton.
    /// </summary>
    /// <param name="path">The input BXDJ file</param>
    /// <returns>The root node of the skeleton</returns>
    public static RigidNode_Base ReadSkeleton(string path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        // Sanity check
        uint version = reader.ReadUInt32();
        if (version != BXDIO.FORMAT_VERSION)
        {
            reader.Close();
            throw new Exception("\"" + path + "\" was created with format version " + BXDIO.VersionToString(version) + ", this library was compiled to read version " + BXDIO.VersionToString(BXDIO.FORMAT_VERSION));
        }

        int nodeCount = reader.ReadInt32();
        if (nodeCount <= 0)
        {
            reader.Close();
            throw new Exception("This appears to be an empty skeleton");
        }
        RigidNode_Base root = null;
        RigidNode_Base[] nodes = new RigidNode_Base[nodeCount];
        int[] driveIndex = new int[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i] = RigidNode_Base.NODE_FACTORY.Create();
            int parent = reader.ReadInt32();
            nodes[i].SetModelFileName(reader.ReadString());
            nodes[i].SetModelID(reader.ReadString());
            if (parent != -1)
            {
                driveIndex[i] = reader.ReadInt32();
                SkeletalJoint_Base joint = SkeletalJoint_Base.ReadJointFully(reader);
                nodes[parent].AddChild(joint, nodes[i]);
            }
            else
            {
                driveIndex[i] = -1;
                root = nodes[i];
            }
        }

        if (root == null)
        {
            reader.Close();
            throw new Exception("This skeleton has no known base.  \"" + path + "\" is probably corrupted.");
        }

        int driveCount = reader.ReadInt32();
        JointDriver[] drivers = new JointDriver[driveCount];
        for (int i = 0; i < driveCount; i++)
        {
            drivers[i] = new JointDriver(JointDriverType.MOTOR);    // Real type resolved in next call
            drivers[i].ReadData(reader);
        }
        for (int i = 0; i < nodeCount; i++)
        {
            if (driveIndex[i] >= 0 && driveIndex[i] < driveCount && nodes[i].GetSkeletalJoint() != null)
            {
                nodes[i].GetSkeletalJoint().cDriver = drivers[driveIndex[i]];
            }
        }
        reader.Close();
        return root;
    }
}
