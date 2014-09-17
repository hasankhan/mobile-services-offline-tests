using System;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace AzureMobileServicesOfflineTests
{
    public class TestContext : IDisposable
    {
        public MobileServiceClient Client { get; private set; }

        public void Initialize(string uri)
        {
            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.DefaultTraceListener());

            string db = Guid.NewGuid().ToString("N");

            var store = new LoggingLocalStore(db);
            store.DefineTable<Storage>();
            store.DefineTable<Mongo>();
            store.DefineTable<Entity>();
            store.DefineTable<MappedEntity>();
            store.DefineTable<Node>();

            this.Client = new MobileServiceClient(uri);
            this.Client.SyncContext.InitializeAsync(store).Wait();
        }

        public void Dispose()
        {
            if (this.Client != null)
            {
                this.Client.Dispose();
            }
        }
    }
}
