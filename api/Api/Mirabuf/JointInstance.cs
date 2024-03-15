namespace Mirabuf.Joint {
    public partial class JointInstance {

        public enum WheelTypeEnum {
            Standard = 0, Omni = 1
        }

        private bool? _isWheel = null;
        public bool IsWheel(Assembly assembly) {
            if (!_isWheel.HasValue) {
                _isWheel = Info.Name != "grounded"
                           && assembly.Data.Joints.JointDefinitions[JointReference].UserData != null
                           && assembly.Data.Joints.JointDefinitions[JointReference].UserData.Data
                               .TryGetValue("wheel", out var isWheel)
                           && isWheel == "true";
            }
            return _isWheel.Value;
        }

        private WheelTypeEnum? _wheelType = null;
        public WheelTypeEnum GetWheelType(Assembly assembly) {
            if (!_wheelType.HasValue) {
                if (IsWheel(assembly)) {
                    var hasValue = assembly.Data.Joints.JointDefinitions[JointReference].UserData.Data
                               .TryGetValue("wheelType", out var wheelType);
                    if (hasValue && wheelType.Equals("1")) {
                        _wheelType = WheelTypeEnum.Omni;
                    } else {
                        _wheelType = WheelTypeEnum.Standard;
                    }
                } else {
                    throw new System.Exception("Non-wheels don't have wheel types");
                }
            }

            return _wheelType.Value;
        }
    }
}
