using System;

namespace ErrorHandling
{
		// I am simply creating my own exception. This seems fairly self explanatory
		public class PWMAssignedException: Exception
		{
				public PWMAssignedException (string message):base(message)
				{
				}
		}
}

