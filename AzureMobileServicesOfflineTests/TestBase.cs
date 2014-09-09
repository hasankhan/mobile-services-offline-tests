using Xunit;

namespace AzureMobileServicesOfflineTests
{
    public abstract class TestBase : IUseFixture<TestContext>
    {
        protected TestContext Context { get; private set; }
        public void SetFixture(TestContext context)
        {
            this.Context = context;
            this.Context.Initialize(this.Uri);
            TestInitialize();
        }

        protected virtual void TestInitialize()
        {
        }

        protected virtual string Uri
        {
            get { return "http://localhost:31475/"; }
        }
    }
}
