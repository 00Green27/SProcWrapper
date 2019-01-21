using System;
using System.Collections.Generic;
using System.Linq;

namespace SProcWrapper.Data.RemoteEvents
{
    /// <summary>
    /// Обработчик событий, возникающих в БД
    /// </summary>
    public static class RemoteEventListner
    {
        private static readonly object LockObject = new object();
        private static EventsListener _eventListener;
        private static Dictionary<string, RemoteEventHandler> _remoteEventHandlers;

        /// <summary>
        /// Регистрирует обработку событий
        /// </summary>
        public static void StartListen(params RemoteEventHandler[] eventHandlers)
        {
            lock (LockObject)
            {
                if (_eventListener != null)
                    throw new InvalidOperationException(
                        $"Повторная инициализация {nameof(RemoteEventListner)} не допускается!");

                if (eventHandlers == null) return;

                _remoteEventHandlers = eventHandlers.ToDictionary(x => x.EventName);

                _eventListener = new EventsListener(
                    DataContextFactory.GetConnectionOptions(DbTypeEnum.Main), _remoteEventHandlers.Keys.ToArray());
                _eventListener.EventHandler += OnUserEventHandler;

                foreach (var eventHandler in _remoteEventHandlers)
                {
                    eventHandler.Value.OnStart();
                }
            }
        }

        /// <summary>
        /// Остановка отслеживания событий для возможности изменить список отслеживаемых событий
        /// </summary>
        public static void StopListen()
        {
            lock (LockObject)
            {
                if (_eventListener == null) return;

                _eventListener.EventHandler -= OnUserEventHandler;
                _eventListener.Dispose();
                _eventListener = null;

                foreach (var eventHandler in _remoteEventHandlers)
                {
                    eventHandler.Value.OnStop();
                }

                _remoteEventHandlers?.Clear();
            }
        }

        private static void OnUserEventHandler(object sender, RemoteEventEventArgs e)
        {
            _remoteEventHandlers[e.Name].OnEvent();
        }
    }
}