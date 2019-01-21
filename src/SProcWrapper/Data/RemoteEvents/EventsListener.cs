using FirebirdSql.Data.FirebirdClient;
using System;

namespace SProcWrapper.Data.RemoteEvents
{
    /// <inheritdoc />
    /// <summary>
    /// Класс для мониторинга событий БД Firebird 
    /// Каждый экземпляр класса открывает один порт на сервере, и освобожает его при Dispose()
    /// </summary>
    public class EventsListener : IDisposable
    {
        private readonly FbRemoteEvent _remoteEvent;

        public event EventHandler<RemoteEventEventArgs> EventHandler;

        public EventsListener(IConnectionOptions builder, params string[] events)
        {
            _remoteEvent = new FbRemoteEvent(builder.ToInsecureConnectionString());
            _remoteEvent.RemoteEventCounts += OnRemoteEvent;
            _remoteEvent.QueueEvents(events);
        }

        public void Dispose()
        {
            _remoteEvent?.Dispose();
        }

        private void OnRemoteEvent(object sender, FbRemoteEventCountsEventArgs args)
        {
            EventHandler?.Invoke(sender,
                new RemoteEventEventArgs(args.Name, args.Counts));
        }
    }

    /// <summary>
    /// Класс оболочка над FbRemoteEventCountsEventArgs для устранения зависимостей
    /// </summary>
    public class RemoteEventEventArgs
    {
        public RemoteEventEventArgs(string name, int counts)
        {
            Name = name;
            Counts = counts;
        }

        public string Name { get; }
        public int Counts { get; }
    }
}