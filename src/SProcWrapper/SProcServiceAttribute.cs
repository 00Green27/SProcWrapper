using System;

namespace SProcWrapper
{
    /// <summary>
    /// Указание, что данный интерфейс отражает набор ХП в БД
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class SProcServiceAttribute : Attribute
    {
        public string Namespace { get; set; } = "";
    }
}