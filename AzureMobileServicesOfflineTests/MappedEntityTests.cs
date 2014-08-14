using System.Threading.Tasks;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
{
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
}
