using System;

namespace SProcWrapper.Data
{
    public interface IDataContext : IDisposable, IQuery, IExecute
    {
        string ConnectionString { get; }
        int? CommandTimeout { get; set; }
        int SessionId { get; }
        void BeginTransaction(TransactionType type = TransactionType.Write);
        void RollbackTransaction();
        void CommitTransaction();
    }
}