using NUnit.Framework;

namespace TestApi
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            MockApi.MockApi.Init();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}
