namespace SProcWrapper
{
    /// <summary>
    /// Опциональные параметры вызова ХП
    /// </summary>
    public interface ICallOptionsParameters
    {
        int Timeout { get; }
        bool Buffered { get; }
        bool WithSessionId { get; }

        /// <summary>
        /// Вариант вызова selectable ХП
        /// </summary>
        SelectModeEnum SelectMode { get; }
    }
}