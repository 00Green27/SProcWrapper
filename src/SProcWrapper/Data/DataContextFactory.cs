using System;
using System.Collections.Generic;

namespace SProcWrapper.Data
{
    public static class DataContextFactory
    {
        private static readonly Dictionary<DbTypeEnum, IConnectionOptions> DataBases =
            new Dictionary<DbTypeEnum, IConnectionOptions>();

        /// <summary>
        /// Регистрация различных БД
        /// </summary>
        /// <param name="dbType">Тип БД</param>
        /// <param name="connectionOptions">Строка подключения</param>
        public static void RegisterConnectionOptions(DbTypeEnum dbType, IConnectionOptions connectionOptions)
        {
            DataBases.Add(dbType, connectionOptions);
        }

        /// <summary>
        /// Создание обертки над объектом подключения к указанной БД
        /// </summary>
        /// <param name="dbType">Тип БД</param>
        /// <returns>Объект подключения к указанной БД</returns>
        public static DataContext GetDataContext(DbTypeEnum dbType)
        {
            return new DataContext(GetConnectionOptions(dbType));
        }

        /// <summary>
        /// Получает 
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns>Фабрика строки соединения для указанного типа БД</returns>
        /// <exception cref="InvalidOperationException">Если данный тип не зарегистрирован</exception>
        public static IConnectionOptions GetConnectionOptions(DbTypeEnum dbType)
        {
            return DataBases.TryGetValue(dbType, out var value)
                ? value
                : throw new InvalidOperationException($"Тип БД {dbType} не зарегистрирован");
        }
    }
}