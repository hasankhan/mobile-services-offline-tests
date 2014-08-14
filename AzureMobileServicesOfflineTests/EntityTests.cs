using System;
using System.Threading.Tasks;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
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
}
