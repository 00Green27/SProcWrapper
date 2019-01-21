using System;
using System.Data;
using System.Threading.Tasks;

namespace SProcWrapper.Data
{
    public interface IExecute
    {
        int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);
        Task ExecuteAsync(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);
        object ExecuteScalar(Type returnType, string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null);
        
        Task<T> ExecuteScalarAsync<T>(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null);
    }
}