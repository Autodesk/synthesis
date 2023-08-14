#define DEBUG_MIRABUF

using System.Collections.Generic;
using UnityEngine;
using Mirabuf;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;
using Google.Protobuf;
using Mirabuf.Joint;
using Mirabuf.Material;
using Utilities;
using Logger              = SynthesisAPI.Utilities.Logger;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using UVector3            = UnityEngine.Vector3;

namespace Synthesis.Import {
    public class MirabufLive {
        public const UInt32 CURRENT_MIRA_EXPORTER_VERSION = 5;
        public const UInt32 OLDEST_MIRA_EXPORTER_VERSION  = 4;

        private readonly string _path;
        public string MiraPath => _path;
        private Assembly _miraAssembly;
        public Assembly MiraAssembly => _miraAssembly;

        private readonly Task<RigidbodyDefinitions> _findDefinitions;

        public RigidbodyDefinitions Definitions {
            get {
                if (_findDefinitions.Status < TaskStatus.RanToCompletion) {
                    _findDefinitions.Wait();
                }

                return _findDefinitions.Result;
            }
        }

        public MirabufLive(string path) {
            _path = path;

            Load();

            _findDefinitions = Task<RigidbodyDefinitions>.Factory.StartNew(
                () => MirabufDefinitionHelper.FindRigidbodyDefinitions(this));
        }

        public static MirabufLive OpenMirabufFile(string path) => MirabufCache.Get(path);

        private void Load() {
            byte[] buff = File.ReadAllBytes(_path);

            // Check if data is compressed, and if so decompress it
            if (buff[0] == 0x1f && buff[1] == 0x8b) {
                int originalLength = BitConverter.ToInt32(buff, buff.Length - 4);

                MemoryStream mem        = new MemoryStream(buff);
                byte[] res              = new byte[originalLength];
                GZipStream decompresser = new GZipStream(mem, CompressionMode.Decompress);
                decompresser.Read(res, 0, res.Length);
                decompresser.Close();
                mem.Close();
                buff = res;
            }

            _miraAssembly = Assembly.Parser.ParseFrom(buff);
        }

        public void Save() {
            if (_miraAssembly == null) {
                File.Delete(_path);
                return;
            }

            byte[] buff = new byte[_miraAssembly.CalculateSize()];
            _miraAssembly.WriteTo(new CodedOutputStream(buff));
            string backupPath = $"{_path}.bak";
            File.Delete(backupPath);
            File.Move(_path, backupPath);
            File.WriteAllBytes(_path, buff);
        }

        public Dictionary<string, GameObject> GenerateDefinitionObjects(GameObject assemblyContainer,
            bool rigidbodies = true, bool colliders = true, bool useIndex = false, int partIndex = 0,
            int dynamicLayer = -1) {
            Dictionary<string, GameObject> groupObjects = new Dictionary<string, GameObject>();

            if (rigidbodies && !colliders) {
                Logger.Log("Cannot generate definition objects with rigidbodies and colliders", LogLevel.Error);
                throw new Exception();
            }

            if ((colliders) && _miraAssembly.Dynamic) {
                if (dynamicLayer == -1)
                    dynamicLayer = DynamicLayerManager.NextRobotLayer;

                assemblyContainer.layer = dynamicLayer;
                assemblyContainer.AddComponent<DynamicLayerReserver>();
            }

            foreach (var group in Definitions.Definitions.Values) {
                GameObject groupObject = new GameObject(useIndex ? $"{group.Name}_{partIndex}" : group.Name);
                var isGamepiece        = group.IsGamepiece;
                var isStatic           = group.IsStatic;
                // Import Parts

#region Parts

                foreach (var part in group.Parts) {
                    var partInstance   = part.Value;
                    var partDefinition = _miraAssembly.Data.Parts.PartDefinitions[partInstance.PartDefinitionReference];
                    GameObject partObject =
                        new GameObject(useIndex ? $"{partInstance.Info.Name}_{partIndex}" : partInstance.Info.Name);

                    MirabufDefinitionHelper.MakePartDefinition(partObject, partDefinition, partInstance,
                        _miraAssembly.Data,
                        !colliders ? MirabufDefinitionHelper.ColliderGenType.NoCollider
                                   : (isStatic ? MirabufDefinitionHelper.ColliderGenType.Concave
                                               : MirabufDefinitionHelper.ColliderGenType.Convex),
                        useIndex, partIndex);
                    partObject.transform.parent        = groupObject.transform;
                    var gt                             = partInstance.GlobalTransform.UnityMatrix;
                    partObject.transform.localPosition = gt.GetPosition();
                    partObject.transform.localRotation = gt.rotation;
                }

                groupObject.transform.parent = assemblyContainer.transform;

#endregion

                if (!_miraAssembly.Dynamic && !isGamepiece) {
                    groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(
                        x => x.gameObject.layer = dynamicLayer = DynamicLayerManager.FieldLayer);
                } else if (_miraAssembly.Dynamic && colliders) {
                    groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(
                        x => x.gameObject.layer = dynamicLayer);
                }

                if (rigidbodies && colliders) {
                    // Combine all physical data for grouping
                    var rb = groupObject.AddComponent<Rigidbody>();
                    if (isStatic)
                        rb.isKinematic = true;
                    rb.mass         = (float) group.CollectivePhysicalProperties.Mass;
                    rb.centerOfMass = group.CollectivePhysicalProperties.Com; // I actually don't need to flip this
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }

                groupObjects.Add(group.GUID, groupObject);
            }

            return groupObjects;
        }

        /// <summary>
        /// Collection of parts that move together
        /// </summary>
        public struct RigidbodyDefinition {
            public string GUID;
            public string Name;
            public bool IsGamepiece;
            public bool IsStatic;
            public MPhysicalProperties CollectivePhysicalProperties;
            public Dictionary<string, PartInstance> Parts; // Using a dictionary to make Key searches faster
        }

        public struct RigidbodyDefinitions {
            public float Mass;
            public Dictionary<string, RigidbodyDefinition> Definitions;
            public Dictionary<string, string> PartToDefinitionMap;
        }
    }
}