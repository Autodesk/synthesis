using System;
using System.Runtime.InteropServices;

namespace DemoCSharpActiveX
{
    /// <summary>
    /// Demo HelloWorld class
    /// </summary>
    [ProgId("DemoCSharpActiveX.HelloWord")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("FC3EB753-8750-452C-80CE-40B064EC4616")]
    [ComVisible(true)]
    public class HelloWord
    {
        [ComVisible(true)]
        public String SayHello()
        {
            return "Hello World!";
        }
    }
}