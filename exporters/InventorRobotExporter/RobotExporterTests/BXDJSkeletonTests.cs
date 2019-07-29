using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
namespace BXDSim.IO.BXDJ
{
    [TestFixture]
    class BXDJSkeletonTests
    {
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Test\";

        private Guid id;
        public BXDJSkeletonTests()
        {
            id = new Guid();
            try { Directory.CreateDirectory(path); } catch (Exception e) { }
        }

        [Test]
        public void CreateSkeleton()
        {
            RigidNode_Base node = new RigidNode_Base(id);
            Assert.NotNull(node);
        }

        [Test]
        public void AddChildren()
        {
            RigidNode_Base baseNode = CreateNodeWithChild();

            Assert.AreEqual(baseNode.Children.Count, 1);
            Assert.IsNotNull(baseNode.Children[baseNode.Children.Keys.First<SkeletalJoint_Base>()]);
        }

        [Test]
        public void ExportJSON()
        {
            string filePath = path + "test_export.json";
            RigidNode_Base baseNode = CreateNodeWithChild();
            BXDJSkeletonJson.WriteSkeleton(filePath, baseNode);

            string result = File.ReadAllText(filePath);

            Assert.Greater(result.Length, 50);
        }
        

        [Test]
        public void ReadJSON()
        {
            string filePath = path + "test_export.json";
            RigidNode_Base toCreate = CreateNodeWithChild();
            BXDJSkeletonJson.WriteSkeleton(filePath, toCreate);


            RigidNode_Base created = BXDJSkeletonJson.ReadSkeleton(filePath);


            Assert.AreEqual(toCreate.GUID, created.GUID);
            Assert.AreEqual(toCreate.Children.Count, created.Children.Count);

            SkeletalJoint_Base firstJointtoCreate = toCreate.Children.Keys.First<SkeletalJoint_Base>();
            SkeletalJoint_Base firstJointCreated = created.Children.Keys.First<SkeletalJoint_Base>();

            Assert.AreEqual(
                toCreate.Children[firstJointtoCreate].GetSkeletalJoint().GetJointType(),
                created.Children[firstJointCreated].GetSkeletalJoint().GetJointType()
                );
            

    
        }

        //Odeza Test
        // This attempts to load the odeza bot that was fully exported from Inventor
        // If the bot is not found, this test will just pass as normal 
        // This was developed for local testing on my machine
        // - Alex

        [Test]
        public void TestOdeza()
        {
            string odezaPath = @"C:\Users\Victo\AppData\Roaming\Autodesk\Synthesis\Robots\Odesza1\\skeleton.json";
            if (!File.Exists(odezaPath))
            {
                return;
            }
            
            RigidNode_Base loaded = BXDJSkeletonJson.ReadSkeleton(odezaPath);

            Assert.IsNotNull(loaded);


        }

        // Generation Methods
        private RigidNode_Base CreateNodeWithChild()
        {
            RotationalJoint_Base joint = new RotationalJoint_Base();
            joint.axis = new BXDVector3(2, 2, 2);
            joint.basePoint = new BXDVector3(3, 131, 3);

            JointDriver driver = new JointDriver(JointDriverType.MOTOR);
           
            driver.SetPort(414);
            driver.isCan = true;

            WheelDriverMeta wheelDriverMeta = new WheelDriverMeta();
            wheelDriverMeta.radius = 9.4F;
            wheelDriverMeta.center = new BXDVector3(1, 1, 1);

            driver.AddInfo(wheelDriverMeta);

            
            joint.cDriver = driver;
       
          

            Guid childID = new Guid();
            RigidNode_Base baseNode = new RigidNode_Base(id);
            RigidNode_Base childNode = new RigidNode_Base(childID);
       
            baseNode.AddChild(joint, childNode);
            return baseNode;
        }
    }
}
