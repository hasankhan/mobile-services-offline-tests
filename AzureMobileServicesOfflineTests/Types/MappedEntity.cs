using System;
using Microsoft.WindowsAzure.MobileServices;

namespace AzureMobileServicesOfflineTests.Types
{
    [DataTable("BrownOnline")]
    public class MappedEntity
    {
        public string Id { get; set; }

        [Version]
        public string Version { get; set; }

        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }

        [UpdatedAt]
        public DateTimeOffset UpdatedAt { get; set; }

        [Deleted]
        public bool Deleted { get; set; }
        public string Item { get; set; }

        public int Quantity { get; set; }

        public string CustomerName { get; set; }
    }
}
