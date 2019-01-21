using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SProcWrapper.Proxy
{
    public static class SProcAttributesHandler
    {
        public const string DefaultPrefix = "";

        public static string SprocServiceAttributeHandle(Type type)
        {
            var serviceAttribute = type.GetCustomAttribute<SProcServiceAttribute>();
            return string.IsNullOrWhiteSpace(serviceAttribute?.Namespace)
                ? DefaultPrefix
                : serviceAttribute.Namespace + "_";
        }

        internal static IEnumerable<(MethodInfo methodInfo, SProcCallAttribute attribute)>
            FindSProcCallAnnotatedMethods(Type type)
        {
            var attributeType = typeof(SProcCallAttribute);
            return type.GetMethods()
                .Select(m =>
                    (methodInfo: m, attribute: (SProcCallAttribute) m.GetCustomAttribute(attributeType, false)))
                .Where(tuple => tuple.attribute != null);
        }

        public static Dictionary<MethodInfo, StoredProcedure> Handle(Type type)
        {
            var prefix = SprocServiceAttributeHandle(type);
            var attributeType = typeof(SProcParamAttribute);

            return FindSProcCallAnnotatedMethods(type).ToDictionary(
                method => method.methodInfo,
                method =>
                {
                    var storedProcedureParameters = method.methodInfo.GetParameters()
                        .Select(x => (param: x, attribute: (SProcParamAttribute) x.GetCustomAttribute(attributeType)))
                        .Where(tuple => tuple.attribute != null)
                        .Select(parameter => new StoredProcedureParameter(
                            parameter.param.Name,
                            parameter.param.ParameterType,
                            parameter.attribute.TableType,
                            parameter.attribute.DbType,
                            parameter.param.Position,
                            parameter.attribute.Sensitive))
                        .ToList();

                    return new StoredProcedure(prefix + method.attribute.Name, method.methodInfo.ReturnType,
                        storedProcedureParameters, method.attribute);
                });
        }
    }
}