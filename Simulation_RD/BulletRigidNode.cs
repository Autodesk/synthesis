using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;

namespace Simulation_RD
{
    class BulletRigidNode : RigidNode_Base
    {
        /// <summary>
        /// Defines collision mesh. Is not a <see cref="BulletSharp.RigidBody"/> because this might be a soft body
        /// </summary>
        public CollisionObject BulletObject;

        public BulletRigidNode(Guid guid) : base(guid) { }

        public void CreateMesh(string FilePath)
        {
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(FilePath);

            CompoundShape shape = new CompoundShape();

            for(int i = 0; i < mesh.colliders.Count; i++)
            {
                BXDAMesh.BXDASubMesh sub = mesh.colliders[i];

                Vector3[] vertices = MeshUtilities.DataToVector(sub.verts);

                TriangleMesh tmesh = new TriangleMesh();
                
                foreach(BXDAMesh.BXDASurface surf in sub.surfaces)
                    for(int j = 0; j < surf.indicies.Length; j += 3)
                    {
                        tmesh.AddTriangle(
                            vertices[surf.indicies[j]],
                            vertices[surf.indicies[j + 1]],
                            vertices[surf.indicies[j + 2]]
                            );
                    }

                //I don't believe there are any transformations necessary here.
                shape.AddChildShape(Matrix4.Zero, new BvhTriangleMeshShape(tmesh, true));
                Console.WriteLine("Successfully created and added sub shape");
            }

            DefaultMotionState m = new DefaultMotionState();
            RigidBodyConstructionInfo ci = new RigidBodyConstructionInfo(mesh.physics.mass, m, shape);
            BulletObject = new RigidBody(ci);
        }
    }
}
