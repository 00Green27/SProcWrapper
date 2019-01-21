using System;
using SProcWrapper.Data;
using SProcWrapper.Extensions;
using SProcWrapper.Proxy;

namespace SProcWrapper
{
    public abstract class AbstractSProcService<TInterface>
    {
        protected readonly IDataContext Context;
        protected readonly TInterface Sproc;

        /// <exception cref="ArgumentNullException"></exception>
        public AbstractSProcService(IDataContext context)
        {
            Context = context.ThrowIfNull(paramName: nameof(context));
            Sproc = SProcProxy.Build<TInterface>(Context);
        }
    }
}