using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using SProcWrapper.Data;
using SProcWrapper.Extensions;

namespace SProcWrapper.Proxy
{
    public class StoredProcedure
    {
        private string _query;
        private readonly Type _returnType;
        private readonly int _timeout;
        private readonly bool _buffered;
        private readonly bool _withSessionId;
        private readonly bool _selectable;
        private readonly bool _returnTypeAsOutput;
        private readonly IList<StoredProcedureParameter> _parameters;

        private readonly ExecutorTypeEnum _executorType;

        enum ExecutorTypeEnum
        {
            MultiRowSimpleType,
            SingleScalarType,
            SingleRowSimpleType,
        }

        public StoredProcedure(string name, Type returnType, IList<StoredProcedureParameter> storedProcedureParameters,
            ICallOptionsParameters callOptionsParameters)
        {
            Name = name.ThrowIfNullOrWhiteSpace(paramName: "Наименование ХП");
            _returnType = returnType;
            _timeout = callOptionsParameters.Timeout;
            _buffered = callOptionsParameters.Buffered;
            _withSessionId = callOptionsParameters.WithSessionId;

            _parameters = storedProcedureParameters;

            if (returnType.IsGenericType)
            {
                var gTypeDef = returnType.GetGenericTypeDefinition();
                if (gTypeDef == typeof(IEnumerable<>) || gTypeDef.GetInterfaces()
                        .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    _executorType = ExecutorTypeEnum.MultiRowSimpleType;
                    _selectable = true;

                    var moneyType = typeof(MoneyAttribute);

                    if (callOptionsParameters.SelectMode == SelectModeEnum.ByColumnName)
                    {
                        var columnType = typeof(ColumnAttribute);

                        var columnNames = returnType.GetGenericArguments()[0].GetProperties().Where(x => x.CanWrite)
                            .Select(x =>
                            {
                                var columnName = ((ColumnAttribute) x.GetCustomAttribute(columnType))?.Name ?? x.Name;

                                if (x.IsDefined(moneyType))
                                    columnName = $"{columnName}/100.00 as {columnName}";

                                return columnName;
                            });

                        Query = $"SELECT {string.Join(",", columnNames)} FROM {Name}({GetSqlParameterList()})";
                    }
                    else
                    {
                        if (returnType.GetGenericArguments()[0].GetProperties().Any(x => x.IsDefined(moneyType)))
                            throw new NotSupportedException(
                                $"Если отпределен атрибут {moneyType.Name}, то тип вызова должен быть {SelectModeEnum.ByColumnName}");
                    }
                }
            }
            else if (returnType.IsPrimitive || returnType == typeof(string))
            {
                _executorType = ExecutorTypeEnum.SingleScalarType;
            }
            else
            {
                _returnTypeAsOutput = true;
                _executorType = ExecutorTypeEnum.SingleRowSimpleType;
            }
        }

        public string Name { get; }

        public object Execute(IDataContext dataContext, object[] argumentsParameters)
        {
            var parameters = GetParameters(argumentsParameters, dataContext);
            switch (_executorType)
            {
                case ExecutorTypeEnum.MultiRowSimpleType:
                    return dataContext.Query(_returnType.GetGenericArguments()[0], Query, parameters,
                        _buffered, _timeout);


                case ExecutorTypeEnum.SingleScalarType:
                    return dataContext.ExecuteScalar(_returnType, Query, parameters, _timeout);

                case ExecutorTypeEnum.SingleRowSimpleType:
                    dataContext.Execute(Query, parameters, _timeout);

                    if (_returnType == typeof(void))
                    {
                        return null;
                    }

                    //todo: Данную конструкцию необходимо заменить на использование селективных ХП
                    var result = Activator.CreateInstance(_returnType);

                    foreach (var pi in _returnType.GetProperties().Where(x => x.CanWrite))
                    {
                        var name = parameters.ParameterNames.Single(x => x == pi.Name);
                        pi.SetValue(result, parameters.Get<object>(name), null);
                    }

                    return result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DynamicParameters GetParameters(object[] args, IDataContext dataContext)
        {
            var result = new DynamicParameters();

            if (_withSessionId)
                result.Add("aSessID", dataContext.SessionId);

            foreach (var p in _parameters)
            {
                result.Add(p.Name, args[p.Position]);
            }
            if (_returnTypeAsOutput)
            {
                foreach (var p in _returnType.GetProperties().Where(x => x.CanWrite))
                {
                    result.Add(p.Name, direction: ParameterDirection.Output);
                }
            }

            return result;
        }

        public string GetSqlParameterList()
        {
            string s = "";

            if (_withSessionId)
            {
                s += "@aSessID";
                if (_parameters.Any()) s += ",";
            }


            return s + string.Join(",", _parameters.Select(x => $"@{x.Name}"));
        }

        public string Query
        {
            get
            {
                if (_query == null)
                {
                    string command = _selectable ? "SELECT * FROM " : "EXECUTE PROCEDURE ";
                    _query = command + Name + " ( " + GetSqlParameterList() + " )";
                }
                return _query;
            }
            private set { _query = value; }
        }
    }
}