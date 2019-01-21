using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using SProcWrapper.Extensions;

namespace SProcWrapper.Data
{
    [DebuggerDisplay(
        "InTransaction={_transaction != null}, ConnectionString = {" + nameof(ConnectionString) + "}"
    )]
    public class DataContext : IDataContext
    {
        private readonly IConnectionOptions _connectionOptions;
        private int? _commandTimeout;
        private DbConnection _connection;
        private DbTransaction _transaction;
        private int _connectionDepth;
        private readonly DbProviderFactory _factory;

        public void Dispose()
        {
            CloseConnection();
        }

        /// <exception cref="ArgumentNullException"></exception>
        public DataContext(string connectionString, DbProviderFactory factory)
        {
            ConnectionString = connectionString.ThrowIfNullOrWhiteSpace(paramName: nameof(connectionString));
            _factory = factory.ThrowIfNull(paramName: nameof(factory));
            //KeepConnectionAlive = true;
        }

        public DataContext(IConnectionOptions builder) : this(builder.ToInsecureConnectionString(),
            builder.GetProviderFactory())
        {
            _connectionOptions = builder;
        }

        #region Connection 

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OpenConnection()
        {
            if (_connectionDepth == 0)
            {
                _connection = _factory.CreateConnection();
                _connection.ConnectionString = ConnectionString;

                switch (_connection.State)
                {
                    case ConnectionState.Broken:
                        _connection.Close();
                        break;
                    case ConnectionState.Closed:
                        _connection.Open();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _connectionDepth++;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CloseConnection()
        {
            if (_connectionDepth > 0)
            {
                _connectionDepth--;
                if (_connectionDepth == 0)
                {
//                    OnConnectionClosing(_connection);
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        #endregion
        public int SessionId => _connectionOptions != null && _connectionOptions.SessionId != 0
            ? _connectionOptions.SessionId
            : throw new InvalidOperationException("Не указана № сессии подключения");

        #region Transaction
        public void BeginTransaction(TransactionType type = TransactionType.Write)
        {
            OpenConnection();
            switch (type)
            {
                case TransactionType.ReadOnly:
                {
                    switch (_connection)
                    {
                        case FbConnection fbConnection:
                        {
                            _transaction = fbConnection.BeginTransaction(new FbTransactionOptions
                            {
                                WaitTimeout = new TimeSpan?(),
                                TransactionBehavior = FbTransactionBehavior.Read |
                                                      FbTransactionBehavior.NoWait |
                                                      FbTransactionBehavior.ReadCommitted |
                                                      FbTransactionBehavior.RecVersion
                            });
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_connection));
                    }
                    break;
                }
                case TransactionType.Write:
                {
                    _transaction = _connection.BeginTransaction();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        public void RollbackTransaction()
        {
            _transaction.Rollback();
            CleanupTransaction();
        }

        private void CleanupTransaction()
        {
            _transaction.Dispose();
            _transaction = null;
            CloseConnection();
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            CleanupTransaction();
        }

        #endregion

        //Logging
        public virtual bool OnException(Exception x)
        {
            Debug.WriteLine(x.ToString());
            return true;
        }


        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value.HasValue
                    && (value < 0))
                {
                    throw new ArgumentException("Неверное значение таймаута для команды.");
                }

                _commandTimeout = value;
            }
        }

        public string ConnectionString { get; }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.QueryAsync<T>(_connection, sql, param, _transaction, commandTimeout ?? CommandTimeout,
                    commandType);
            }
            catch (Exception ex)
            {
                if (OnException(ex))
                    throw;
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        public int Execute(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.Execute(_connection, sql, param, _transaction, commandTimeout ?? CommandTimeout,
                    commandType);
            }
            catch (Exception e)
            {
                if (OnException(e))
                    throw;
                return -1;
            }
            finally
            {
                CloseConnection();
            }
        }

        public Task ExecuteAsync(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.ExecuteAsync(_connection, sql, param, _transaction, commandTimeout ?? CommandTimeout,
                    commandType);
            }
            catch (Exception e)
            {
                if (OnException(e))
                    throw;
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        public T ExecuteScalar<T>(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.ExecuteScalar<T>(_connection, sql, param, _transaction,
                    commandTimeout ?? CommandTimeout, commandType);
            }
            catch (Exception e)
            {
                if (OnException(e))
                    throw;
                return default(T);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Метод получения из БД типизированного поля через рефлексию
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public object ExecuteScalar(Type returnType, string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            //Не работает, если из БД возвращается int, а поле long
            //return SqlMapper.ExecuteScalar(_connection, sql, param, _transaction, commandTimeout ?? CommandTimeout, commandType);
            try
            {
                var method = 
                    GetType().GetMethods()
                        .Single(m => m.Name == nameof(IDataContext.ExecuteScalar) && m.IsGenericMethod && m.GetParameters().Length == 4)
                        .MakeGenericMethod(returnType);

                return method.Invoke(this, new object[] { sql, param, commandTimeout, commandType });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, dynamic param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.ExecuteScalarAsync<T>(_connection, sql, param, _transaction,
                    commandTimeout ?? CommandTimeout, commandType);
            }
            catch (Exception e)
            {
                if (OnException(e))
                    throw;
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Метод получения из БД типизированного IEnumerable через рефлексию
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public object Query(Type returnType, string sql, dynamic param = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            try
            {
                var method =
                    GetType().GetMethods()
                        .Single(m => m.Name == nameof(IDataContext.Query) && m.IsGenericMethod && m.GetParameters().Length == 5);

                return method.MakeGenericMethod(returnType)
                    .Invoke(this, new object[] {sql, param, buffered, commandTimeout, commandType});
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        public IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            OpenConnection();
            try
            {
                return SqlMapper.Query<T>(_connection, sql, param, _transaction, buffered,
                    commandTimeout ?? CommandTimeout, commandType);
            }
            catch (Exception ex)
            {
                if (OnException(ex))
                    throw;
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}