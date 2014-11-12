using System;
using System.Threading.Tasks;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
    public class NodeTests : TestBase
    {
        private IMobileServiceSyncTable<Node> table;
        protected override void TestInitialize()
        {
            this.table = this.Context.Client.GetSyncTable<Node>();
        }

        protected override string Uri
        {
            get { return "http://localhost:11926/"; }
        }

        [Fact]
        public async Task Node_PullAsync_DownloadsTheItems()
        {
            for (int i = 0; i < 2; i++)
            {
                var original = NewItem();
                // insert an item
                await this.table.InsertAsync(original);
                // push it 
                await this.Context.Client.SyncContext.PushAsync();
                // clear the table
                await this.table.PurgeAsync();
                // download the changes
                await this.table.PullAsync(null, null);
                // verify the item was also downloaded
                Node downloaded = await this.table.LookupAsync(original.Id);
                // the item should be there
                Assert.NotNull(downloaded);
                Assert.Equal(original.Name, downloaded.Name);
                Assert.Equal(original.Col5, downloaded.Col5);
            }
        }

        [Fact]
        public async Task Node_PullAsync_Incremental_DownloadsTheItems()
        {
            var first = NewItem();
            // insert an item
            await this.table.InsertAsync(first);
            // push it 
            await this.Context.Client.SyncContext.PushAsync();
            // clear the table
            await this.table.PurgeAsync();
            // download the changes
            await this.table.PullAsync("items", this.table.CreateQuery());
            // verify the item was also downloaded
            Node downloaded = await this.table.LookupAsync(first.Id);
            // the item should be there
            Assert.NotNull(downloaded);
            Assert.Equal(first.Col5, downloaded.Col5);
            Assert.Equal(first.Name, downloaded.Name);

            var second = NewItem();
            await this.Context.Client.GetTable<Node>().InsertAsync(second);
            // download the changes
            await this.table.PullAsync("items", this.table.CreateQuery());

            // verify the item was also downloaded
            downloaded = await this.table.LookupAsync(second.Id);
            // the item should be there
            Assert.NotNull(downloaded);
            Assert.Equal(second.Name, downloaded.Name);
            Assert.Equal(second.Col5, downloaded.Col5);
        }


        private static Node NewItem()
        {
            var original = new Node()
            {
                Id = Guid.NewGuid().ToString("N"),
                Col5 = true,
                Name = "Someone"
            };
            return original;
        }
    }
}
