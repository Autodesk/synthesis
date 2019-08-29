using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;

namespace Synthesis.RN
{
    public partial class RigidNode : RigidNode_Base
    {
        public GameObject MainObject { get; private set; }

        public Vector3 ComOffset { get; set; }

        public PhysicalProperties PhysicalProperties { get; private set; }

        public RigidNode RootNode
        {
            get
            {
                RigidNode root = this;
                RigidNode_Base parent;

                while ((parent = root.GetParent()) != null && parent is RigidNode)
                    root = parent as RigidNode;

                return root;
            }
        }

        public int WheelCount { get; private set; }

        public Vector3 WheelsNormal { get; private set; }

        private Transform root;
        private Component joint;

        public RigidNode(Guid guid)
            : base(guid)
        {
        }

        public void CreateTransform(Transform root)
        {
            MainObject = new GameObject(ModelFileName);
            MainObject.transform.parent = root;

            this.root = root;

            ComOffset = Vector3.zero;
        }
    }
}
