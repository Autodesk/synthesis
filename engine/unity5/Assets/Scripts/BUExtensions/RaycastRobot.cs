#define ROLLING_INFLUENCE_FIX

using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assets.Scripts.BUExtensions
{
    /// <summary>
    /// Describes info about a robot wheel.
    /// </summary>
    public class RobotWheelInfo : WheelInfo
    {
        public float SlidingFriction;
        public float Speed;
        public float FreeSpinDamping;

        public RobotWheelInfo(WheelInfoConstructionInfo ci) : base(ci)
        {
            SlidingFriction = 1.0f;
            FreeSpinDamping = 0.05f;
        }
    }

    /// <summary>
    /// This class is a modified version of RaycastVehicle to better suit how robots behave.
    /// </summary>
    public class RaycastRobot
    {
        // The higher the contactDamping, the more resistance the robot has when turning.
        const float contactDamping = 0.1f; // Original: 0.2f;

        // The higher the sideFactor, the less forward traction the robot has when sliding.
        const float sideFactor = 0.05f; // Original: 1.0f;

        // The higher the fwdFactor, the less side traction the robot has wheen accelerating.
        const float fwdFactor = 0.5f;// Original: 0.5f;

        RobotWheelInfo[] wheelInfo = new RobotWheelInfo[0];

        Vector3[] forwardWS = new Vector3[0];
        Vector3[] axle = new Vector3[0];
        float[] forwardImpulse = new float[0];
        float[] sideImpulse = new float[0];

        /// <summary>
        /// Controls the maximum wheel angular velocity.
        /// </summary>
        public float MaxWheelAngularVelocity { get; set; }

        /// <summary>
        /// The world transform of the parent chassis.
        /// </summary>
        public Matrix ChassisWorldTransform
        {
            get
            {
                return RigidBody.CenterOfMassTransform;
            }
        }

        float currentVehicleSpeedKmHour;

        /// <summary>
        /// The number of wheels associated with the RaycastRobot.
        /// </summary>
        public int NumWheels
        {
            get { return wheelInfo.Length; }
        }

        int indexRightAxis = 0;

        /// <summary>
        /// The relative right axis of the robot.
        /// </summary>
        public int RightAxis
        {
            get { return indexRightAxis; }
        }

        int indexUpAxis = 2;
        int indexForwardAxis = 1;

        RigidBody chassisBody;

        /// <summary>
        /// The <see cref="BulletSharp.RigidBody"/> associated with the parent chassis.
        /// </summary>
        public RigidBody RigidBody
        {
            get { return chassisBody; }
        }

        /// <summary>
        /// This property can be used to override the parent chassis mass used for calculations.
        /// </summary>
        public float OverrideMass { get; set; }

        /// <summary>
        /// This property can be used to override the parent chassis <see cref="BulletSharp.RigidBody"/> for calculations.
        /// </summary>
        public RigidBody RootRigidBody { get; set; }

        IVehicleRaycaster vehicleRaycaster;

        static RigidBody fixedBody;

        /// <summary>
        /// Sets the brake value for the given wheel.
        /// </summary>
        /// <param name="brake"></param>
        /// <param name="wheelIndex"></param>
        public void SetBrake(float brake, int wheelIndex)
        {
            Debug.Assert((wheelIndex >= 0) && (wheelIndex < NumWheels));
            GetWheelInfo(wheelIndex).Brake = brake;
        }

        /// <summary>
        /// Returns the steering value for the given wheel.
        /// </summary>
        /// <param name="wheel"></param>
        /// <returns></returns>
        public float GetSteeringValue(int wheel)
        {
            return GetWheelInfo(wheel).Steering;
        }

        /// <summary>
        /// Sets the steering value for the given wheel.
        /// </summary>
        /// <param name="steering"></param>
        /// <param name="wheel"></param>
        public void SetSteeringValue(float steering, int wheel)
        {
            Debug.Assert(wheel >= 0 && wheel < NumWheels);

            WheelInfo wheelInfo = GetWheelInfo(wheel);
            wheelInfo.Steering = steering;
        }

        /// <summary>
        /// Defines the coordinate system for the robot.
        /// </summary>
        /// <param name="rightIndex"></param>
        /// <param name="upIndex"></param>
        /// <param name="forwardIndex"></param>
        public void SetCoordinateSystem(int rightIndex, int upIndex, int forwardIndex)
        {
            indexRightAxis = rightIndex;
            indexUpAxis = upIndex;
            indexForwardAxis = forwardIndex;
        }

        /// <summary>
        /// Returns the wheel transform <see cref="Matrix"/> for the given wheel.
        /// </summary>
        /// <param name="wheelIndex"></param>
        /// <returns></returns>
        public Matrix GetWheelTransformWS(int wheelIndex)
        {
            Debug.Assert(wheelIndex < NumWheels);
            return wheelInfo[wheelIndex].WorldTransform;
        }

        /// <summary>
        /// Initializes the static RaycastRobot instance.
        /// </summary>
        static RaycastRobot()
        {
            using (var ci = new RigidBodyConstructionInfo(0, null, null))
            {
                fixedBody = new RigidBody(ci);
                fixedBody.SetMassProps(0, Vector3.Zero);
            }
        }

        /// <summary>
        /// Initializes the RaycastRobot with the given vehicle settings and parent chassis.
        /// </summary>
        /// <param name="tuning"></param>
        /// <param name="chassis"></param>
        /// <param name="raycaster"></param>
        public RaycastRobot(VehicleTuning tuning, RigidBody chassis, IVehicleRaycaster raycaster)
        {
            chassisBody = chassis;
            RootRigidBody = chassis;
            vehicleRaycaster = raycaster;

            MaxWheelAngularVelocity = 40f;
            OverrideMass = 1.0f / chassis.InvMass;
        }

        /// <summary>
        /// Adds a wheel to the RaycastRobot.
        /// </summary>
        /// <param name="connectionPointCS"></param>
        /// <param name="wheelDirectionCS0"></param>
        /// <param name="wheelAxleCS"></param>
        /// <param name="suspensionRestLength"></param>
        /// <param name="wheelRadius"></param>
        /// <param name="tuning"></param>
        /// <param name="isFrontWheel"></param>
        /// <returns></returns>
        public RobotWheelInfo AddWheel(Vector3 connectionPointCS, Vector3 wheelDirectionCS0, Vector3 wheelAxleCS,
            float suspensionRestLength, float wheelRadius, VehicleTuning tuning, bool isFrontWheel)
        {
            WheelInfoConstructionInfo ci = new WheelInfoConstructionInfo
            {
                ChassisConnectionCS = connectionPointCS,
                WheelDirectionCS = wheelDirectionCS0,
                WheelAxleCS = wheelAxleCS,
                SuspensionRestLength = suspensionRestLength,
                WheelRadius = wheelRadius,
                SuspensionStiffness = tuning.SuspensionStiffness,
                WheelsDampingCompression = tuning.SuspensionCompression,
                WheelsDampingRelaxation = tuning.SuspensionDamping,
                FrictionSlip = tuning.FrictionSlip,
                IsFrontWheel = isFrontWheel,
                MaxSuspensionTravelCm = tuning.MaxSuspensionTravelCm,
                MaxSuspensionForce = tuning.MaxSuspensionForce
            };

            Array.Resize(ref wheelInfo, wheelInfo.Length + 1);
            RobotWheelInfo wheel = new RobotWheelInfo(ci);
            wheelInfo[wheelInfo.Length - 1] = wheel;

            UpdateWheelTransformsWS(wheel, false);
            UpdateWheelTransform(NumWheels - 1, false);
            return wheel;
        }

        /// <summary>
        /// Applies the provided force to the given wheel.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="wheel"></param>
        public void ApplyEngineForce(float force, int wheel)
        {
            Debug.Assert(wheel >= 0 && wheel < NumWheels);
            WheelInfo wheelInfo = GetWheelInfo(wheel);
            wheelInfo.EngineForce = force;
        }

        /// <summary>
        /// Calculates rolling friction for a wheel.
        /// </summary>
        /// <param name="body0"></param>
        /// <param name="body1"></param>
        /// <param name="contactPosWorld"></param>
        /// <param name="frictionDirectionWorld"></param>
        /// <param name="maxImpulse"></param>
        /// <returns></returns>
        float CalcRollingFriction(RigidBody body0, RigidBody body1, Vector3 contactPosWorld, Vector3 frictionDirectionWorld, float maxImpulse)
        {
            float denom0 = body0.ComputeImpulseDenominator(contactPosWorld, frictionDirectionWorld);
            float denom1 = body1.ComputeImpulseDenominator(contactPosWorld, frictionDirectionWorld);
            const float relaxation = 1.0f;
            float jacDiagABInv = relaxation / (denom0 + denom1);

            float j1;

            Vector3 rel_pos1 = contactPosWorld - body0.CenterOfMassPosition;
            Vector3 rel_pos2 = contactPosWorld - body1.CenterOfMassPosition;

            Vector3 vel1 = body0.GetVelocityInLocalPoint(rel_pos1);
            Vector3 vel2 = body1.GetVelocityInLocalPoint(rel_pos2);
            Vector3 vel = vel1 - vel2;

            float vrel;
            Vector3.Dot(ref frictionDirectionWorld, ref vel, out vrel);

            // calculate j that moves us to zero relative velocity
            j1 = -vrel * jacDiagABInv;
            j1 = System.Math.Min(j1, maxImpulse);
            j1 = System.Math.Max(j1, -maxImpulse);

            return j1;
        }

        Vector3 blue = new Vector3(0, 0, 1);
        Vector3 magenta = new Vector3(1, 0, 1);
        public void DebugDraw(IDebugDraw debugDrawer)
        {
            for (int v = 0; v < NumWheels; v++)
            {
                WheelInfo wheelInfo = GetWheelInfo(v);

                Vector3 wheelColor;
                if (wheelInfo.RaycastInfo.IsInContact)
                {
                    wheelColor = blue;
                }
                else
                {
                    wheelColor = magenta;
                }

                Matrix transform = wheelInfo.WorldTransform;
                Vector3 wheelPosWS = transform.Origin;

                Vector3 axle = new Vector3(
                    transform[0, RightAxis],
                    transform[1, RightAxis],
                    transform[2, RightAxis]);

                Vector3 to1 = wheelPosWS + axle;
                Vector3 to2 = GetWheelInfo(v).RaycastInfo.ContactPointWS;

                //debug wheels (cylinders)
                debugDrawer.DrawLine(ref wheelPosWS, ref to1, ref wheelColor);
                debugDrawer.DrawLine(ref wheelPosWS, ref to2, ref wheelColor);

            }
        }

        /// <summary>
        /// Returns the <see cref="RobotWheelInfo"/> for the given wheel index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RobotWheelInfo GetWheelInfo(int index)
        {
            Debug.Assert((index >= 0) && (index < NumWheels));

            return wheelInfo[index];
        }

        /// <summary>
        /// Casts a ray given the provided <see cref="WheelInfo"/> for suspension calculations.
        /// </summary>
        /// <param name="wheel"></param>
        /// <returns></returns>
        private float RayCast(WheelInfo wheel)
        {
            UpdateWheelTransformsWS(wheel, false);

            float depth = -1;
            float raylen = wheel.SuspensionRestLength + wheel.WheelsRadius;

            Vector3 rayvector = wheel.RaycastInfo.WheelDirectionWS * raylen;
            Vector3 source = wheel.RaycastInfo.HardPointWS;
            wheel.RaycastInfo.ContactPointWS = source + rayvector;
            Vector3 target = wheel.RaycastInfo.ContactPointWS;

            float param = 0;
            VehicleRaycasterResult rayResults = new VehicleRaycasterResult();

            Debug.Assert(vehicleRaycaster != null);
            object obj = vehicleRaycaster.CastRay(ref source, ref target, rayResults);

            wheel.RaycastInfo.GroundObject = null;

            if (obj != null)
            {
                param = rayResults.DistFraction;
                depth = raylen * rayResults.DistFraction;
                wheel.RaycastInfo.ContactNormalWS = rayResults.HitNormalInWorld;
                wheel.RaycastInfo.IsInContact = true;

                wheel.RaycastInfo.GroundObject = fixedBody;///@todo for driving on dynamic/movable objects!;
                /////wheel.RaycastInfo.GroundObject = object;

                float hitDistance = param * raylen;
                wheel.RaycastInfo.SuspensionLength = hitDistance - wheel.WheelsRadius;
                //clamp on max suspension travel

                float minSuspensionLength = wheel.SuspensionRestLength - wheel.MaxSuspensionTravelCm * 0.01f;
                float maxSuspensionLength = wheel.SuspensionRestLength + wheel.MaxSuspensionTravelCm * 0.01f;
                if (wheel.RaycastInfo.SuspensionLength < minSuspensionLength)
                {
                    wheel.RaycastInfo.SuspensionLength = minSuspensionLength;
                }
                if (wheel.RaycastInfo.SuspensionLength > maxSuspensionLength)
                {
                    wheel.RaycastInfo.SuspensionLength = maxSuspensionLength;
                }

                wheel.RaycastInfo.ContactPointWS = rayResults.HitPointInWorld;

                float denominator = Vector3.Dot(wheel.RaycastInfo.ContactNormalWS, wheel.RaycastInfo.WheelDirectionWS);

                Vector3 chassis_velocity_at_contactPoint;
                Vector3 relpos = wheel.RaycastInfo.ContactPointWS - RigidBody.CenterOfMassPosition;

                chassis_velocity_at_contactPoint = RigidBody.GetVelocityInLocalPoint(relpos);

                float projVel = Vector3.Dot(wheel.RaycastInfo.ContactNormalWS, chassis_velocity_at_contactPoint);

                if (denominator >= -0.1f)
                {
                    wheel.SuspensionRelativeVelocity = 0;
                    wheel.ClippedInvContactDotSuspension = 1.0f / 0.1f;
                }
                else
                {
                    float inv = -1.0f / denominator;
                    wheel.SuspensionRelativeVelocity = projVel * inv;
                    wheel.ClippedInvContactDotSuspension = inv;
                }
            }
            else
            {
                //put wheel info as in rest position
                wheel.RaycastInfo.SuspensionLength = wheel.SuspensionRestLength;
                wheel.SuspensionRelativeVelocity = 0.0f;
                wheel.RaycastInfo.ContactNormalWS = -wheel.RaycastInfo.WheelDirectionWS;
                wheel.ClippedInvContactDotSuspension = 1.0f;
            }

            return depth;
        }

        /// <summary>
        /// Resets the suspension for all wheels.
        /// </summary>
        void ResetSuspension()
        {
            for (int i = 0; i < NumWheels; i++)
            {
                WheelInfo wheel = GetWheelInfo(i);
                wheel.RaycastInfo.SuspensionLength = wheel.SuspensionRestLength;
                wheel.SuspensionRelativeVelocity = 0;

                wheel.RaycastInfo.ContactNormalWS = -wheel.RaycastInfo.WheelDirectionWS;
                //wheel.ContactFriction = 0;
                wheel.ClippedInvContactDotSuspension = 1;
            }
        }

        /// <summary>
        /// Calculates friction between two RigidBodies.
        /// </summary>
        /// <param name="body1"></param>
        /// <param name="pos1"></param>
        /// <param name="body2"></param>
        /// <param name="pos2"></param>
        /// <param name="distance"></param>
        /// <param name="normal"></param>
        /// <param name="impulse"></param>
        /// <param name="timeStep"></param>
        private void ResolveSingleBilateral(RigidBody body1, Vector3 pos1, RigidBody body2, Vector3 pos2, float distance, Vector3 normal, ref float impulse, float timeStep)
        {
            float normalLenSqr = normal.LengthSquared;
            Debug.Assert(System.Math.Abs(normalLenSqr) < 1.1f);
            if (normalLenSqr > 1.1f)
            {
                impulse = 0;
                return;
            }
            Vector3 rel_pos1 = pos1 - body1.CenterOfMassPosition;
            Vector3 rel_pos2 = pos2 - body2.CenterOfMassPosition;

            Vector3 vel1 = body1.GetVelocityInLocalPoint(rel_pos1);
            Vector3 vel2 = body2.GetVelocityInLocalPoint(rel_pos2);
            Vector3 vel = vel1 - vel2;

            Matrix world2A = Matrix.Transpose(body1.CenterOfMassTransform.Basis);
            Matrix world2B = Matrix.Transpose(body2.CenterOfMassTransform.Basis);
            Vector3 m_aJ = Vector3.TransformCoordinate(Vector3.Cross(rel_pos1, normal), world2A);
            Vector3 m_bJ = Vector3.TransformCoordinate(Vector3.Cross(rel_pos2, -normal), world2B);
            Vector3 m_0MinvJt = body1.InvInertiaDiagLocal * m_aJ;
            Vector3 m_1MinvJt = body2.InvInertiaDiagLocal * m_bJ;
            float dot0, dot1;
            Vector3.Dot(ref m_0MinvJt, ref m_aJ, out dot0);
            Vector3.Dot(ref m_1MinvJt, ref m_bJ, out dot1);
            float jacDiagAB = body1.InvMass + dot0 + body2.InvMass + dot1;
            float jacDiagABInv = 1.0f / jacDiagAB;

            float rel_vel;
            Vector3.Dot(ref normal, ref vel, out rel_vel);

            float velocityImpulse = -contactDamping * rel_vel * jacDiagABInv;
            impulse = velocityImpulse;
        }

        /// <summary>
        /// Updates the friction for the RaycastRobot.
        /// </summary>
        /// <param name="timeStep"></param>
        public void UpdateFriction(float timeStep)
        {
            //calculate the impulse, so that the wheels don't move sidewards
            int numWheel = NumWheels;
            if (numWheel == 0)
                return;

            Array.Resize<Vector3>(ref forwardWS, numWheel);
            Array.Resize<Vector3>(ref axle, numWheel);
            Array.Resize<float>(ref forwardImpulse, numWheel);
            Array.Resize<float>(ref sideImpulse, numWheel);

            int numWheelsOnGround = 0;

            //collapse all those loops into one!
            for (int i = 0; i < NumWheels; i++)
            {
                RigidBody groundObject = wheelInfo[i].RaycastInfo.GroundObject as RigidBody;
                if (groundObject != null)
                    numWheelsOnGround++;
                sideImpulse[i] = 0;
                forwardImpulse[i] = 0;
            }

            for (int i = 0; i < NumWheels; i++)
            {
                RobotWheelInfo wheel = wheelInfo[i];

                RigidBody groundObject = wheel.RaycastInfo.GroundObject as RigidBody;
                if (groundObject != null)
                {
                    Matrix wheelTrans = GetWheelTransformWS(i);

                    axle[i] = new Vector3(
                        wheelTrans[0, indexRightAxis],
                        wheelTrans[1, indexRightAxis],
                        wheelTrans[2, indexRightAxis]);

                    Vector3 surfNormalWS = wheel.RaycastInfo.ContactNormalWS;
                    float proj;
                    Vector3.Dot(ref axle[i], ref surfNormalWS, out proj);
                    axle[i] -= surfNormalWS * proj;
                    axle[i].Normalize();

                    Vector3.Cross(ref surfNormalWS, ref axle[i], out forwardWS[i]);
                    forwardWS[i].Normalize();

                    ResolveSingleBilateral(RootRigidBody, wheel.RaycastInfo.ContactPointWS,
                              groundObject, wheel.RaycastInfo.ContactPointWS,
                              0, axle[i], ref sideImpulse[i], timeStep);

                    sideImpulse[i] *= wheel.SlidingFriction;
                }
                else
                {
                    if (wheel.Speed > 0)
                        wheel.Speed = Math.Max(wheel.Speed - wheel.FreeSpinDamping, 0f);
                    else if (wheel.Speed < 0)
                        wheel.Speed = Math.Min(wheel.Speed + wheel.FreeSpinDamping, 0f);
                }
            }

            bool sliding = false;

            for (int i = 0; i < NumWheels; i++)
            {
                RobotWheelInfo wheel = wheelInfo[i];
                RigidBody groundObject = wheel.RaycastInfo.GroundObject as RigidBody;

                float rollingFriction = 0.0f;

                if (groundObject != null)
                {
                    Vector3 velocity = chassisBody.GetVelocityInLocalPoint(wheel.ChassisConnectionPointCS);
                    Vector3 localVelocity = Vector3.TransformNormal(velocity, Matrix.Invert(chassisBody.WorldTransform.Basis));
                    Vector3 forwardAxis = (UnityEngine.Quaternion.AngleAxis(90f, UnityEngine.Vector3.up) *
                        wheel.WheelAxleCS.ToUnity() / (MathUtil.SIMD_PI * wheel.WheelsRadius)).ToBullet();

                    float speed = Vector3.Dot(localVelocity, forwardAxis);

                    wheel.Speed = speed;

                    if (wheel.EngineForce != 0.0f)
                    {
                        //apply torque curves
                        float engineForce = wheel.EngineForce;

                        if (speed * engineForce > 0)
                            engineForce *= 1 - (Math.Abs(speed) / MaxWheelAngularVelocity);

                        rollingFriction = engineForce * timeStep;

                        if (!RootRigidBody.IsActive)
                            RootRigidBody.Activate();
                    }
                    else
                    {
                        float defaultRollingFrictionImpulse = 0.0f;
                        float maxImpulse = (wheel.Brake != 0) ? wheel.Brake : defaultRollingFrictionImpulse;
                        rollingFriction = CalcRollingFriction(RootRigidBody, groundObject, wheel.RaycastInfo.ContactPointWS, forwardWS[i], maxImpulse);
                    }
                }

                //switch between active rolling (throttle), braking and non-active rolling friction (no throttle/break)

                forwardImpulse[i] = 0;
                wheelInfo[i].SkidInfo = 1.0f;

                if (groundObject != null)
                {
                    wheelInfo[i].SkidInfo = 1.0f;

                    float maximp = wheel.WheelsSuspensionForce * timeStep * wheel.FrictionSlip;
                    float maximpSide = maximp;

                    float maximpSquared = maximp * maximpSide;

                    forwardImpulse[i] = rollingFriction;

                    float x = forwardImpulse[i] * fwdFactor;
                    float y = sideImpulse[i] * sideFactor;

                    float impulseSquared = (x * x + y * y);

                    if (impulseSquared > maximpSquared)
                    {
                        sliding = true;

                        float factor = maximp / (float)System.Math.Sqrt(impulseSquared);

                        wheelInfo[i].SkidInfo *= factor;
                    }
                }
            }

            if (sliding)
            {
                for (int wheel = 0; wheel < NumWheels; wheel++)
                {
                    if (sideImpulse[wheel] != 0)
                    {
                        if (wheelInfo[wheel].SkidInfo < 1.0f)
                        {
                            forwardImpulse[wheel] *= wheelInfo[wheel].SkidInfo;
                            sideImpulse[wheel] *= wheelInfo[wheel].SkidInfo;
                        }
                    }
                }
            }

            // apply the impulses
            for (int i = 0; i < NumWheels; i++)
            {
                WheelInfo wheel = wheelInfo[i];

                Vector3 rel_pos = wheel.RaycastInfo.ContactPointWS -
                        chassisBody.CenterOfMassPosition;

                if (forwardImpulse[i] != 0)
                {
                    chassisBody.ApplyImpulse(forwardWS[i] * forwardImpulse[i], rel_pos);
                }
                if (sideImpulse[i] != 0)
                {
                    RigidBody groundObject = wheel.RaycastInfo.GroundObject as RigidBody;

                    Vector3 rel_pos2 = wheel.RaycastInfo.ContactPointWS -
                        groundObject.CenterOfMassPosition;


                    Vector3 sideImp = axle[i] * sideImpulse[i];

#if ROLLING_INFLUENCE_FIX // fix. It only worked if car's up was along Y - VT.
                    //Vector4 vChassisWorldUp = RigidBody.CenterOfMassTransform.get_Columns(indexUpAxis);
                    Vector3 vChassisWorldUp = new Vector3(
                        RigidBody.CenterOfMassTransform.Row1[indexUpAxis],
                        RigidBody.CenterOfMassTransform.Row2[indexUpAxis],
                        RigidBody.CenterOfMassTransform.Row3[indexUpAxis]);
                    float dot;
                    Vector3.Dot(ref vChassisWorldUp, ref rel_pos, out dot);
                    rel_pos -= vChassisWorldUp * (dot * (1.0f - wheel.RollInfluence));
#else
                    rel_pos[indexUpAxis] *= wheel.RollInfluence;
#endif
                    chassisBody.ApplyImpulse(sideImp, rel_pos);

                    //apply friction impulse on the ground
                    groundObject.ApplyImpulse(-sideImp, rel_pos2);
                }
            }
        }

        /// <summary>
        /// Updates suspension forces for the RaycastRobot.
        /// </summary>
        /// <param name="step"></param>
        public void UpdateSuspension(float step)
        {
            for (int w_it = 0; w_it < NumWheels; w_it++)
            {
                WheelInfo wheel_info = wheelInfo[w_it];

                if (wheel_info.RaycastInfo.IsInContact)
                {
                    float force;
                    //	Spring
                    {
                        float susp_length = wheel_info.SuspensionRestLength;
                        float current_length = wheel_info.RaycastInfo.SuspensionLength;

                        float length_diff = (susp_length - current_length);

                        force = wheel_info.SuspensionStiffness
                            * length_diff * wheel_info.ClippedInvContactDotSuspension;
                    }

                    // Damper
                    {
                        float projected_rel_vel = wheel_info.SuspensionRelativeVelocity;
                        {
                            float susp_damping;
                            if (projected_rel_vel < 0.0f)
                            {
                                susp_damping = wheel_info.WheelsDampingCompression;
                            }
                            else
                            {
                                susp_damping = wheel_info.WheelsDampingRelaxation;
                            }
                            force -= susp_damping * projected_rel_vel;
                        }
                    }

                    // RESULT
                    wheel_info.WheelsSuspensionForce = force * OverrideMass;
                    if (wheel_info.WheelsSuspensionForce < 0)
                    {
                        wheel_info.WheelsSuspensionForce = 0;
                    }
                }
                else
                {
                    wheel_info.WheelsSuspensionForce = 0;
                }
            }
        }

        /// <summary>
        /// Updates the RaycastRobot as a whole.
        /// </summary>
        /// <param name="step"></param>
        public void UpdateVehicle(float step)
        {
            for (int i = 0; i < wheelInfo.Length; i++)
            {
                UpdateWheelTransform(i, false);
            }

            currentVehicleSpeedKmHour = 3.6f * RigidBody.LinearVelocity.Length;

            Matrix chassisTrans = ChassisWorldTransform;

            Vector3 forwardW = new Vector3(
                chassisTrans[0, indexForwardAxis],
                chassisTrans[1, indexForwardAxis],
                chassisTrans[2, indexForwardAxis]);

            if (Vector3.Dot(forwardW, RigidBody.LinearVelocity) < 0)
            {
                currentVehicleSpeedKmHour *= -1.0f;
            }

            // Simulate suspension
            for (int i = 0; i < wheelInfo.Length; i++)
            {
                //float depth = 
                RayCast(wheelInfo[i]);
            }


            UpdateSuspension(step);

            for (int i = 0; i < wheelInfo.Length; i++)
            {
                //apply suspension force
                WheelInfo wheel = wheelInfo[i];

                float suspensionForce = wheel.WheelsSuspensionForce;

                if (suspensionForce > wheel.MaxSuspensionForce)
                {
                    suspensionForce = wheel.MaxSuspensionForce;
                }
                Vector3 impulse = wheel.RaycastInfo.ContactNormalWS * suspensionForce * step;
                Vector3 relpos = wheel.RaycastInfo.ContactPointWS - RigidBody.CenterOfMassPosition;

                RigidBody.ApplyImpulse(impulse, relpos);
            }

            UpdateFriction(step);

            for (int i = 0; i < wheelInfo.Length; i++)
            {
                WheelInfo wheel = wheelInfo[i];
                Vector3 relpos = wheel.RaycastInfo.HardPointWS - RigidBody.CenterOfMassPosition;
                Vector3 vel = RigidBody.GetVelocityInLocalPoint(relpos);

                if (wheel.RaycastInfo.IsInContact)
                {
                    Matrix chassisWorldTransform = ChassisWorldTransform;

                    Vector3 fwd = new Vector3(
                        chassisWorldTransform[0, indexForwardAxis],
                        chassisWorldTransform[1, indexForwardAxis],
                        chassisWorldTransform[2, indexForwardAxis]);

                    float proj = Vector3.Dot(fwd, wheel.RaycastInfo.ContactNormalWS);
                    fwd -= wheel.RaycastInfo.ContactNormalWS * proj;

                    float proj2;
                    Vector3.Dot(ref fwd, ref vel, out proj2);

                    wheel.DeltaRotation = (proj2 * step) / (wheel.WheelsRadius);
                    wheel.Rotation += wheel.DeltaRotation;
                }
                else
                {
                    wheel.Rotation += wheel.DeltaRotation;
                }

                wheel.DeltaRotation *= 0.99f;//damping of rotation when not in contact
            }
        }

        /// <summary>
        /// Updates the wheel transform that corresponds with the provided wheel index.
        /// </summary>
        /// <param name="wheelIndex"></param>
        /// <param name="interpolatedTransform"></param>
        public void UpdateWheelTransform(int wheelIndex, bool interpolatedTransform)
        {
            WheelInfo wheel = wheelInfo[wheelIndex];
            UpdateWheelTransformsWS(wheel, interpolatedTransform);
            Vector3 up = -wheel.RaycastInfo.WheelDirectionWS;
            Vector3 right = wheel.RaycastInfo.WheelAxleWS;
            Vector3 fwd = Vector3.Cross(up, right);
            fwd.Normalize();
            //up = Vector3.Cross(right, fwd);
            //up.Normalize();

            //rotate around steering over the wheelAxleWS
            Matrix steeringMat = Matrix.RotationAxis(up, wheel.Steering);
            Matrix rotatingMat = Matrix.RotationAxis(right, -wheel.Rotation);

            Matrix basis2 = new Matrix();
            basis2.M11 = right[0];
            basis2.M12 = fwd[0];
            basis2.M13 = up[0];
            basis2.M21 = right[1];
            basis2.M22 = fwd[1];
            basis2.M23 = up[1];
            basis2.M31 = right[2];
            basis2.M32 = fwd[2];
            basis2.M13 = up[2];

            Matrix transform = steeringMat * rotatingMat * basis2;
            transform.Origin = wheel.RaycastInfo.HardPointWS + wheel.RaycastInfo.WheelDirectionWS * wheel.RaycastInfo.SuspensionLength;
            wheel.WorldTransform = transform;
        }

        /// <summary>
        /// Updates the wheel transform given the provided <see cref="WheelInfo"/>.
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="interpolatedTransform"></param>
        void UpdateWheelTransformsWS(WheelInfo wheel, bool interpolatedTransform)
        {
            wheel.RaycastInfo.IsInContact = false;

            Matrix chassisTrans = ChassisWorldTransform;
            if (interpolatedTransform && RigidBody.MotionState != null)
            {
                chassisTrans = RigidBody.MotionState.WorldTransform;
            }

            wheel.RaycastInfo.HardPointWS = Vector3.TransformCoordinate(wheel.ChassisConnectionPointCS, chassisTrans);
            Matrix chassisTransBasis = chassisTrans.Basis;
            wheel.RaycastInfo.WheelDirectionWS = Vector3.TransformCoordinate(wheel.WheelDirectionCS, chassisTransBasis);
            wheel.RaycastInfo.WheelAxleWS = Vector3.TransformCoordinate(wheel.WheelAxleCS, chassisTransBasis);
        }
    }
}
