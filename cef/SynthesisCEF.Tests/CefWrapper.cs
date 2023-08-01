namespace SynthesisCEF.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Synthesis.CEF;

[TestClass]
public class CefWrapper {
    [TestMethod]
    public void RunCef() {
        CEF.RunCef();
    }
}
