using System;
using System.IO;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace AzureMobileServicesOfflineTests
{
    public class TestContext : IDisposable
    {
        public MobileServiceClient client { get; set; }

        public TestContext()
        {
            this.client = new MobileServiceClient("http://localhost:31475/");
            File.Delete("test.db");
            var store = new MobileServiceSQLiteStore("test.db");
            store.DefineTable<Storage>();
            store.DefineTable<Mongo>();
            store.DefineTable<Entity>();
            store.DefineTable<MappedEntity>();
            this.client.SyncContext.InitializeAsync(store).Wait();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }
    }
}
