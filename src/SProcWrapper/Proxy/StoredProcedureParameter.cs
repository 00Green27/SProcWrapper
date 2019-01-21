using System;
using System.Data;

namespace SProcWrapper.Proxy
{
    public class StoredProcedureParameter
    {
        public int Position { get; }

        public DbType? DbType { get; }

        public bool IsSensitive { get; }

        public string Name { get; }

        public Type Type { get; }

        public Type TableType { get; }


        public StoredProcedureParameter(string parameterName, Type parameterType, Type type,
            DbType? dbType, int position, bool sensitive)
        {
            Name = parameterName;
            Type = parameterType;
            TableType = type;
            DbType = dbType;
            Position = position;
            IsSensitive = sensitive;
            //TODO: преобразование FB в DB тип
        }
    }
}