using System;

namespace SynthesisAPI.Utilities
{
    public class Toolkit
    {
        public static T DeserializeJson<T>(string jsonString)
        {
            // object result = System.Text.Json.JsonSerializer.Deserialize<T>(jsonString);
            // return (T) Convert.ChangeType(result, typeof(T));
            return default;
        }
    }
}