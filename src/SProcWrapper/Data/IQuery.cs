using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SProcWrapper.Data
{
    public interface IQuery
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null);

        IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null);

        object Query(Type returnType, string sql, dynamic param = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null);

    }
}