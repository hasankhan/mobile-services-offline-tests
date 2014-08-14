using System;
using System.Linq;
using System.Threading.Tasks;
using AssertExLib;
using AzureMobileServicesOfflineTests.Types;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xunit;

namespace AzureMobileServicesOfflineTests
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
}
