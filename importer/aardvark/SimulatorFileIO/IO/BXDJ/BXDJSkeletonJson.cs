using System;

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using BXDSim.IO.BXDJ;
using Newtonsoft.Json.Linq;
/// <summary>
/// Utility functions for reading/writing BXDJ files
/// </summary>



public static partial class BXDJSkeletonJson
{
    /// <summary>
    /// Represents the current version of the BXDA file.
    /// </summary>
    public const string BXDJ_CURRENT_VERSION = "4.3.0";

    /// <summary>
    /// Ensures that every node is assigned a model file name by assigning all nodes without a file name a generated name.
    /// </summary>
    /// <param name="baseNode">The base node of the skeleton</param>
    /// <param name="overwrite">Overwrite existing</param>
    /// 

    

    public static void SetupFileNames(RigidNode_Base baseNode)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].ModelFileName == null) 
                nodes[i].ModelFileName = ("node_" + i + ".bxda");
        }
    }

    /// <summary>
    /// Writes out the skeleton file for the skeleton with the base provided to the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="baseNode"></param>
    public static void WriteSkeleton(string path, RigidNode_Base baseNode)
    {

        //EXPERIMENT

       // JSONExport(path, baseNode);
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);
        
        // Determine the parent ID for each node in the list.
        int[] parentID = new int[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].GetParent() != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].GetParent());

                if (parentID[i] < 0) throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
            }
            else
            {
                parentID[i] = -1;
            }
        }


        JsonSkeleton jsonSkeleton = new JsonSkeleton();
        jsonSkeleton.Version = BXDJ_CURRENT_VERSION;

        for (int i = 0; i < nodes.Count; i++)
        {
            JsonSkeletonNode node = new JsonSkeletonNode();

            node.GUID = nodes[i].GUID.ToString();
            node.ParentID = parentID[i].ToString();
            node.ModelFileName = FileUtilities.SanatizeFileName("node_" + i + ".bxda");
            node.ModelID = nodes[i].GetModelID();

          

            if (parentID[i] >= 0)
            {
                node.joint = nodes[i].GetSkeletalJoint();
                
                node.joint.typeSave = node.joint.GetJointType();

                if (node.joint.cDriver != null)
                {
                    node.joint.cDriver.port1 += 1;
                    node.joint.cDriver.port2 += 1;
                }


            }



            //WriteJoint(nodes[i].GetSkeletalJoint(), node);

            jsonSkeleton.Nodes.Add(node);
        }
        jsonSkeleton.DriveTrainType = baseNode.driveTrainType;
        jsonSkeleton.SoftwareExportedWith = "INVENTOR";
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        string toWrite = JsonConvert.SerializeObject(jsonSkeleton, settings);
        File.WriteAllText(path, toWrite);
    }

    public static RigidNode_Base ReadSkeleton(string path)
    {
        string jsonData = File.ReadAllText(path);
        RigidNode_Base root = null;
        JsonConverter[] converters = { new JointConverter(), new DriverMetaConverter() };
        JsonSkeleton skeleton = JsonConvert.DeserializeObject<JsonSkeleton>(jsonData, new JsonSerializerSettings() { Converters = converters });
        List<JsonSkeletonNode> nodes = skeleton.Nodes;

        if(nodes.Count < 1)
        {
            Console.Error.WriteLine("0 Nodes Loaded! Failed import");

            return null;
        }

        foreach(JsonSkeletonNode node in nodes)
        {
            RigidNode_Base newNode = RigidNode_Base.NODE_FACTORY(new Guid(node.GUID));
            newNode.ModelFileName = node.ModelFileName;
            newNode.ModelFullID = node.ModelID;


            if (node.ParentID == "-1")
            {
                root = newNode;
                root.driveTrainType = skeleton.DriveTrainType;
               
            }
            else
            {
                
                root.AddChild(node.joint, newNode);
            }

            
        }

      
     
        return root;
    }
    public class JointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(SkeletalJoint_Base));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);

            switch (jo["typeSave"].Value<String>())
            {
                case "BALL":

                    return jo.ToObject <BallJoint_Base> (serializer);

                    break;

                case "CYLINDRICAL":
                    return jo.ToObject <CylindricalJoint_Base > (serializer);

                    break;

                case "LINEAR":
                    return jo.ToObject <LinearJoint_Base > (serializer);

                    break;

                case "PLANAR":
                    return jo.ToObject < PlanarJoint_Base > (serializer);

                    break;

                case "ROTATIONAL":
                    return jo.ToObject < RotationalJoint_Base > (serializer);

                    break;

                default:
                    return null;

            }


        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class DriverMetaConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(JointDriverMeta));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);

            switch (jo["$type"].Value<String>())
            {
                case "WheelDriverMeta, SimulatorAPI":

                    return jo.ToObject<WheelDriverMeta>(serializer);

                    break;

                case "ElevatorDriverMeta, SimulatorAPI":
                    return jo.ToObject<ElevatorDriverMeta>(serializer);

                    break;

                case "PneumaticDriverMeta, SimulatorAPI":
                    return jo.ToObject<PneumaticDriverMeta>(serializer);

                    break;

     

                default:
                    return null;

            }

        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }



    

    /// <summary>
    /// Reads the skeleton contained in the BXDJ file specified and returns the root node for that skeleton.
    /// </summary>
    /// <param name="path">The input BXDJ file</param>
    /// <returns>The root node of the skeleton</returns>
    public static RigidNode_Base ReadBinarySkeleton(string path)
    {
        BinaryReader reader = null;
        try
        {
            reader = new BinaryReader(new FileStream(path, FileMode.Open)); //Throws IOException
            // Sanity check
            uint version = reader.ReadUInt32();
            BXDIO.CheckReadVersion(version); //Throws FormatException

            int nodeCount = reader.ReadInt32();
            if (nodeCount <= 0) throw new Exception("This appears to be an empty skeleton");

            RigidNode_Base root = null;
            RigidNode_Base[] nodes = new RigidNode_Base[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                //nodes[i] = RigidNode_Base.NODE_FACTORY();

                int parent = reader.ReadInt32();
                nodes[i].ModelFileName = (reader.ReadString());
                nodes[i].ModelFullID = (reader.ReadString());

                if (parent != -1)
                {
                    SkeletalJoint_Base joint = SkeletalJoint_Base.ReadJointFully(reader);
                    nodes[parent].AddChild(joint, nodes[i]);
                }
                else
                {
                    root = nodes[i];
                }
            }

            if (root == null)
            {
                throw new Exception("This skeleton has no known base.  \"" + path + "\" is probably corrupted.");
            }

            return root;
        }
        catch (FormatException fe)
        {
            Console.WriteLine("File version mismatch");
            Console.WriteLine(fe);
            return null;
        }
        catch (IOException ie)
        {
            Console.WriteLine("Could not open skeleton file");
            Console.WriteLine(ie);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
        finally
        {
            if (reader != null) reader.Close();
        }
    }

    /// <summary>
    /// Clones joint settings for matching skeletal joints from one skeleton to the other.  This does not overwrite existing joint drivers.
    /// </summary>
    /// <param name="from">Source skeleton</param>
    /// <param name="to">Destination skeleton</param>
    public static void CloneDriversFromTo(RigidNode_Base from, RigidNode_Base to, bool overwrite = false)
    {
        List<RigidNode_Base> tempNodes = new List<RigidNode_Base>();
        from.ListAllNodes(tempNodes);

        Dictionary<string, RigidNode_Base> fromNodes = new Dictionary<string, RigidNode_Base>();
        foreach (RigidNode_Base cpy in tempNodes)
        {
            fromNodes[cpy.GetModelID()] = cpy;
        }

        tempNodes.Clear();
        to.ListAllNodes(tempNodes);
        foreach (RigidNode_Base copyTo in tempNodes)
        {
            RigidNode_Base fromNode;
            if (fromNodes.TryGetValue(copyTo.GetModelID(), out fromNode))
            {
                if (copyTo.GetSkeletalJoint() != null && fromNode.GetSkeletalJoint() != null && copyTo.GetSkeletalJoint().GetJointType() == fromNode.GetSkeletalJoint().GetJointType())
                {
                    if(copyTo.GetSkeletalJoint().cDriver == null || overwrite)
                    {
                        // Swap driver.
                        copyTo.GetSkeletalJoint().cDriver = fromNode.GetSkeletalJoint().cDriver;
                    }

                    if (copyTo.GetSkeletalJoint().attachedSensors.Count == 0 || overwrite)
                    {
                        // Swap sensors.
                        copyTo.GetSkeletalJoint().attachedSensors = fromNode.GetSkeletalJoint().attachedSensors;
                    }
                }
            }
        }
    }

}
