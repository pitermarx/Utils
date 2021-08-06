using System;
using System.Collections.Generic;

namespace pitermarx.SqlDependency
{
    internal static class SqlDependenciesManager
    {
        private static readonly Dictionary<string, int> ConnectionStrings;
        private static readonly object LockObj = new();

        static SqlDependenciesManager()
        {
            ConnectionStrings = new Dictionary<string, int>();

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                foreach (var cs in ConnectionStrings)
                {
                    Microsoft.Data.SqlClient.SqlDependency.Stop(cs.Key);
                }
            };
        }

        internal static void AddConnectionString(string cs)
        {
            lock (LockObj)
            {
                if (!ConnectionStrings.ContainsKey(cs))
                {
                    Microsoft.Data.SqlClient.SqlDependency.Start(cs);
                    ConnectionStrings.Add(cs, 1);
                }
                else
                {
                    ConnectionStrings[cs] += 1;
                }

            }
        }

        internal static void RemoveConnectionString(string cs)
        {
            lock (LockObj)
            {
                if (ConnectionStrings.ContainsKey(cs))
                {
                    if(ConnectionStrings[cs] == 1)
                    {
                        Microsoft.Data.SqlClient.SqlDependency.Stop(cs);
                        ConnectionStrings.Remove(cs);
                    }
                    else
                    {
                        ConnectionStrings[cs] -= 1;
                    }
                }
            }
        }
    }
}