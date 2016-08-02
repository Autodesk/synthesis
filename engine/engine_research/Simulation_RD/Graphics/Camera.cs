using OpenTK;
using System;

namespace Simulation_RD.Graphics
{
    /// <summary>
    /// All the different ways the camera can move
    /// </summary>
    enum Camera_Movement
    {
        forward, 
        backward, 
        left, 
        right
    };
    
    /// <summary>
    /// Controls camera movement and matrix calculations
    /// </summary>
    class Camera
    {
        public float Yaw = -90.0f;
        public float Pitch = 0.0f;
        public float Movespeed = 75f;
        public float Sensitivity = 0.75f;
        public float Zoom = 45.0f;

        Vector3 Position;
        Vector3 Front;
        Vector3 Up;
        Vector3 Right;
        Vector3 WorldUp;

        /// <summary>
        /// Creates a camera at the explained position
        /// </summary>
        /// <param name="posX">X world location</param>
        /// <param name="posY">Y world location</param>
        /// <param name="posZ">Z world location</param>
        /// <param name="upX">idk</param>
        /// <param name="upY">idk</param>
        /// <param name="upZ">idk</param>
        /// <param name="yaw">rotation</param>
        /// <param name="pitch">rotation</param>
        public Camera(float posX, float posY, float posZ, float upX, float upY, float upZ, float yaw, float pitch)
        {
            this.Position = new Vector3(posX, posY, posZ);
            this.WorldUp = new Vector3(upX, upY, upZ);
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.UpdateCameraVectors();
        }

        /// <summary>
        /// Gets the camera matrix. Uses values calculated from <see cref="UpdateCameraVectors"/>
        /// </summary>
        /// <returns></returns>
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(this.Position, this.Position + this.Front, this.Up);
        }

        /// <summary>
        /// Moves the camera in a direcion
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="deltaTime"></param>
        public void ProcessKeyboard(Camera_Movement direction, float deltaTime)
        {
            float velocity = this.Movespeed * deltaTime;
            if (direction == Camera_Movement.forward)
                this.Position += this.Front * velocity;
            if (direction == Camera_Movement.backward)
                this.Position -= this.Front * velocity;
            if (direction == Camera_Movement.left)
                this.Position -= this.Right * velocity;
            if (direction == Camera_Movement.right)
                this.Position += this.Right * velocity;
        }

        /// <summary>
        /// Process angle changes from mouse movement
        /// </summary>
        /// <param name="xoffset"></param>
        /// <param name="yoffset"></param>
        /// <param name="constrainPitch">Stops the camera from rolling over</param>
        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= this.Sensitivity;
            yoffset *= this.Sensitivity;
            
            this.Yaw += xoffset;
            this.Pitch += yoffset;

            if (constrainPitch)
            {
                if (this.Pitch > Math.PI / 2)
                    this.Pitch = (float)Math.PI / 2;
                if (this.Pitch < -Math.PI / 2)
                    this.Pitch = -(float)Math.PI / 2;
            }

            this.UpdateCameraVectors();
        }

        /// <summary>
        /// zooms the camera (does not work?)
        /// </summary>
        /// <param name="yoffset"></param>
        public void ProcessMouseScroll(float yoffset)
        {
            if (this.Zoom >= 1.0f && this.Zoom <= 45.0f)
                this.Zoom -= yoffset;
            if (this.Zoom <= 1.0f)
                this.Zoom = 1.0f;
            if (this.Zoom >= 45.0f)
                this.Zoom = 45.0f;
        }

        /// <summary>
        /// Recalculates all matrix stuff
        /// </summary>
        public void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = (float)Math.Cos(Yaw) * (float)Math.Cos(Pitch);
            front.Y = (float)Math.Sin(Pitch);
            front.Z = (float)Math.Sin(Yaw) * (float)Math.Cos(Pitch);
            Front = front.Normalized();
            Right = Vector3.Cross(Front, WorldUp).Normalized();
            Up = Vector3.Cross(Right, Front).Normalized();
        }
    }
}
