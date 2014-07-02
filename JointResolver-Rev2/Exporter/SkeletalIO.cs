using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class SkeletalIO
{
    public static void write(String path, List<RigidNode> nodes, out Dictionary<CustomRigidGroup, string> bxdaOutputPath)
    {
        // Check if we know parents
        /*List<JointDriver> jointDrivers = new List<JointDriver>();
        int[] parentID = new int[nodes.Count];
        int[] driverID = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].parent != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].parent);
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

        bxdaOutputPath = new Dictionary<CustomRigidGroup, string>(); // Prepare output paths

        // Write node values
        writer.Write(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
        {
            writer.Write(parentID[i]);
            string modelName = "node_" + (nodes[i].group.occurrences.Count > 0 ? nodes[i].group.occurrences[0].Name : "empty") +
                (nodes[i].group.occurrences.Count > 1 ? "_x" + nodes[i].group.occurrences.Count : "") + ".bxda";
            modelName = Utilities.sanatizeFileName(modelName);

            writer.Write(modelName);
            bxdaOutputPath.Add(nodes[i].group, Directory.GetParent(path) + "\\" + modelName);
            if (parentID[i] >= 0)
            {
                writer.Write(driverID[i]);
                writer.Write((byte)nodes[i].getSkeletalJoint().getJointType());
                nodes[i].getSkeletalJoint().writeJoint(writer);
            }
        }

        // Write driver values
        writer.Write(jointDrivers.Count);
        foreach (JointDriver d in jointDrivers)
        {
            d.writeData(writer);
        }*/


    }
}