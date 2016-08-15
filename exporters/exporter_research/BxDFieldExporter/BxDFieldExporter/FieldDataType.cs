using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDFieldExporter
{
    public enum ColliderType { Sphere, Box, Mesh };
    public class FieldDataType
    {
        public ArrayList compOcc;
        public ColliderType colliderType;
        public BrowserNodeDefinition node;
        public double X;
        public double Y;
        public double Z;
        public double Scale;
        public double Friction;
        public bool Dynamic;
        public double Mass;
        public string Name;
        public FieldDataType(BrowserNodeDefinition f)
        {
            compOcc = new ArrayList();
            Name = f.Label;
            node = f;
            colliderType = ColliderType.Box;
            X = 1;
            Y = 1;
            Z = 1;
            Scale = 1;
            Friction = 50;
            Dynamic = false;
            Mass = 0;
            
        }
        public void copyToNewType(FieldDataType f)
        {
            f.colliderType = this.colliderType;
            f.X = this.X;
            f.Y = this.Y;
            f.Z = this.Z;
            f.Scale = this.Scale;
            f.Friction = this.Friction;
            f.Dynamic = this.Dynamic;
            f.Mass = this.Mass;
        }
        public bool same(BrowserNodeDefinition f)
        {
            if (f.Label.Equals(node.Label))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
