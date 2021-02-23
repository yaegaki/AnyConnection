using System;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace AnyConnection
{
    public class ClientConnection
    {
        public static readonly ClientConnection Instance = new ClientConnection();

        private readonly IClientConnectionImpl impl;

        private ClientConnection()
        {
#if UNITY_EDITOR
            StaticConnection.Instance.ConnectClient();
            impl = StaticConnection.Instance;
#else
            impl = new ClientConnectionImplPlayerConnection();
#endif
        } 

        public bool IsConnected => impl.IsConnected;

        public void RegisterConnection(UnityAction<int> callback)
            => impl.RegisterConnection(callback);

        public void RegisterDisconnection(UnityAction<int> callback)
            => impl.RegisterDisconnection(callback);

        public void Register(Guid messageId, UnityAction<MessageEventArgs> callback)
            => impl.Register(messageId, callback);

        public void UnRegister(Guid messageId, UnityAction<MessageEventArgs> callback)
            => impl.Unregister(messageId, callback);

        public bool TrySend(Guid messageId, byte[] data)
            => impl.TrySend(messageId, data);
    }

    interface IClientConnectionImpl
    {
        bool IsConnected { get; }
        void RegisterConnection(UnityAction<int> callback);
        void RegisterDisconnection(UnityAction<int> callback);
        void Register(Guid messageId, UnityAction<MessageEventArgs> callback);
        void Unregister(Guid messageId, UnityAction<MessageEventArgs> callback);
        bool TrySend(Guid messageId, byte[] data);
    }

    /// <summary>
    /// For not editor runtime.
    /// </summary>
    class ClientConnectionImplPlayerConnection : IClientConnectionImpl
    {
        private readonly PlayerConnection connection;

        public bool IsConnected => connection.isConnected;

        public ClientConnectionImplPlayerConnection()
            => this.connection = PlayerConnection.instance;

        public void RegisterConnection(UnityAction<int> callback)
            => this.connection.RegisterConnection(callback);

        public void RegisterDisconnection(UnityAction<int> callback)
            => this.connection.RegisterDisconnection(callback);

        public void Register(Guid messageId, UnityAction<MessageEventArgs> callback)
            => this.connection.Register(messageId, callback);

        public void Unregister(Guid messageId, UnityAction<MessageEventArgs> callback)
            => this.connection.Unregister(messageId, callback);

        public bool TrySend(Guid messageId, byte[] data)
            => this.connection.TrySend(messageId, data);
    }
}
