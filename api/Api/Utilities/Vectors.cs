using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SynthesisAPI.Utilities
{
    public class Vector3
    {
        private UnityEngine.Vector3 vector_instance;

        public float X { get => vector_instance.x; set => vector_instance.x = value; }
        public float Y { get => vector_instance.y; set => vector_instance.y = value; }
        public float Z { get => vector_instance.z; set => vector_instance.z = value; }

        public Vector3(float x, float y, float z) => vector_instance = new UnityEngine.Vector3(x, y, z);

        public Vector3(UnityEngine.Vector3 vector) => vector_instance = vector;

        public static implicit operator UnityEngine.Vector3(Vector3 vec) => vec.vector_instance;
        public static implicit operator Vector3(UnityEngine.Vector3 vec) => new Vector3(vec);

        //public void Rotate(Vector3 vector)
        //{
        //    
        //}

        public void Move(Vector3 vector)
        {
            vector_instance += vector;
        }
    }

    public class Vector2
    {
        private UnityEngine.Vector2 vector_instance;

        public float X { get => vector_instance.x; set => vector_instance.x = value; }
        public float Y { get => vector_instance.y; set => vector_instance.y = value; }

        public Vector2(float x, float y) => vector_instance = new UnityEngine.Vector2(x, y);

        public Vector2(UnityEngine.Vector2 vector) => vector_instance = vector;

        public static implicit operator UnityEngine.Vector2(Vector2 vec) => vec.vector_instance;
        public static implicit operator Vector2(UnityEngine.Vector2 vec) => new Vector2(vec);

        //public void Rotate(Vector3 vector)
        //{
        //    
        //}

        public void Move(Vector2 vector)
        {
            vector_instance += vector;
        }
    }
}
