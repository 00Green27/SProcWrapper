using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using SProcWrapper.Data;
using SProcWrapper.Extensions;

namespace SProcWrapper.Proxy
{
    public class SProcProxy : IInterceptor
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private readonly IReadOnlyDictionary<MethodInfo, StoredProcedure> _sprocs;
        private readonly IDataContext _dataContext;

        public static T Build<T>(IDataContext context)
        {
            var spDictionary = SProcAttributesHandler.Handle(typeof(T));
            var proxy = new SProcProxy(context, spDictionary);
            return (T) ProxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(T), proxy);
        }

        private SProcProxy(IDataContext context, IReadOnlyDictionary<MethodInfo, StoredProcedure> spDictionary)
        {
            _dataContext = context.ThrowIfNull(paramName: nameof(context));
            _sprocs = spDictionary;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_sprocs.TryGetValue(invocation.Method, out var storedProcedure))
            {
                invocation.ReturnValue = storedProcedure.Execute(_dataContext, invocation.Arguments);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}