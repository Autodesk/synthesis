using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static PropertySet;

public partial class BXDFPropertiesJson
{
    /// <summary>
    /// Represents the current version of the BXDF file.
    /// </summary>
    public const string BXDF_CURRENT_VERSION = "2.2.0";

    /// <summary>
    /// Represents the default name of any element.
    /// </summary>
    public const string BXDF_DEFAULT_NAME = "UNDEFINED";

    /// <summary>
    /// Reads the given BXDF file from the given path with the latest version possible.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadProperties(string path)
    {
        string result;
        return ReadProperties(path, out result);
    }

    /// <summary>
    /// Reads the given BXDF file from the given path with the latest version possible.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadProperties(string path, out string result)
    {
        string data = System.IO.File.ReadAllText(path);
        result = data;
        JsonConverter[] converters = { new ProperySetCoverter(), new FieldDefinitionConverter() };
        return JsonConvert.DeserializeObject<FieldDefinition>(data, new JsonSerializerSettings() { Converters = converters });
    }

    public class FieldDefinitionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(FieldDefinition));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            FieldDefinition defintion;

            var loaded = JObject.Load(reader);
            

            defintion = FieldDefinition.Factory(new Guid(loaded["GUID"].Value<string>()));

            serializer.Populate(loaded.CreateReader(), defintion);

            return defintion;
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

    public class ProperySetCoverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(PropertySetCollider));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            JObject jo = JObject.Load(reader);

            switch (jo["CollisionType"].Value<int>())
            {
                case 0:
                    return jo.ToObject<PropertySet.BoxCollider>(serializer);
                case 1:
                    return jo.ToObject<PropertySet.SphereCollider>(serializer);
                case 2:
                    return jo.ToObject<PropertySet.MeshCollider>(serializer);
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
    /// Writes out the properties file in XML format for the node with the base provided to
    /// the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fieldDefinition"></param>
    public static void WriteProperties(string path, FieldDefinition fieldDefinition)
    {
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

        fieldDefinition.mesh = null;

        string toWrite = JsonConvert.SerializeObject(fieldDefinition, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText(path, toWrite);
    }
}
