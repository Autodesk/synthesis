using Controller.Rpc;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Runtime;
using SynthesisCore.Components;

namespace Controller
{
    public static class ControllerApi
    {
        [RpcMethod]
        public static void Log(string msg, LogLevel logLevel = LogLevel.Info)
        {
            ApiProvider.Log(msg, logLevel);
        }

        #region Transform movement

        [RpcMethod]
        public static void Forward(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, 0, distance);
            }
        }

        [RpcMethod]
        public static void Backward(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, 0, -distance);
            }
        }

        [RpcMethod]
        public static void Left(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(-distance, 0, 0);
            }
        }

        [RpcMethod]
        public static void Right(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(distance, 0, 0);
            }
        }

        [RpcMethod]
        public static void Up(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, distance, 0);
            }
        }

        [RpcMethod]
        public static void Down(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, -distance, 0);
            }
        }

        [RpcMethod]
        public static void SetPosition(uint channel, double x, double y, double z)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position = new Vector3D(x, y, z);
            }
        }

        [RpcMethod]
        public static void RotateEulerAngles(uint channel, double x, double y, double z)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Rotate(new Vector3D(x, y, z));
            }
        }

        #endregion
    }
}
