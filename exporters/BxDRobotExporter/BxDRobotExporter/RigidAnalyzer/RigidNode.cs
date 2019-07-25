using System;
using System.Collections.Generic;
using BxDRobotExporter.OGLViewer;
using Inventor;

namespace BxDRobotExporter.RigidAnalyzer
{
    public class RigidNode : OGL_RigidNode 
    {
        public delegate void DeferredCalculation(RigidNode node);

        public CustomRigidGroup group;

        public RigidNode(Guid guid)
            : base(guid)
        {
            group = null;
        }

        public RigidNode(Guid guid, CustomRigidGroup grp)
            : base(guid)
        {
            this.group = grp;
        }

        public override object GetModel()
        {
            return group;
        }

        public override string GetModelID()
        {
            // Compile a model ID
            List<string> components = new List<string>();
            if (group == null)
            {
                components.Add(GetHashCode().ToString());
            }
            else
            {
                foreach (ComponentOccurrence oc in group.occurrences)
                {
                    if (oc != null)
                    {// prevents weird hidden components from ruining our day-
                        components.Add(oc.Name);
                    }
                }
            }

            string id = "";
            for (int i = 0; i < components.Count; i++)
            {
                id += components[i];

                // Act as separators
                if (i < components.Count - 1)
                    id += "-_-";
            }
            return id;
        }
    }
}