using System;
using Newtonsoft.Json;
using System.IO;

namespace SimBridge
{
    class DeserializerJSON
    {
        public void deserializeJSON()
        {
            try
            {
                //var json = JsonConvert.DeserializeObject<dynamic>(strJSON);
                using (StreamReader r = new StreamReader("file.json"))
                {
                    string strJSON = r.ReadToEnd();
                    var jItems = JsonConvert.DeserializeObject<JSONdata>(strJSON);
                }
            }
            catch(Exception ex)
            {
                string error = ex.Message.ToString();
            }
        }
    }

    public class JSONdata
    {
        public int thing1 { get; set; }
        public string thing2 { get; set; }
        public string thing3 { get; set; }
        public float thing4 { get; set; }
        public float thing5 { get; set; }

        public class subData
        {
            public int subthing1 { get; set; }
            public float subthing2 { get; set; }
            public string subthing3 { get; set; }
        }
    }
}