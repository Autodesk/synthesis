using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class SkeletonIO
{
    public static void writeSkeleton(String path, RigidNode_Base baseNode, out Dictionary<RigidNode_Base, string> bxdaOutputPath)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.listAllNodes(nodes);

        // Check if we know parents
        List<JointDriver> jointDrivers = new List<JointDriver>();
        int[] parentID = new int[nodes.Count];
        int[] driverID = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].getParent() != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].getParent());
                if (parentID[i] < 0)
                {
                    throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
                }
            }
            else
            {
                parentID[i] = -1;
            }
            if (nodes[i].getSkeletalJoint() != null && nodes[i].getSkeletalJoint().cDriver != null)
            {
                driverID[i] = jointDrivers.Count;
                jointDrivers.Add(nodes[i].getSkeletalJoint().cDriver);
            }
            else
            {
                driverID[i] = -1;
            }
        }

        // Begin IO
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));

        bxdaOutputPath = new Dictionary<RigidNode_Base, string>(); // Prepare output paths

        // Write node values
        writer.Write(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
        {
            writer.Write(parentID[i]);
            string modelName = "node_" + i + ".bxda";
            modelName = FileUtilities.sanatizeFileName(modelName);

            writer.Write(modelName);
            bxdaOutputPath.Add(nodes[i], Directory.GetParent(path) + "\\" + modelName);
            if (parentID[i] >= 0)
            {
                writer.Write(driverID[i]);

                writer.Write((byte)((int)nodes[i].getSkeletalJoint().getJointType()));
                nodes[i].getSkeletalJoint().writeJoint(writer);
            }
        }

        // Write driver values
        writer.Write(jointDrivers.Count);
        foreach (JointDriver d in jointDrivers)
        {
            d.writeData(writer);
        }
        writer.Close();
    }

    public static RigidNode_Base readSkeleton(string path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        int nodeCount = reader.ReadInt32();
        RigidNode_Base root = null;
        RigidNode_Base[] nodes = new RigidNode_Base[nodeCount];
        int[] driveIndex = new int[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i] = RigidNode_Base.NODE_FACTORY.create();
            int parent = reader.ReadInt32();
            nodes[i].modelName = reader.ReadString();
            if (parent != -1)
            {
                driveIndex[i] = reader.ReadInt32();
                SkeletalJoint_Base joint = SkeletalJoint_Base.readJointFully(reader);
                nodes[parent].addChild(joint, nodes[i]);
            }
            else
            {
                driveIndex[i] = -1;
                root = nodes[i];
            }
        }

        int driveCount = reader.ReadInt32();
        JointDriver[] drivers = new JointDriver[driveCount];
        for (int i = 0; i < driveCount; i++)
        {
            drivers[i] = new JointDriver(JointDriverType.MOTOR);    // Real type resolved in next call
            drivers[i].readData(reader);
        }
        for (int i = 0; i < nodeCount; i++)
        {
            if (driveIndex[i] >= 0 && driveIndex[i] < driveCount && nodes[i].getSkeletalJoint() != null)
            {
                nodes[i].getSkeletalJoint().cDriver = drivers[driveIndex[i]];
            }
        }
        reader.Close();
        return root;
    }
}
