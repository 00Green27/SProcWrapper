using System;

namespace SProcWrapper
{
    /// <summary>
    /// Указание, что данный метод соответсвует ХП в БД
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SProcCallAttribute : Attribute, ICallOptionsParameters
    {
        public SProcCallAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        /// <inheritdoc />
        public SelectModeEnum SelectMode { get; set; }


        public int Timeout { get; set; } = 0;
        public bool Buffered { get; set; } = true;

        public bool WithSessionId { get; set; } = false;
    }

    /// <summary>
    /// Варианты вызова selectable ХП
    /// </summary>
    public enum SelectModeEnum
    {
        /// <summary>
        /// select * from...
        /// </summary>
        All,

        /// <summary>
        /// select column1, column2 from...
        /// </summary>
        ByColumnName
    }
}