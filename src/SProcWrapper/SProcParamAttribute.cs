using System;
using System.Data;

namespace SProcWrapper
{
    /// <summary>
    /// Указание, что данный параметр соответствует таковому в ХП
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SProcParamAttribute : Attribute
    {
        public SProcParamAttribute()
        {
        }

        public SProcParamAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; } = "";
        public Type TableType { get; set; }
        public DbType? DbType { get; set; }

        /// <summary>
        ///     Параметр содержит конфиденциальную информацию, которую не стоит писать в лог.
        /// </summary>
        /// <value>
        ///     <c>true</c> конфиденциальной информации, иначе <c>false</c>.
        /// </value>
        public bool Sensitive { get; set; } = false;
    }
}