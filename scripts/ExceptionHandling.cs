using System;

namespace ExceptionHandling
{
		// I am simply creating my own exception. This seems fairly self explanatory
		public class PWMAssignedException: Exception
		{
				public PWMAssignedException (string message):base(message)
				{
				}
		}

		public class SolenoidConflictException: Exception
		{
				public SolenoidConflictException (int x):base(string.Format("Error, you are attempting to assign Solenoid port {0}, but it has already been assigned.", x))
				{
				}
		}

		public class WheelDifferenceException: Exception
		{
				public WheelDifferenceException (string message):base(message)
				{
				}
		}
}

