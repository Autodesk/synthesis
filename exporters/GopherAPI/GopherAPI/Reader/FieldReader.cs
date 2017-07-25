using System;
using System.IO;
using GopherAPI.Nodes.Colliders;

namespace GopherAPI.Reader
{
    internal class FieldReader : GopherReader_Base
    {
        internal RawField Field = new RawField();

        /// <summary>
        /// Step 3: Processes the facets of all the RawMeshes and adds the PreProcessed data into an STLMesh object
        /// </summary>
        new internal void ProcessSTL()
        {
            Field.Meshes = base.ProcessSTL();
        }

        /// <summary>
        /// Step 4: Processes the joint section 
        /// </summary>
        new internal void ProcessJoints()
        {
            Field.Joints = base.ProcessJoints();
        }

        /// <summary>
        /// Step 5: Process the collider section
        /// </summary>
        /// <param name="raw"></param>
        internal void ProcessColliders(Section raw)
        {
            if (raw.ID != SectionType.STL_ATTRIBUTE)
                throw new ArgumentException("ERROR: Non-collider section passed to ProcessColliders", "raw");
            using (var reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint ColliderCount = reader.ReadUInt32();
                for(uint i = 0; i < ColliderCount; i++)
                {
                    uint tempID = reader.ReadUInt32();
                    ColliderType ct = (ColliderType)reader.ReadUInt16();

                    switch (ct)
                    {
                        case ColliderType.BOX_COLLIDER:
                            var box = new BoxCollider
                            {
                                Meta = new ColliderMeta(ct, tempID),
                                Scale = new Other.Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                Friction = reader.ReadSingle(),
                                IsDynamic = reader.ReadBoolean()
                            };
                            if (box.IsDynamic)
                                box.Mass = reader.ReadSingle();
                            Field.Colliders.Add(box);
                            break;
                        case ColliderType.SPHERE_COLLIDER:
                            var sphere = new SphereCollider
                            {
                                Meta = new ColliderMeta(ct, tempID),
                                Scale = reader.ReadSingle(),
                                Friction = reader.ReadSingle(),
                                IsDynamic = reader.ReadBoolean()
                            };
                            if (sphere.IsDynamic)
                                sphere.Mass = reader.ReadSingle();
                            Field.Colliders.Add(sphere);
                            break;
                        case ColliderType.MESH_COLLIDER:
                            var mesh = new MeshCollider
                            {
                                Meta = new ColliderMeta(ct, tempID),
                                Friction = reader.ReadSingle(),
                                IsDynamic = reader.ReadBoolean()
                            };
                            if (mesh.IsDynamic)
                                mesh.Mass = reader.ReadSingle();
                            Field.Colliders.Add(mesh);
                            break;
                        default:
                            throw new Exception("ERROR: Unknown or bad collider type");
                    }

                }
            }

        }

        /// <summary>
        /// Step 5: Process the collider section
        /// </summary>
        internal void ProcessColliders()
        {
            foreach(var section in sections)
            {
                if (section.ID == SectionType.STL_ATTRIBUTE)
                    ProcessColliders(section);
            }
        }

        public FieldReader(string path) : base(path)
        {
            if(Path.GetExtension(path).ToLower() != ".field")
                throw new ArgumentException("ERROR: non field file passed to FieldReader", "path");
        }
    }
}