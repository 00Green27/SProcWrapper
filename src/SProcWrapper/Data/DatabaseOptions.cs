using FirebirdSql.Data.FirebirdClient;
using SProcWrapper.Extensions;
using System.Data.Common;
using System.Net;

namespace SProcWrapper.Data
{
    /// <summary>
    ///     Представляет настройки для подключенияк базе данных.
    /// </summary>
    public class DatabaseOptions : IConnectionOptions
    {
        public DatabaseOptions(string server, string database, string user, string password, string dbName)
        {
            Server = server;
            Database = database;
            DbName = dbName;
            Credential = new NetworkCredential(user, password);
        }

        /// <summary>
        /// Получить строку соединения с репликой.
        /// Считается, что реплика идентична, за исключениме пути к ней
        /// </summary>
        /// <param name="pathForReplica"></param>
        /// <returns></returns>
        public IConnectionOptions CreateConnectionOptionsForReplica(string pathForReplica)
        {
            var strings = pathForReplica.ThrowIfNullOrWhiteSpace().Split(':');
            return new DatabaseOptions(strings[0], strings[1],
                Credential.UserName, Credential.Password,
                DbName + "_replica") {SessionId = SessionId};
        }

        /// <summary>
        ///     Время ожидания при попытке установки подключения, по истечении которого попытка подключения завершается и
        ///     создается ошибка.
        /// </summary>
        public int? ConnectionTimeoutInSeconds { get; set; }

        /// <summary>
        ///     Credential состоит из идентификатора пользователя и пароля, используемых для аутентификации Firebird.Пароль в
        ///     объекте Credential имеет тип SecureString.
        /// </summary>
        private NetworkCredential Credential { get; set; }

        /// <summary>
        ///     Источник данных.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     Путь или алиас к базе данных на сервере.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        ///     Кодировка определяет, символы какого национального алфавита будут использоваться. По умолчанию используется
        ///     кодировка WIN1251.
        /// </summary>
        public string Charset { get; set; } = "WIN1251";

        /// <summary>
        ///     Обьединять подключения в пул. Пул соединений создается для каждой уникальной строки соединения.  При создании пула
        ///     создается множество объектов соединения, которые добавляются в пул для удовлетворения требования к минимальному
        ///     размеру пула.  Соединения добавляются в пул по необходимости, вплоть до указанного максимального размера пула (по
        ///     умолчанию - 100).  Соединения освобождаются обратно в пул при закрытии или ликвидации.
        /// </summary>
        public bool Pooling { get; set; } = true;

        /// <summary>
        ///     Минимальный размер пула соединений с базой данных. Если аргумент MinPoolSize не указан в строке соединения или
        ///     указано значение 0, соединения в пуле будут закрыты после периода отсутствия активности.  Однако, если значение
        ///     аргумента MinPoolSize больше 0, пул соединений не уничтожается, пока не будет выгружен домен приложения AppDomain и
        ///     не завершится процесс.  Обслуживание неактивных или пустых пулов требует минимальных системных издержек.
        /// </summary>
        public short MinPoolSize { get; set; } = 1;

        /// <summary>
        ///     Максимальный размер пула соединений с базой данных. Если достигнут максимальный размер пула, а пригодные соединения
        ///     недоступны, запрос помещается в очередь.  Затем организатор пулов пытается вернуть любое соединение до истечения
        ///     времени ожидания (по умолчанию - 15 секунд).  Если организатор пулов не может обработать запрос до истечения
        ///     времени ожидания соединения, возникнет исключение.
        /// </summary>
        public short MaxPoolSize { get; set; } = 50;

        public int SessionId { get; set; }
        public string DbName { get; }

        public string ToInsecureConnectionString()
        {
            return new FbConnectionStringBuilder
            {
                DataSource = Server,
                Database = Database,
                UserID = Credential?.UserName,
                Password = Credential?.Password,
                Pooling = Pooling,
                Charset = Charset,
                MinPoolSize = MinPoolSize,
                MaxPoolSize = MaxPoolSize
            }.ConnectionString;
        }

        public DbProviderFactory GetProviderFactory()
        {
            return FirebirdClientFactory.Instance;
        }
    }
}