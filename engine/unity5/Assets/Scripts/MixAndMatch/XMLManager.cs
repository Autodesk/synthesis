using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Synthesis.MixAndMatch
{
    public class XMLManager : MonoBehaviour
    {
        public static XMLManager ins;

        private void Awake()
        {
            ins = this;
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //list of items
        public ItemDatabase itemDB;

        //Save Items in the List to an XML file
        public void SaveItems()
        {
            //open a new xml file
            XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase));
            FileStream stream = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Create);

            //Taking info from unity class and putting it into XML file
            serializer.Serialize(stream, itemDB);
            stream.Close();
        }

        //Loads items from an XML file to the list
        public void LoadItems()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase));
            FileStream stream2 = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Open);

            //TO DO Check for file
            ins.itemDB = (ItemDatabase)serializer.Deserialize(stream2);
            stream2.Close();
        }
    }

    [System.Serializable]
    public class ItemDatabase
    {
        public List<MaMPreset> xmlList = new List<MaMPreset>();
    }
}
