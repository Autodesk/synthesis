using Inventor;
using System;
using System.Collections;

namespace BxDFieldExporter {
    public enum ColliderType { Sphere, Box, Mesh };
    public class FieldDataComponent {
        public ArrayList CompOccs;
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
            CompOccs = new ArrayList();
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
        public void copyToNewComponent(FieldDataComponent f) {
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
        public bool same(BrowserNodeDefinition f) {
            if (f.Label.Equals(node.Label)) {
                return true;
            }
            else {
                return false;
            }
        }

        public PropertySet ToLegacy()
        {
            PropertySet.PropertySetCollider col = null;

            switch (colliderType)
            {
                case ColliderType.Sphere:
                    col = new PropertySet.SphereCollider((float)Scale);
                    break;
                case ColliderType.Box:
                    col = new PropertySet.BoxCollider(new BXDVector3(X, Y, Z));
                    break;
                case ColliderType.Mesh:
                    col = new PropertySet.MeshCollider(false);
                    break;
            }
            if (Dynamic)
                return new PropertySet(Name, col, (int)Friction, (float)Mass);
            else
                return new PropertySet(Name, col, (int)Friction);
        }
    }
}
