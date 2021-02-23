using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace AnyConnection
{
    public interface IStaticConnectionHost
    {
        void OnReceiveMessage(Guid messageId, byte[] data);
    }

    public class StaticConnection : IClientConnectionImpl
    {
        public static readonly StaticConnection Instance = new StaticConnection();

        private IStaticConnectionHost host;
        public bool IsHostConnected => host != null;

        public bool IsClientConnected { get; private set; }

        private List<UnityAction<int>> connectionCallbacks = new List<UnityAction<int>>();
        private List<UnityAction<int>> disconnectionCallbacks = new List<UnityAction<int>>();
        private Dictionary<Guid, List<UnityAction<MessageEventArgs>>> registerCallbacks = new Dictionary<Guid, List<UnityAction<MessageEventArgs>>>();

#region host
        public void ConnectHost(IStaticConnectionHost host)
        {
            this.host = host;
            foreach (var callback in this.connectionCallbacks)
            {
                try
                {
                    callback(-1);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void DisconnectHost()
        {
            if (this.host == null) return;

            foreach (var callback in this.disconnectionCallbacks)
            {
                try
                {
                    callback(-1);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void SendToClient(Guid messageId, byte[] data)
        {
            if (!Application.isPlaying)
            {
                DisconnectClient();
                return;
            }

            if (!this.registerCallbacks.TryGetValue(messageId, out var list))
            {
                return;
            }


            var msg = new MessageEventArgs
            {
                playerId = 0,
                data = data,
            };

            foreach (var callback in list)
            {
                try
                {
                    callback(msg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
#endregion

#region client
        public void ConnectClient()
        {
            this.IsClientConnected = true;
        }

        public void DisconnectClient()
        {
            this.IsClientConnected = false;
        }

        bool IClientConnectionImpl.IsConnected => IsHostConnected;

        void IClientConnectionImpl.RegisterConnection(UnityAction<int> callback)
        {
            this.connectionCallbacks.Add(callback);
            if (this.IsHostConnected)
            {
                try
                {
                    callback(-1);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        void IClientConnectionImpl.RegisterDisconnection(UnityAction<int> callback)
        {
            this.disconnectionCallbacks.Add(callback);
        }

        void IClientConnectionImpl.Register(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (!this.registerCallbacks.TryGetValue(messageId, out var list))
            {
                list = new List<UnityAction<MessageEventArgs>>();
                this.registerCallbacks[messageId] = list;
            }

            list.Add(callback);
        }

        void IClientConnectionImpl.Unregister(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (!this.registerCallbacks.TryGetValue(messageId, out var list)) return;
            list.Remove(callback);
        }

        bool IClientConnectionImpl.TrySend(Guid messageId, byte[] data)
        {
            if (!this.IsHostConnected) return false;

            try
            {
                this.host.OnReceiveMessage(messageId, data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return true;
        }
#endregion
    }
}
