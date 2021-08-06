using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace pitermarx.Data
{
    public static class DataUtils
    {
        public static T Execute<T>(this DbConnection conn, string text, Func<DbCommand, T> action)
        {
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = text;
            return action(cmd);
        }

        public static T Set<T>(this T cmd, string sql, params (string name, object value)[] parameters)
            where T : IDbCommand
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.Clear();
            foreach (var (name, value) in parameters)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                param.Value = value;
                cmd.Parameters.Add(param);
            }
            return cmd;
        }

        public static async Task<IReadOnlyList<T>> ReadAll<T>(this DbCommand cmd, Func<DbDataReader, T> read, CancellationToken token = default)
        {
            var list = new List<T>();

            using (var r = await cmd.ExecuteReaderAsync(token))
            {
                while (await r.ReadAsync(token))
                {
                    list.Add(read(r));
                }
            }

            return list;
        }
    }
}