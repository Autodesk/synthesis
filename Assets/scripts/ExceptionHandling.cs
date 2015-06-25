using System;

namespace ExceptionHandling
{
	public class PistonDataMissing: Exception
	{
		public PistonDataMissing(string pistonName):base(string.Format("It looks like piston {0} wants to be actuated, but no custom force value was set. Make sure to set PSI and Diameter values when you expor the robot.", pistonName))
		{
		}
	}
}

