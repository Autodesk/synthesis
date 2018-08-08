using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Stores physical properties for a node or group of nodes.
/// </summary>
public struct PropertySet
{
    /// <summary>
    /// Stores collider information for a PropertySet.
    /// </summary>
    public class PropertySetCollider
    {
        /// <summary>
        /// Used for defining the type of collision for the PropertySetCollider.
        /// </summary>
        public enum PropertySetCollisionType
        {
            /// <summary>
            /// Used for approximating collision boundaries with a box.
            /// </summary>
            BOX,

            /// <summary>
            /// Used for approximating collision boundaries with a sphere.
            /// </summary>
            SPHERE,

            /// <summary>
            /// Used for taking the object's visible mesh and simplifying it for collision.
            /// </summary>
            MESH
        }

        /// <summary>
        /// Stores the type of collision for the PropertySetCollider
        /// </summary>
        public PropertySetCollisionType CollisionType
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PropertySetCollider class.
        /// </summary>
        /// <param name="collisionType"></param>
        public PropertySetCollider(PropertySetCollisionType collisionType)
        {
            CollisionType = collisionType;
        }
    }

    public class BoxCollider : PropertySetCollider
    {
        /// <summary>
        /// The scale of the BoxCollider.
        /// </summary>
        public BXDVector3 Scale
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the BoxCollider class.
        /// </summary>
        public BoxCollider(BXDVector3 scale)
            : base(PropertySetCollider.PropertySetCollisionType.BOX)
        {
            Scale = scale;
        }
    }

    public class SphereCollider : PropertySetCollider
    {
        /// <summary>
        /// The scale of the SphereCollider.
        /// </summary>
        public float Scale
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SphereCollider class.
        /// </summary>
        public SphereCollider(float scale)
            : base(PropertySetCollisionType.SPHERE)
        {
            Scale = scale;
        }
    }

    public class MeshCollider : PropertySetCollider
    {
        /// <summary>
        /// Determines whether or not the MeshCollider is convex.
        /// </summary>
        public bool Convex
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the MeshCollider class.
        /// </summary>
        public MeshCollider(bool convex)
            : base(PropertySetCollisionType.MESH)
        {
            Convex = convex;
        }
    }

    /// <summary>
    /// ID of the PropertySet.
    /// </summary>
    public string PropertySetID;

    /// <summary>
    /// Collider of the PhysicsGroup.
    /// </summary>
    public PropertySetCollider Collider;

    /// <summary>
    /// Friction value of the PhysicsGroup.
    /// </summary>
    public int Friction;

    /// <summary>
    /// Stores the mass of the object.
    /// </summary>
    public float Mass;

    /// <summary>
    /// Constructs a new PhysicsGroup with the specified values.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="type"></param>
    /// <param name="frictionValue"></param>
    public PropertySet(string physicsGroupID, PropertySetCollider collider, int friction, float mass = 0.0f)
    {
        this.PropertySetID = physicsGroupID;
        this.Collider = collider;
        this.Friction = friction;
        this.Mass = mass;
    }

    /// <summary>
    /// Stores joint information for a PropertySet
    /// </summary>
    public class PropertySetJoint
    {
        /// <summary>
        /// Used to define the joint type of the Property Set
        /// </summary>
        public enum PropertySetJointType
        {
            /// <summary>
            /// Used to identify the joint as rotational - it will rotatate around an axis
            /// </summary>
            ROTATIONAL,

            /// <summary>
            /// Used to identify the joint as Linear - it will slide forward and back along an axis
            /// </summary>
            LINEAR

            //TODO: Add Planar, Cylindrical and Ball joints
        }

        /// <summary>
        /// Stores the Joint type for PropertySetJoint
        /// </summary>
        public PropertySetJointType JointType
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PropertySetJoint class
        /// </summary>
        /// <param name="jointType"></param>
        public PropertySetJoint(PropertySetJointType jointType)
        {
            JointType = jointType;
        }
    }

    public class Rotational_Joint : PropertySetJoint
    {
        /// <summary>
        /// Initializes a new instance of the class Rotational_Joint
        /// </summary>
        public Rotational_Joint() : base(PropertySetJointType.ROTATIONAL)
        {

        }      
    }

    public class Linear_Joint : PropertySetJoint
    {

        public Linear_Joint() : base(PropertySetJointType.ROTATIONAL)
        {

        }

    }
}