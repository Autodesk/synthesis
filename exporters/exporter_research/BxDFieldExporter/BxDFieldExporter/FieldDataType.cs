using Inventor;
using System;
using System.Collections;

namespace BxDFieldExporter {
    public enum ColliderType { Box, Sphere, Mesh };
    public class FieldDataComponent {
        public ArrayList compOcc;
        public ColliderType colliderType;
        public BrowserNodeDefinition node;
        public double X;
        public double Y;
        public double Z;
        public double Scale;
        public double Friction;
        public bool Dynamic;
        public uint ID;
        public double Mass;
        public String Name;
        public FieldDataComponent(BrowserNodeDefinition f, uint ID) {
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
            this.ID = ID;
        }
        public void CopyToNewComponent(FieldDataComponent f) {
            f.colliderType = colliderType;
            f.X = X;
            f.Y = Y;
            f.Z = Z;
            f.Scale = Scale;
            f.Friction = Friction;
            f.Dynamic = Dynamic;
            f.Mass = Mass;
            f.ID = ID;
        }
        public bool Same(BrowserNodeDefinition f) {
            if (f.Label.Equals(node.Label)) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
