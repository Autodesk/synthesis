using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using Newtonsoft.Json;
namespace FieldExporterTests
{
    [TestFixture]
    class JsonTests
    {
        public readonly string FIELD_FOLDER = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\Synthesis\\Fields\\";
        [Test]
        public void SaveJson()
        {
            String path = getDirectory("TestField");
            FieldDefinition newDef = GenerateDefintion();
            BXDFPropertiesJson.WriteProperties(path + "/test_SaveJson.json", newDef);

            string data = File.ReadAllText(path + "/test_SaveJson.json");

            Assert.Greater(data.Length, 50);
        }


        [Test]
        public void ReadJson()
        {
            String path = getDirectory("TestField");
            FieldDefinition original = GenerateDefintion();
            BXDFPropertiesJson.WriteProperties(path + "/test_ReadJson.json", original);

            FieldDefinition loaded = BXDFPropertiesJson.ReadProperties(path + "/test_ReadJson.json");
            
            Assert.AreEqual(loaded.GUID.ToString(), original.GUID.ToString());
            Assert.AreEqual(loaded.rotationOffset, original.rotationOffset);
            Assert.AreEqual(loaded.propertySets.Count, loaded.propertySets.Count);
            Assert.AreEqual(loaded.propertySets["pro-1"], loaded.propertySets["pro-1"]);
        }

        [Test]
        public void TestLocal()
        {


            FieldDefinition loaded = BXDFPropertiesJson.ReadProperties("C:\\Users\\Victo\\AppData\\Roaming\\Autodesk\\Synthesis\\Fields\\TestExport\\definition.json");

            Assert.NotNull(loaded);
        }

        private String getDirectory(string fieldName)
        {
            string directory = FIELD_FOLDER + fieldName ;

            // Create directory if it does not exist
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return directory;
        }
        private FieldDefinition GenerateDefintion()
        {
            FieldDefinition newDef = FieldDefinition.Factory(Guid.NewGuid(), "Test Field");


            PropertySet set1 = new PropertySet();
            set1.Friction = 10;
            set1.Mass = 12;
            set1.PropertySetID = "pro-1";
          
            newDef.AddPropertySet(set1);

            newDef.NodeGroup.AddNode(new FieldNode("node-1", "nonethinglol"));

            return newDef;
        }
    }
}
