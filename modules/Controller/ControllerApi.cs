﻿using Controller.Rpc;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using SynthesisCore.Components;

namespace Controller
{
    public static class ControllerApi
    {
        [RpcMethod("log_str")]
        public static void LogStr(string msg, LogLevel logLevel = LogLevel.Info)
        {
            Logger.Log(msg, logLevel);
        }

        #region Transform movement

        [RpcMethod("forward")]
        public static void Forward(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, 0, distance);
            }
        }

        [RpcMethod("backward")]
        public static void Backward(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, 0, -distance);
            }
        }

        [RpcMethod("left")]
        public static void Left(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(-distance, 0, 0);
            }
        }

        [RpcMethod("right")]
        public static void Right(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(distance, 0, 0);
            }
        }

        [RpcMethod("up")]
        public static void Up(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, distance, 0);
            }
        }

        [RpcMethod("down")]
        public static void Down(uint channel, double distance)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position += new Vector3D(0, -distance, 0);
            }
        }

        [RpcMethod("set_position")]
        public static void SetPosition(uint channel, double x, double y, double z)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Position = new Vector3D(x, y, z);
            }
        }

        [RpcMethod("rotate_euler_angles")]
        public static void RotateEulerAngles(uint channel, double x, double y, double z)
        {
            foreach (var e in EnvironmentManager.GetEntitiesWhere(
                e => e.GetComponent<Moveable>()?.Channel == channel))
            {
                e.GetComponent<Transform>().Rotate(new Vector3D(x, y, z));
            }
        }

        [RpcMethod("test")]
        public static int Test(int test)
        {
            if (test == 25)
            {
                throw new System.Exception("Test exception");
            }
            return test;
        }

        #endregion
    }
}
