using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

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
        ins.itemDB.xmlList.Clear();
        foreach (MaMPreset var in itemDB.xmlList)
        {
            Debug.Log("presets list" + var.GetName());
        }
            
    }

    // Update is called once per frame
    void Update()
    {

    }

    //list of items
    public ItemDatabase itemDB;

    //save function
    public void SaveItems()
    {
        //open a new xml file
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase));
        FileStream stream = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Create);

       // XmlTextWriter writer = new XmlTextWriter(Application.persistentDataPath + "/item_data.xml", new XmlWriterSettings { Encoding = Encoding.Unicode, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment, Indent = true, NewLineOnAttributes = false });
        //Taking info from unity class and putting it into XML file
        serializer.Serialize(stream, itemDB);
        stream.Close();
    }

    //load function
    public void LoadItems()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase));
        FileStream stream2 = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Open);
      
        //TO DO Check for file
        itemDB = (ItemDatabase)serializer.Deserialize(stream2);
        stream2.Close();
    }
}

[System.Serializable]
public class ItemDatabase
{
    public List<MaMPreset> xmlList = new List<MaMPreset>();
}
