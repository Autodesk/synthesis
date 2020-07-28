using Controller.Rpc;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Runtime;

namespace Controller
{
    public static class ControllerApi
    {
        [RpcMethod]
        public static void Log(object o, LogLevel logLevel = LogLevel.Info)
        {
            ApiProvider.Log(o, logLevel);
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
        public static void SetPosition(uint channel, Vector3D position)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position = position;
            }
        }

        [RpcMethod]
        public static void RotateEulerAngles(uint channel, Vector3D rotation)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Rotate(rotation);
            }
        }

        #endregion
    }
}
