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
    }

    public class MixAndMatchPartData {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        
        public MixAndMatchConnectionPoint[] ConnectionPoints;
        
        public MixAndMatchConnectionPoint ConnectedPoint;
        
        // TODO: figure out how to handle part rotations
        // TODO: store which mesh this corresponds to without just using the index
        public int PartIndex;

        public MixAndMatchPartData(Vector3 localPosition, Quaternion localRotation, MixAndMatchConnectionPoint[] connectionPoints, MixAndMatchConnectionPoint connectedPoint = null) {
            LocalPosition = localPosition;
            ConnectionPoints = connectionPoints;
            ConnectedPoint = connectedPoint;
            LocalRotation = localRotation;

            connectionPoints.ForEach(point => point.ParentPart = this);
        }
    }

    public class MixAndMatchConnectionPoint {
        public Vector3 LocalPosition;
        public Vector3 Normal;

        public MixAndMatchPartData ParentPart;

        public MixAndMatchConnectionPoint(Vector3 localPosition, Vector3 normal) {
            LocalPosition = localPosition;
            Normal = normal;
        }
    }
}