using System;
using UnityEngine;

namespace SimObjects.MixAndMatch {
    public class MixAndMatchTransformData {
        public MixAndMatchPartData[] Parts;

        public MixAndMatchTransformData(MixAndMatchPartData[] parts) {
            Parts = parts;
            
            parts.ForEachIndex((i, p) => {
                p.PartIndex = i;
            });
        }
        
        public void SaveToJson() {
            throw new NotImplementedException();
        }
    }

    public class MixAndMatchPartData {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        
        public (Vector3 position, Vector3 normal)[] ConnectionPoints;
        
        public MixAndMatchPartData ConnectedPart;
        
        // TODO: figure out how to handle part rotations
        // TODO: store which mesh this corresponds to without just using the index
        public int PartIndex;

        public MixAndMatchPartData(Vector3 localPosition, Quaternion localRotation, (Vector3 position, Vector3 normal)[] connectionPoints) {
            LocalPosition = localPosition;
            ConnectionPoints = connectionPoints;
            LocalRotation = localRotation;
        }

        public void SaveToJson() {
            throw new NotImplementedException();
        }
    }
}