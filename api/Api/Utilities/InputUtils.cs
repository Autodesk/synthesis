using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SynthesisAPI.Utilities
{
	public static class InputUtils
	{
		public static bool GetKeyOrButtonDown(string name)
		{
			try
			{
				try
				{
					return Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), name));
				}
				catch
				{
					return Input.GetKeyDown(name);
				}
			}
			catch (ArgumentException)
			{
				try
				{
					return Input.GetButtonDown(name);
				}
				catch (ArgumentException)
				{
					Logger.Log($"Failed to find key or button named {name}");
					return false;
				}
			}
		}

		public static bool GetKeyOrButton(string name)
		{
			try
			{

				try
				{
					return Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), name));
				}
				catch
				{
					return Input.GetKey(name);
				}
			}
			catch (ArgumentException)
			{
				try
				{
					return Input.GetButton(name);
				}
				catch (ArgumentException)
				{
					Logger.Log($"Failed to find key or button named {name}");
					return false;
				}
			}
		}

		public static bool GetKeyOrButtonUp(string name)
		{
			try
			{
				try
				{
					return Input.GetKeyUp((KeyCode)Enum.Parse(typeof(KeyCode), name));
				}
				catch
				{
					return Input.GetKeyUp(name);
				}
			}
			catch (ArgumentException)
			{
				try
				{
					return Input.GetButtonUp(name);
				}
				catch (ArgumentException)
				{
					Logger.Log($"Failed to find key or button named {name}");
					return false;
				}
			}
		}
	}
}