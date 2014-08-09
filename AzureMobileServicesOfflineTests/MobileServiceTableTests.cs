using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
    public class MobileServiceTableTests : IUseFixture<MobileServiceTableTests.TestContext>
    {
        private TestContext context;
        public void SetFixture(MobileServiceTableTests.TestContext context)
        {
            this.context = context;
        }

        [Fact]
        public async Task PullAsync_Storage()
        {
            var table = this.context.client.GetSyncTable<Storage>();
            List<Storage> items = await table.ToListAsync();
            // table should be empty
            Assert.Equal(items.Count, 0);

            string id = "'" + Guid.NewGuid().ToString("N") + "','" + Guid.NewGuid().ToString("N") + "'";
            var original = new Storage()
            {
                Id = id,
                FirstName = "hasan",
                LastName = "khan",
                Age = 29
            };

            // insert an item
            await table.InsertAsync(original);
            // push it 
            await this.context.client.SyncContext.PushAsync();
            // clear the table
            await table.PurgeAsync();
            // download the changes
            await table.PullAsync();
            // verify the item was also downloaded
            Storage item = await table.LookupAsync(id);
            // the item should be there
            Assert.NotNull(item);
            Assert.Equal(item.FirstName, original.FirstName);
            Assert.Equal(item.LastName, original.LastName);
            Assert.Equal(item.Age, 5);

        }

        public class TestContext : IDisposable
        {
            public MobileServiceClient client { get; set; }

            public TestContext()
            {
                this.client = new MobileServiceClient("http://localhost:31475/");
                File.Delete("test.db");
                var store = new MobileServiceSQLiteStore("test.db");
                store.DefineTable<Storage>();
                this.client.SyncContext.InitializeAsync(store).Wait();
            }

            public void Dispose()
            {
                this.client.Dispose();
            }
        }


    }
}
