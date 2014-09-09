using System;
using System.Threading.Tasks;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
    public class StorageTests : TestBase
    {
        private IMobileServiceSyncTable<Storage> table;
        protected override void TestInitialize()
        {
            this.table = this.Context.Client.GetSyncTable<Storage>();
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
                await this.Context.Client.SyncContext.PushAsync();
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
        public async Task Storage_PullAsync_Incremental__DownloadsTheItems()
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
            Storage downloaded = await this.table.LookupAsync(first.Id);
            // the item should be there
            Assert.NotNull(downloaded);
            Assert.Equal(first.FirstName, downloaded.FirstName);
            Assert.Equal(first.LastName, downloaded.LastName);

            var second = NewItem();
            await this.Context.Client.GetTable<Storage>().InsertAsync(second);
            // download the changes
            await this.table.PullAsync("items", this.table.CreateQuery());

            // verify the item was also downloaded
            downloaded = await this.table.LookupAsync(second.Id);
            // the item should be there
            Assert.NotNull(downloaded);
            Assert.Equal(second.FirstName, downloaded.FirstName);
            Assert.Equal(second.LastName, downloaded.LastName);
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
}
