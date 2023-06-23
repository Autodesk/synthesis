using System;
using System.Collections.Generic;

namespace Mirabuf {
    public partial class Assembly {

        private Dictionary<string, List<string>>? _partJointMap = null;

        private void LoadPartJointMap() {
            _partJointMap = new Dictionary<string, List<string>>();
            Data.Parts.PartInstances.ForEach(x => {
                _partJointMap[x.Key] = new List<string>();
                Data.Joints.JointInstances.ForEach(y => {
                    if (y.Value.ChildPart == x.Key || y.Value.ParentPart == x.Key)
                        _partJointMap[x.Key].Add(y.Key);
                });
            });
        }
        
        public List<string> GetPartJoints(string partInstance) {
            if (_partJointMap == null)
                LoadPartJointMap();
            // The previous line should fill this I guess ?
            _partJointMap!.TryGetValue(partInstance, out List<string> joints);
            return joints;
        }
    }
}