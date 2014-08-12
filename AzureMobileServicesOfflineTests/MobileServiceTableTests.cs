using System;
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
                this.table.SupportedOptions &= ~(MobileServiceRemoteTableOptions.Skip | MobileServiceRemoteTableOptions.Top);
            }

            [Fact]
            public async Task Mongo_PullAsync_DownloadsTheItems()
            {
                for (int i = 0; i < 2; i++)
                {
                    var original = NewItem();
                    // insert an item
                    await this.Context.client.GetTable<Mongo>().InsertAsync(original);
                    // download the changes by comparing the id because in mongo if the item
                    // is added at the end then we're not going to iterate over pages
                    // since skip and top are broken
                    await this.table.PullAsync(this.table.Where(m => m.Id == original.Id));
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

            [Fact]
            public void Mongo_PullAsync_Incremental_Throws()
            {
                var ex = AssertEx.TaskThrows<MobileServiceInvalidOperationException>(() => this.table.PullAsync("items", this.table.CreateQuery()));
                Assert.Contains("Exception has been thrown by the target of an invocation.", ex.Response.Content.ReadAsStringAsync().Result);
                Assert.Equal("http://localhost:31475/tables/Mongo?$filter=(__updatedAt ge datetimeoffset'0001-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__version", ex.Request.RequestUri.ToString());
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
        }

        public class StorageTests : TestBase
        {
            private IMobileServiceSyncTable<Storage> table;
            protected override void TestInitialize()
            {
                this.table = this.Context.client.GetSyncTable<Storage>();
                this.table.SupportedOptions &= ~(MobileServiceRemoteTableOptions.OrderBy | MobileServiceRemoteTableOptions.Skip);
            }

            [Fact]
            public async Task Storage_PullAsync_DownloadsTheItems()
            {
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

            [Fact]
            public void Storage_PullAsync_Incremental_Throws()
            {
                var ex = AssertEx.TaskThrows<MobileServiceInvalidOperationException>(() => this.table.PullAsync("items", this.table.CreateQuery()));
                Assert.Contains("Could not find a property named '__updatedAt' on type 'Local.Models.Person", ex.Response.Content.ReadAsStringAsync().Result);
                Assert.Equal(ex.Request.RequestUri.ToString(), "http://localhost:31475/tables/Person?$filter=(__updatedAt ge datetimeoffset'0001-01-01T00:00:00.0000000%2B00:00')&$top=50&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__version");
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
        }

        public class EntityTests : TestBase
        {
            private IMobileServiceSyncTable<Entity> table;
            protected override void TestInitialize()
            {
                this.table = this.Context.client.GetSyncTable<Entity>();
            }

            [Fact]
            public async Task Entity_PullAsync_DownloadsTheItems()
            {
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
                    Entity downloaded = await this.table.LookupAsync(original.Id);
                    // the item should be there
                    Assert.NotNull(downloaded);
                    Assert.Equal(original.Text, downloaded.Text);
                    Assert.Equal(original.Done, downloaded.Done);
                }
            }

            [Fact]
            public async Task Entity_PullAsync_Incremental_DownloadsTheItems()
            {
                var first = NewItem();
                // insert an item
                await this.table.InsertAsync(first);
                // push it 
                await this.Context.client.SyncContext.PushAsync();
                // clear the table
                await this.table.PurgeAsync();
                // download the changes
                await this.table.PullAsync("items", this.table.CreateQuery());
                // verify the item was also downloaded
                Entity downloaded = await this.table.LookupAsync(first.Id);
                // the item should be there
                Assert.NotNull(downloaded);
                Assert.Equal(first.Text, downloaded.Text);
                Assert.Equal(first.Done, downloaded.Done);

                var second = NewItem();
                await this.Context.client.GetTable<Entity>().InsertAsync(second);
                // download the changes
                await this.table.PullAsync("items", this.table.CreateQuery());

                // verify the item was also downloaded
                downloaded = await this.table.LookupAsync(second.Id);
                // the item should be there
                Assert.NotNull(downloaded);
                Assert.Equal(second.Text, downloaded.Text);
                Assert.Equal(second.Done, downloaded.Done);
            }


            private static Entity NewItem()
            {
                var original = new Entity()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Text = "some text",
                    Done = false
                };
                return original;
            }
        }

        public class MappedEntityTests : TestBase
        {
            private IMobileServiceSyncTable<MappedEntity> table;
            protected override void TestInitialize()
            {
                this.table = this.Context.client.GetSyncTable<MappedEntity>();
            }

            [Fact]
            public async Task MappedEntity_PullAsync_DownloadsTheItems()
            {
                for (int i = 0; i < 2; i++)
                {
                    var original = NewItem();
                    // insert an item
                    await this.Context.client.GetTable<MappedEntity>().InsertAsync(original);
                    // download the changes
                    await this.table.PullAsync();
                    // verify the item was also downloaded
                    MappedEntity downloaded = await this.table.LookupAsync(original.Id);
                    // the item should be there
                    Assert.NotNull(downloaded);
                    Assert.Equal(original.CustomerName, downloaded.CustomerName);
                    Assert.Equal(original.Item, downloaded.Item);
                    Assert.Equal(original.Quantity, downloaded.Quantity);
                }
            }

            [Fact]
            public async Task MappedEntity_PullAsync_Incremental_DoesNotWork()
            {
                var first = NewItem();
                // insert an item
                await this.Context.client.GetTable<MappedEntity>().InsertAsync(first);
                // download the changes
                await this.table.PullAsync("items", this.table.CreateQuery());
                // verify the item was also downloaded
                MappedEntity downloaded = await this.table.LookupAsync(first.Id);
                // the item should not be there because __updatedAt is null for mapped entity
                // and doing a query where __updatedAt greater than some value simply returns no results
                Assert.Null(downloaded);
            }

            private static MappedEntity NewItem()
            {
                var original = new MappedEntity()
                {
                    CustomerName = "Henrik",
                    Item = "some item",
                    Quantity = 3
                };
                return original;
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
}
