/*
 * User: frankfralick
 * Date: 11/20/2012
 * Time: 9:04 AM
 */
using System;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CSharp;

namespace SimpleAddIn
{
	/// <summary>
	/// This is a basic class that sets up the python engine.  
	/// </summary>
	internal class Engine 
	{
		ScriptEngine _engine;
		ScriptRuntime _runtime;
		CompiledCode _code;
		ScriptScope _scope;
		public Engine(string filePath,Inventor.PlanarSketch oSketch,Inventor.Application oApp, double slotHeight, double slotWidth)
		{
			_engine = Python.CreateEngine(new Dictionary<string, object>() { {"Frames", true}, {"FullFrames", true}});
			_runtime = _engine.Runtime;
			Assembly invAssembly = Assembly.LoadFile(@"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Autodesk.Inventor.Interop\v4.0_17.0.0.0__d84147f8b4276564\Autodesk.Inventor.interop.dll");
			_runtime.LoadAssembly(invAssembly);
			_scope = _engine.CreateScope();
			//Make variable names visible within the python file.  These string names will be used an arguments in the python file.
			_scope.SetVariable("oPlanarSketch",oSketch);
			_scope.SetVariable("oApp",oApp);
			_scope.SetVariable("slotHeight",slotHeight);
			_scope.SetVariable("slotWidth",slotWidth);
			ScriptSource _script = _engine.CreateScriptSourceFromFile(filePath);
			_code = _script.Compile();                                                            
		}
		
		public bool Execute()
		{
			try
			{
				_code.Execute(_scope);	
				return true;
			}
			catch (Exception e)
			{
				ExceptionOperations eo = _engine.GetService<ExceptionOperations>();
				Console.Write(eo.FormatException(e));
				System.Windows.Forms.MessageBox.Show(eo.FormatException(e),"Python Error",System.Windows.Forms.MessageBoxButtons.OK);
				return false;
			}
		}
		public void SetVariable(string name, object value)
		{
			_scope.SetVariable(name,value);
		}
		
		public bool TryGetVariable(string name, out string result)
		{
			return _scope.TryGetVariable(name, out result);
		}
		
		
	}
}
