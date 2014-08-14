using System;
using Microsoft.WindowsAzure.MobileServices;

namespace AzureMobileServicesOfflineTests.Types
{
    [DataTable("stringid_test_table")]
    public class Node
    {
        public string Id { get; set; }
        public bool Col5 { get; set; }
        public string Name { get; set; }

        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }

        [UpdatedAt]
        public DateTimeOffset UpdatedAt { get; set; }

        [Deleted]
        public bool Deleted { get; set; }
    }
}
