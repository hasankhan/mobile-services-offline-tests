using System;
using Microsoft.WindowsAzure.MobileServices;

namespace AzureMobileServicesOfflineTests.Types
{
    [DataTable("Mongo")]
    public class Mongo
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

        public string Name { get; set; }

        public int Age { get; set; }
    }
}
