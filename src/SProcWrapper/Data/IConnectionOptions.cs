using System.Data.Common;

namespace SProcWrapper.Data
{
    public interface ISessionInfo
    {
        /// <summary>
        /// Идентификатор подключения (сессия пользователя).
        /// </summary>
        int SessionId { get; }
    }

    public interface IConnectionOptions : ISessionInfo
    {
        /// <summary>
        /// Наименование БД
        /// </summary>
        string DbName { get; }

        string ToInsecureConnectionString();

        DbProviderFactory GetProviderFactory();

    }
}