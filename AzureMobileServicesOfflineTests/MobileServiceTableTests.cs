using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssertExLib;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
    public class MobileServiceTableTests
    {
        public class MongoTests : TestBase
        {
            private IMobileServiceSyncTable<Mongo> table;
            protected override void TestInitialize()
            {
                this.table = this.Context.client.GetSyncTable<Mongo>();
            }

            [Fact]
            public async Task PullAsync_DownloadsTheItems()
            {
                List<Mongo> items = await this.table.ToListAsync();
                // table should be empty
                Assert.Equal(items.Count, 0);

                for (int i = 0; i < 2; i++)
                {
                    var original = NewItem();
                    // insert an item
                    await this.Context.client.GetTable<Mongo>().InsertAsync(original);
                    // download the changes
                    await this.table.PullAsync();
                    // verify the item was also downloaded
                    Mongo downloaded = (await this.table.Where(x => x.Name == original.Name).ToListAsync()).FirstOrDefault();
                    // the item should be there
                    Assert.NotNull(downloaded);
                    Assert.Equal(downloaded.Name, original.Name);
                    Assert.Equal(downloaded.Age, original.Age);

                    downloaded = await this.table.LookupAsync(downloaded.Id);
                    Assert.Equal(original.Name, downloaded.Name);
                    Assert.Equal(original.Age, downloaded.Age);
                }
            }

            private static Mongo NewItem()
            {
                var original = new Mongo()
                {
                    Name = Guid.NewGuid().ToString("N"),
                    Age = 29
                };
                return original;
            }

            [Fact]
            public async Task PullAsync_Incremental_Throws()
            {
                var ex = AssertEx.TaskThrows<MobileServiceInvalidOperationException>(() => this.table.PullAsync("items", this.table.CreateQuery()));
                Assert.Equal(ex.Request.RequestUri.ToString(), "http://localhost:31475/tables/Person?$filter=(__updatedAt ge datetimeoffset'0001-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$top=50&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__version");
            }
        }

        public class StorageTests : TestBase
        {
            private IMobileServiceSyncTable<Storage> table;
            protected override void TestInitialize()
            {
                this.table = this.Context.client.GetSyncTable<Storage>();
            }

            [Fact]
            public async Task PullAsync_DownloadsTheItems()
            {
                List<Storage> items = await this.table.ToListAsync();
                // table should be empty
                Assert.Equal(items.Count, 0);

                for (int i = 0; i < 2; i++)
                {
                    var original = NewItem();
                    // insert an item
                    await this.table.InsertAsync(original);
                    // push it 
                    await this.Context.client.SyncContext.PushAsync();
                    // clear the table
                    await this.table.PurgeAsync();
                    // download the changes
                    await this.table.PullAsync();
                    // verify the item was also downloaded
                    Storage downloaded = await this.table.LookupAsync(original.Id);
                    // the item should be there
                    Assert.NotNull(downloaded);
                    Assert.Equal(original.FirstName, downloaded.FirstName);
                    Assert.Equal(original.LastName, downloaded.LastName);
                    Assert.Equal(original.Age, downloaded.Age);
                }
            }

            private static Storage NewItem()
            {
                var original = new Storage()
                {
                    Id = "'" + Guid.NewGuid().ToString("N") + "','" + Guid.NewGuid().ToString("N") + "'",
                    FirstName = "hasan",
                    LastName = "khan",
                    Age = 29
                };
                return original;
            }

            [Fact]
            public async Task PullAsync_Incremental_Throws()
            {
                var ex = AssertEx.TaskThrows<MobileServiceInvalidOperationException>(() => this.table.PullAsync("items", this.table.CreateQuery()));
                Assert.Equal(ex.Request.RequestUri.ToString(), "http://localhost:31475/tables/Person?$filter=(__updatedAt ge datetimeoffset'0001-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$top=50&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__version");
            }
        }

        public class TestBase : IUseFixture<MobileServiceTableTests.TestContext>
        {
            protected TestContext Context { get; private set; }
            public void SetFixture(MobileServiceTableTests.TestContext context)
            {
                this.Context = context;
                TestInitialize();
            }

            protected virtual void TestInitialize()
            {
            }
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
                store.DefineTable<Mongo>();
                this.client.SyncContext.InitializeAsync(store).Wait();
            }

            public void Dispose()
            {
                this.client.Dispose();
            }
        }


    }
}
