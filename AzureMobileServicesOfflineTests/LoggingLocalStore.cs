using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace AzureMobileServicesOfflineTests
{
    class LoggingLocalStore : MobileServiceSQLiteStore
    {
        public LoggingLocalStore(string fileName)
            : base(fileName)
        {
        }

        protected override IList<Newtonsoft.Json.Linq.JObject> ExecuteQuery(string tableName, string sql, IDictionary<string, object> parameters)
        {
            Debug.WriteLine(string.Format("Query - tableName: {0}, sql: {1}", tableName, sql));

            return base.ExecuteQuery(tableName, sql, parameters);
        }

        protected override void ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            Debug.WriteLine(string.Format("NonQuery - sql: {0}", sql));

            base.ExecuteNonQuery(sql, parameters);
        }
    }
}
