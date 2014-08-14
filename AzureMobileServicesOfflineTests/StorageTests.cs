using System;
using System.Threading.Tasks;
using AssertExLib;
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
}
