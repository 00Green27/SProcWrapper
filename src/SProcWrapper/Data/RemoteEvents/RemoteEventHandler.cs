using SProcWrapper.Extensions;

namespace SProcWrapper.Data.RemoteEvents
{
    /// <summary>
    /// Класс обработчиков событий, возникающих в БД
    /// </summary>
    public abstract class RemoteEventHandler
    {
        protected RemoteEventHandler(string eventName)
        {
            EventName = eventName.ThrowIfNullOrWhiteSpace("Имя эвента обязательно");
        }

        /// <summary>
        /// Имя обрабатываемого события
        /// </summary>
        /// <returns></returns>
        public string EventName { get; }

        /// <summary>
        /// Обработка события
        /// </summary>
        public abstract void OnEvent();

        /// <summary>
        /// Метод вызываемый при запуске прослушивателя
        /// </summary>
        public virtual void OnStart()
        {
        }

        /// <summary>
        /// Метод вызываемый при остановке прослушивателя
        /// </summary>
        public virtual void OnStop()
        {
        }
    }
}