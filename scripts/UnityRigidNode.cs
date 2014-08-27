using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnityRigidNode : RigidNode_Base
{
    public GameObject unityObject, wCollider;
    protected Joint joint;
    protected WheelDriverMeta wheel;
    private PhysicalProperties bxdPhysics;
    private float center, current;
    public MeshCollider meshCollider = new MeshCollider();


    public bool IsWheel
    {
        get
        {
            return (wheel != null && wheel.type != WheelType.NOT_A_WHEEL);
        }
    }

    //public delegate void Action(); //reminder of how action and function work

    //The root transform for the whole object model is determined in this constructor passively
    public void CreateTransform(Transform root)
    {
        unityObject = new GameObject();
        unityObject.transform.parent = root;
        unityObject.transform.position = new Vector3(0, 0, 0);
        unityObject.name = base.modelFileName;
    }

    //creates a uniform configurable joint which can be altered through conditionals.
    private T ConfigJointInternal<T>(Vector3 pos, Vector3 axis, Action<T> jointType) where T : Joint
    {

        GameObject rigid = ((UnityRigidNode) GetParent()).unityObject;
        if (!rigid.gameObject.GetComponent<Rigidbody>())
        {
            rigid.gameObject.AddComponent<Rigidbody>();
        }
        Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody>();
        joint = unityObject.gameObject.AddComponent<T>();

        joint.connectedBody = rigidB;

        //configures the joint
        joint.anchor = pos;
        joint.connectedAnchor = pos;

        axis.Normalize();
        joint.axis = axis;


        //joint.secondaryAxis = new Vector3 (0, 0, 1);
        if (joint is ConfigurableJoint)
        {
            ConfigurableJoint cj = (ConfigurableJoint) joint;
            cj.angularXMotion = ConfigurableJointMotion.Locked;
            cj.angularYMotion = ConfigurableJointMotion.Locked;
            cj.angularZMotion = ConfigurableJointMotion.Locked;
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
        }
        jointType((T) joint);
        return (T) joint;
    }
    //creates a wheel collider and centers it on the current transform
    private void CreateWheel(RotationalJoint_Base center)
    {
        wCollider = new GameObject(unityObject.name + " Collider");

        wCollider.transform.parent = unityObject.transform;
        Vector3 anchorBase = joint.connectedAnchor;
        float centerMod = Vector3.Dot(auxFunctions.ConvertV3(wheel.center) - anchorBase, joint.axis);
        Debug.Log(unityObject.name + " mod by " + centerMod);
        wCollider.transform.localPosition = centerMod * joint.axis + anchorBase;
        wCollider.AddComponent<CapsuleCollider>();
        wCollider.GetComponent<CapsuleCollider>().radius = (wheel.radius * 1.10f) * 0.01f;
        wCollider.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(joint.axis.x, joint.axis.y, joint.axis.z));
        wCollider.GetComponent<CapsuleCollider>().height = wCollider.GetComponent<CapsuleCollider>().radius / 4f + wheel.width * 0.01f;
        wCollider.GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);
        wCollider.GetComponent<CapsuleCollider>().direction = 0;
        unityObject.AddComponent<BetterWheelCollider>().attach(this);
        //I want the grandfather to have a rigidbody

        unityObject.GetComponent<Rigidbody>().useConeFriction = true;
    }

    //converts inventor's limit information to the modular system unity uses (180/-180)
    private void AngularLimit(float[] limit)
    {
        if (limit.Length >= 3)
        {
            limit[1] = (limit[2] - limit[1]) / 2.0f;
            limit[2] = limit[1] < 0.0f ? Mathf.Abs(limit[1]) : -(limit[1]);
        }

        if (joint is ConfigurableJoint)
        {
            SoftJointLimit low = new SoftJointLimit(), high = new SoftJointLimit();
            ConfigurableJoint cj = (ConfigurableJoint) joint;
            if ((limit[2] - limit[1]) >= Mathf.Abs(360.0f))
            {
                cj.angularXMotion = ConfigurableJointMotion.Free;
                return;
            }
            else
            {
                low.limit = limit[1];
                high.limit = limit[2];
            }


            cj.lowAngularXLimit = low;
            cj.highAngularXLimit = high;
        }
        else if (joint is HingeJoint)
        {
            HingeJoint hj = (HingeJoint) joint;
            JointLimits limits = new JointLimits();
            limits.min = limit[1];
            limits.max = limit[2];
            hj.limits = limits;
        }
    }
    //finds the difference between the current position, which is always one of the two end points, then finds the difference between the two. 
    //this is then divided by 2 to find the limit for unity
    private void LinearLimit(Dictionary<string, float> limit)
    {
        center = (limit["end"] - limit["start"]) / 2.0f;
        //also sets limit properties to eliminate any shaking and twitching from the joint when it hit sthe limit
        SoftJointLimit linear = new SoftJointLimit();
        linear.limit = Mathf.Abs(center) * 0.01f;
        linear.bounciness = 1e-05f;
        linear.spring = 0f;
        linear.damper = 1e30f;
        if (joint is ConfigurableJoint)
            ((ConfigurableJoint) joint).linearLimit = linear;
    }
    //assigns motors to the appropriate joint
    private void SetXDrives()
    {
        //if the node has a joint and driver
        if (GetSkeletalJoint() != null && GetSkeletalJoint().cDriver != null)
        {
            if (GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
            {
                PneumaticDriverMeta pneum = GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneum != null)
                {
                    float psiToNMm2 = 0.00689475728f;
                    if (joint is ConfigurableJoint)
                    {
                        JointDrive drMode = new JointDrive();
                        drMode.mode = JointDriveMode.Velocity;
                        drMode.maximumForce = (psiToNMm2 * pneum.pressurePSI) * (Mathf.PI * Mathf.Pow((pneum.widthMM / 2), 2));
                        ((ConfigurableJoint) joint).xDrive = drMode;
                    }
                }
            }
            else if (GetSkeletalJoint().cDriver.GetDriveType().IsMotor())
            {
                if (joint is ConfigurableJoint)
                {
                    JointDrive drMode = new JointDrive();
                    drMode.mode = JointDriveMode.Velocity;
                    drMode.maximumForce = 100.0f;
                    ((ConfigurableJoint) joint).angularXDrive = drMode;
                }
                else if (joint is HingeJoint)
                {
                    JointMotor motor = new JointMotor();
                    motor.force = 100.0f;
                    motor.freeSpin = false;
                    ((HingeJoint) joint).motor = motor;
                    ((HingeJoint) joint).useMotor = true;
                }
            }

            if (IsWheel)
            {
                JointMotor motor = new JointMotor();
                motor.force = 0f;
                motor.freeSpin = true;
                ((HingeJoint) joint).motor = motor;
                ((HingeJoint) joint).useMotor = false;
            }
        }
    }



    //creates the configurable joint then preforms the appropriate alterations based on the joint type
    public void CreateJoint()
    {


        if (joint != null || GetSkeletalJoint() == null)
        {
            return;
        }

        //SkeletalJoint_Base GetSkeletalJoint() = GetSkeletalJoint();
        //this is the conditional for Identified wheels
        if (GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
        {
            //if the mesh contains information which identifies it as a wheel then create a wheel collider.
            wheel = GetSkeletalJoint().cDriver != null ? GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() : null;
            if (IsWheel)
                FlipNorms();

            RotationalJoint_Base nodeR = (RotationalJoint_Base) GetSkeletalJoint();

            //takes the x, y, and z axis information from a custom vector class to unity's vector class
            joint = ConfigJointInternal<HingeJoint>(auxFunctions.ConvertV3(nodeR.basePoint), auxFunctions.ConvertV3(nodeR.axis), delegate(HingeJoint jointSub)
            {
                jointSub.useLimits = nodeR.hasAngularLimit;
                if (nodeR.hasAngularLimit)
                {
                    float[] aLimit = {
											nodeR.currentAngularPosition * (180.0f / Mathf.PI),
											nodeR.angularLimitLow * (180.0f / Mathf.PI),
											nodeR.angularLimitHigh * (180.0f / Mathf.PI)
										};
                    AngularLimit(aLimit);

                }
            });
            //don't worry, I'm a doctor

            if (IsWheel)
            {
                CreateWheel(nodeR);
            }
        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
        {
            CylindricalJoint_Base nodeC = (CylindricalJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(auxFunctions.ConvertV3(nodeC.basePoint), auxFunctions.ConvertV3(nodeC.axis), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;
                jointSub.angularXMotion = !nodeC.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
                Dictionary<string, float> lLimit = new Dictionary<string, float>() {
															{"end",nodeC.linearLimitEnd},
															{"start",nodeC.linearLimitStart},
															{"current",nodeC.currentLinearPosition}
								};
                LinearLimit(lLimit);
                if (GetSkeletalJoint().cDriver != null && GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
                {
                    JointDrive drMode = new JointDrive();
                    drMode.mode = JointDriveMode.Velocity;
                    drMode.maximumForce = 100.0f;
                    jointSub.xDrive = drMode;
                }
                if (jointSub.angularXMotion == ConfigurableJointMotion.Limited)
                {
                    float[] aLimit = {
												nodeC.currentAngularPosition * (180.0f / Mathf.PI),
												nodeC.angularLimitLow * (180.0f / Mathf.PI),
												nodeC.angularLimitHigh * (180.0f / Mathf.PI)
								};
                    AngularLimit(aLimit);
                }
            });


        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
        {
            LinearJoint_Base nodeL = (LinearJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(auxFunctions.ConvertV3(nodeL.basePoint), auxFunctions.ConvertV3(nodeL.axis), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;

                Dictionary<string, float> lLimit = new Dictionary<string, float>() {
												{"end",nodeL.linearLimitHigh},
												{"start",nodeL.linearLimitLow},
												{"current",nodeL.currentLinearPosition}
						};
                LinearLimit(lLimit);
            });

        }
        SetXDrives();
    }

    //loads the bxda format meshes
    public void CreateMesh(string filePath, bool noMesh)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(filePath);

        if (noMesh)
        {
            auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
            {
                //new gameobject is made for the submesh

                GameObject subObject = new GameObject(unityObject.name + " Subpart" + id);
                //it is passively assigned as a child to the root transform 
                subObject.transform.parent = unityObject.transform;
                subObject.transform.position = new Vector3(0, 0, 0);

                subObject.AddComponent<MeshFilter>();
                subObject.GetComponent<MeshFilter>().mesh = meshu;
                subObject.AddComponent<MeshRenderer>();
                Material[] matls = new Material[meshu.subMeshCount];
                for (int i = 0; i < matls.Length; i++)
                {
                    uint val = sub.surfaces[i].hasColor ? sub.surfaces[i].color : 0xFFFFFFFF;
                    Color color = new Color32((byte) (val & 0xFF), (byte) ((val >> 8) & 0xFF), (byte) ((val >> 16) & 0xFF), (byte) ((val >> 24) & 0xFF));
                    if (sub.surfaces[i].transparency != 0)
                    {
                        color.a = sub.surfaces[i].transparency;
                    }
                    else if (sub.surfaces[i].translucency != 0)
                    {
                        color.a = sub.surfaces[i].translucency;
                    }
                    if (color.a == 0)   // No perfectly transparent things plz.
                    {
                        color.a = 1;
                    }
                    matls[i] = new Material((Shader) Shader.Find((color.a != 1 ? "Transparent/" : "") + (sub.surfaces[i].specular > 0 ? "Specular" : "Diffuse")));
                    matls[i].SetColor("_Color", color);
                    if (sub.surfaces[i].specular > 0)
                    {
                        matls[i].SetFloat("_Shininess", sub.surfaces[i].specular);
                    }
                }
                subObject.GetComponent<MeshRenderer>().materials = matls;
            });
        }

        if (!unityObject.GetComponent<Rigidbody>())
        {
            unityObject.AddComponent<Rigidbody>();
            //unityObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        auxFunctions.ReadMeshSet(mesh.colliders, delegate(int id, BXDAMesh.BXDASubMesh useless, Mesh meshu)
        {
            //Debug.Log (unityObject.name + " " + id + " tris: " + meshu.triangles.Length / 3 + " Vertices: " + meshu.vertexCount);
            GameObject subCollider = new GameObject(unityObject.name + " Subcollider" + id);
            subCollider.transform.parent = unityObject.transform;
            subCollider.transform.position = new Vector3(0, 0, 0);
            if (meshu.triangles.Length == 0 && meshu.vertices.Length == 2)
            {
                BoxCollider box = subCollider.AddComponent<BoxCollider>();
                Vector3 center = (meshu.vertices[0] + meshu.vertices[1]) * 0.5f;
                box.center = center;
                box.size = meshu.vertices[1] - center;
            }
            else
            {
                subCollider.AddComponent<MeshCollider>().sharedMesh = meshu;
                //Debug.Log(IsWheel);
                subCollider.GetComponent<MeshCollider>().convex = true;
                meshCollider = subCollider.GetComponent<MeshCollider>();
            }

        });

        Rigidbody rigidB = unityObject.GetComponent<Rigidbody>();
        rigidB.mass = mesh.physics.mass * Init.PHYSICS_MASS_MULTIPLIER; // Unity has magic mass units
        rigidB.centerOfMass = auxFunctions.ConvertV3(mesh.physics.centerOfMass);

        bxdPhysics = mesh.physics;

        #region Free mesh
        foreach (var list in new List<BXDAMesh.BXDASubMesh>[] { mesh.meshes, mesh.colliders })
        {
            foreach (BXDAMesh.BXDASubMesh sub in list)
            {
                sub.verts = null;
                sub.norms = null;
                foreach (BXDAMesh.BXDASurface surf in sub.surfaces)
                {
                    surf.indicies = null;
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = null;
            }
        }
        mesh = null;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        #endregion
    }

    public T GetJoint<T>() where T : Joint
    {
        return joint != null && joint is T ? (T) joint : null;
    }

    // Returns the center of mass of the skeleton. It calculates a weighted average of all the rigiBodies in the gameObject. (Its an average of their positions, weighted by the masses of each rigidBody)
    public static Vector3 TotalCenterOfMass(GameObject gameObj)
    {
        Vector3 centerOfMass = Vector3.zero;
        float sumOfAllWeights = 0f;

        Rigidbody[] rigidBodyArray = gameObj.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidBase in rigidBodyArray)
        {
            centerOfMass += rigidBase.worldCenterOfMass * rigidBase.mass;
            sumOfAllWeights += rigidBase.mass;
        }
        centerOfMass /= sumOfAllWeights;
        return centerOfMass;
    }

    public static BXDVector3 comLOL(RigidNode_Base kk)
    {
        if (kk is UnityRigidNode && ((UnityRigidNode) kk).bxdPhysics != null)
        {
            return ((UnityRigidNode) kk).bxdPhysics.centerOfMass;
        }
        return null;//breakit
    }

    public void FlipNorms()
    {
        if (GetParent() != null && GetSkeletalJoint() != null && GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
        {
            Vector3 com = auxFunctions.ConvertV3(comLOL(GetParent()));
            RotationalJoint_Base rJoint = (RotationalJoint_Base) GetSkeletalJoint();
            Vector3 diff = auxFunctions.ConvertV3(rJoint.basePoint) - com;
            double dot = Vector3.Dot(diff, auxFunctions.ConvertV3(rJoint.axis));
            if (dot < 0)
            {
                rJoint.axis = rJoint.axis.Multiply(-1);
            }
        }
    }


}
