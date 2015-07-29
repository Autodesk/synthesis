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
    /// <param name="fieldDefinition">The field definition to reference</param>
    public static void WriteProperties(String path, FieldDefinition_Base fieldDefinition)
    {
        // Begin IO.
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create));

        // Writes the format version.
        writer.Write(BXDIO.FORMAT_VERSION);

        // Writes the name of the field definition.
        writer.Write(fieldDefinition.definitionID);

        Dictionary<string, PhysicsGroup> physicsGroups = fieldDefinition.GetPhysicsGroups();

        // Writes PhysicsGroup count.
        writer.Write(physicsGroups.Count);
        
        for (int i = 0; i < physicsGroups.Count; i++)
        {
            // The current PhysicsGroup.
            PhysicsGroup currentGroup = physicsGroups.ElementAt(i).Value;

            // Writes the name/ID of the physics group.
            writer.Write(currentGroup.physicsGroupID);

            // Writes the collision type of the current PhysicsGroup.
            writer.Write((byte)currentGroup.collisionType);

            // Writes the friction of the PhysicsGroup.
            writer.Write(currentGroup.friction);

            // Writes a boolean determining if the PhysicsGroup is dynamic.
            writer.Write(currentGroup.dynamic);

            // Writes a double representing the PhysicsGroup's mass.
            writer.Write(currentGroup.mass);
        }

        List<FieldNode_Base> nodes = fieldDefinition.GetChildren();

        // Writes node count.
        writer.Write(nodes.Count);

        for (int i = 0; i < nodes.Count; i++)
        {
            // Writes the name of the current node.
            writer.Write(nodes[i].nodeID);

            // Writes the name of the parent PhysicsGroup.
            writer.Write(nodes[i].physicsGroupID);
        }
        writer.Close();
    }

    /// <summary>
    /// Reads the properties contained in the BXDF file specified and returns the corresponding definition.
    /// </summary>
    /// <param name="path">The input BXDF file</param>
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

            // Reads number of PhysicsGroups.
            int groupCount = reader.ReadInt32();

            PhysicsGroup[] groups = new PhysicsGroup[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                // Reads the ID of the current PhysicsGroup.
                groups[i].physicsGroupID = reader.ReadString();

                // Reads the collision type of the current PhysicsGroup.
                groups[i].collisionType = (PhysicsGroupCollisionType)reader.ReadByte();

                // Reads the friction value of the current PhysicsGroup.
                groups[i].friction = reader.ReadInt32();

                // Reads a boolean to determin if the PhysicsGroup is dynamic.
                groups[i].dynamic = reader.ReadBoolean();

                // Reads the mass value of the current PhysicsGroup.
                groups[i].mass = reader.ReadDouble();

                fieldDefinition.AddPhysicsGroup(groups[i]);
            }

            // Reads number of nodes.
            int nodeCount = reader.ReadInt32();

            FieldNode_Base[] nodes = new FieldNode_Base[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = FieldNode_Base.FIELDNODE_FACTORY();

                // Reads ID of node.
                nodes[i].nodeID = reader.ReadString();

                // Reads ID of node's PhysicsGroup.
                nodes[i].physicsGroupID = reader.ReadString();

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
