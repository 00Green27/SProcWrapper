using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Dapper;

namespace SProcWrapper
{
    public class ColumnAttributeTypeMapper<T> : FallbackTypeMapper
    {
        public ColumnAttributeTypeMapper()
            : base(new SqlMapper.ITypeMap[]
            {
                new CustomPropertyTypeMap(typeof(T), (type, columnName) =>
                    type.GetProperties()
                        .FirstOrDefault(
                            prop =>
                                prop.GetCustomAttributes(false)
                                    .OfType<ColumnAttribute>()
                                    .Any(
                                        attr =>
                                            string.Equals(attr.Name, columnName,
                                                StringComparison.CurrentCultureIgnoreCase))
                                &&
                                (prop.DeclaringType == type
                                    ? prop.GetSetMethod(true)
                                    : prop.DeclaringType?.GetProperty(prop.Name,
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                        ?.GetSetMethod(true)) != null)),
                new DefaultTypeMap(typeof(T))
            })
        {
        }
    }

    public class FallbackTypeMapper : SqlMapper.ITypeMap
    {
        private readonly SqlMapper.ITypeMap[] _mappers;

        protected FallbackTypeMapper(SqlMapper.ITypeMap[] mappers)
        {
            _mappers = mappers;
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _mappers
                .Select(mapper => mapper.FindConstructor(names, types))
                .FirstOrDefault(result => result != null);
        }

        public ConstructorInfo FindExplicitConstructor()
        {
            return _mappers
                .Select(mapper => mapper.FindExplicitConstructor())
                .FirstOrDefault(result => result != null);
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            return _mappers
                .Select(mapper => mapper.GetConstructorParameter(constructor, columnName))
                .FirstOrDefault(result => result != null);
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            return _mappers
                .Select(mapper => mapper.GetMember(columnName))
                .FirstOrDefault(result => result != null);
        }
    }
}