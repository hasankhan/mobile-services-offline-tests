using System;
using Microsoft.WindowsAzure.MobileServices;

namespace AzureMobileServicesOfflineTests.Types
{
    [DataTable("Green")]
    public class Entity
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

        public string Text { get; set; }

        public bool Done { get; set; }
    }
}
