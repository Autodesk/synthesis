using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Nodes.Joint;

namespace GopherAPI.Nodes.Joint.Driver
{
    public enum Wheel { NON_WHEEL, NORMAL, OMNI, MECANUM }
    /// <summary>
    /// NONE is for limited joints without friction | NO_LIMITS is for non-limited joints 
    /// </summary>
    public enum Friction { NONE, HIGH, MEDIUM, LOW, NO_LIMITS }
    public enum InternalDiameter { ONE, HALF, QUARTER }
    public enum Pressure { SIXTY_PSI, TWENTY_PSI, TEN_PSI }
    public enum Stages { SINGLE_STAGE_ELEVATOR, CASCADING_STAGE_ONE, CASCADING_STAGE_TWO, CONTINUOUS_STAGE_ONE, CONTINUOUS_STAGE_TWO }
    public enum Driver { NONE, MOTOR, SERVO, BUMPER_PNUEMATIC, RELAY_PNUEMATIC, WORM_SCREW, DUAL_MOTOR, ELEVATOR, NULL }

    public class GopherDriver_Base
    {
        public GopherJoint_Base Joint { get; internal set; }
        public DriverMeta Meta { get; internal set; }
        public virtual Driver GetDriverType()
        {
            return Driver.NULL;
        }
        public virtual bool GetIsDriveWheel()
        {
            return false;
        }
        /// <summary>
        /// Gets the joint which the driver is attached to
        /// </summary>
        /// <returns></returns>
        public virtual GopherJoint_Base GetJoint()
        {
            return null;
        }
    }
}