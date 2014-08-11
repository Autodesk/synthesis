using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Threading;

class PneumaticAnalyzer
{
    /// <summary>
    /// Saves all of the information of a pneumatic, such as diameter and pressure.
    /// </summary>
    /// <param name="diameter">
    /// The diameter of the pneumatic in cm.
    /// </param>
    /// <param name="pressure">
    /// The pressure of the pneumatic in PSI.
    /// </param>
    public static void SaveToPneumaticJoint(PneumaticDiameter diameter, PneumaticPressure pressure, RigidNode node)
    {
        SkeletalJoint_Base joint = node.GetSkeletalJoint();
        PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta(); //The info about the wheel attached to the joint.
        //RigidNode.DeferredCalculation newCalculation;

        //TODO: Find real values
        switch (diameter)
        {
            case PneumaticDiameter.HIGH:
                pneumaticDriver.widthMM = 10;
                break;
            case PneumaticDiameter.MEDIUM:
                pneumaticDriver.widthMM = 5;
                break;
            case PneumaticDiameter.LOW:
                pneumaticDriver.widthMM = 1;
                break;
        }

        switch (pressure)
        {
            case PneumaticPressure.HIGH:
                pneumaticDriver.pressurePSI = 10;
                break;
            case PneumaticPressure.MEDIUM:
                pneumaticDriver.pressurePSI = 5;
                break;
            case PneumaticPressure.LOW:
                pneumaticDriver.pressurePSI = 1;
                break;
        }

        //newCalculation = StartCalculations;
        node.RegisterDeferredCalculation(node.GetModelID(), null);
    }
}