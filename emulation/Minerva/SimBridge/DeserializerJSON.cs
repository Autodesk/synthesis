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
}