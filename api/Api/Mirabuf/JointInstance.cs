namespace Mirabuf.Joint {
    public partial class JointInstance {
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
    }
}
