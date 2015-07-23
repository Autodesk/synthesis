using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class BXDFProperties
{
    /// <summary>
    /// Writes out the properties file for the node with the base provided to the path provided.
    /// </summary>
    /// <param name="path">The output file path</param>
    /// <param name="fieldDefinition">The base node of the skeleton</param>
    public static void WriteProperties(String path, FieldDefinition_Base fieldDefinition)
    {
        // Begin IO
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create));

        // Writes the format version.
        writer.Write(BXDIO.FORMAT_VERSION);

        // Writes the name of the field definition.
        writer.Write(fieldDefinition.definitionID);

        List<FieldNode_Base> nodes = fieldDefinition.GetChildren();

        // Writes node count.
        writer.Write(nodes.Count);

        for (int i = 0; i < nodes.Count; i++)
        {
            // Writes the name of the current node.
            writer.Write(nodes[i].nodeID);

            // Writes the type of collision of the current node.
            writer.Write((byte)nodes[i].nodeCollisionType);

            // Writes a boolean determining if the node's collision mesh is convex.
            writer.Write(nodes[i].convex);

            // Writes the friction of 
            writer.Write(nodes[i].friction);
        }
        writer.Close();
    }

    /// <summary>
    /// Reads the properties contained in the BXDF file specified and returns the corresponding definition.
    /// </summary>
    /// <param name="path">The input BXDJ file</param>
    /// <returns>The root node of the skeleton</returns>
    public static FieldDefinition_Base ReadProperties(string path)
    {
        BinaryReader reader = null;
        try
        {
            reader = new BinaryReader(new FileStream(path, FileMode.Open)); //Throws IOException
            // Checks version.
            uint version = reader.ReadUInt32();
            BXDIO.CheckReadVersion(version);

            FieldDefinition_Base fieldDefinition = FieldDefinition_Base.FIELDDEFINITION_FACTORY();
            // Reads definition's ID.
            fieldDefinition.definitionID = reader.ReadString();

            // Reads number of nodes.
            int nodeCount = reader.ReadInt32();
            if (nodeCount <= 0) throw new Exception("There appears to be no child nodes.");

            FieldNode_Base[] nodes = new FieldNode_Base[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = FieldNode_Base.FIELDNODE_FACTORY();

                // Reads ID of node.
                nodes[i].nodeID = reader.ReadString();

                // Reads type of collision for node.
                nodes[i].nodeCollisionType = (FieldNodeCollisionType)reader.ReadByte();

                // Reads if collision mesh is convex
                nodes[i].convex = reader.ReadBoolean();

                // Reads friction value.
                nodes[i].friction = reader.ReadInt32();

                fieldDefinition.AddChild(nodes[i]);
            }

            // Returns result.
            return fieldDefinition;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (reader != null) reader.Close();
        }
    }
}
