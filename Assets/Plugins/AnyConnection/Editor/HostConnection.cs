using System;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace AnyConnection
{
    public class HostConnection : IStaticConnectionHost
    {
        public static readonly HostConnection Instance = new HostConnection();

        private List<ConnectedPlayer> connectedPlayers = new List<ConnectedPlayer>();
        private Dictionary<Guid, List<UnityAction<MessageEventArgs>>> registerCallbacks = new Dictionary<Guid, List<UnityAction<MessageEventArgs>>>();

        public void Initialize()
        {
            EditorConnection.instance.Initialize();
            StaticConnection.Instance.ConnectHost(this);
        }

        public void DisconnectAll()
        {
            EditorConnection.instance.DisconnectAll();
            StaticConnection.Instance.DisconnectHost();
        }

        public void Register(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            EditorConnection.instance.Register(messageId, callback);
            if (!registerCallbacks.TryGetValue(messageId, out var list))
            {
                list = new List<UnityAction<MessageEventArgs>>();
                registerCallbacks[messageId] = list;
            }
            list.Add(callback);
        }

        public void Unregister(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            EditorConnection.instance.Unregister(messageId, callback);
            if (!registerCallbacks.TryGetValue(messageId, out var list)) return;
            list.Remove(callback);
        }

        public int GetConnectedPlayersCount()
        {
            var count = EditorConnection.instance.ConnectedPlayers.Count;
            if (StaticConnection.Instance.IsClientConnected)
            {
                count++;
            }
            return count;
        }

        public IEnumerable<ConnectedPlayer> EnumerateConnectedPlayers()
        {
            foreach (var p in EditorConnection.instance.ConnectedPlayers)
            {
                yield return p;
            }

            if (StaticConnection.Instance.IsClientConnected)
            {
                yield return new ConnectedPlayer(-1, "Editor");
            }
        }

        public void Send(Guid messageId, byte[] data)
        {
            EditorConnection.instance.Send(messageId, data);
            StaticConnection.Instance.SendToClient(messageId, data);
        }

        void IStaticConnectionHost.OnReceiveMessage(Guid messageId, byte[] data)
        {
            if  (!registerCallbacks.TryGetValue(messageId, out var list)) return;

            var msg = new MessageEventArgs
            {
                playerId = -1,
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
                    Debug.LogException(e);
                }
            }
        }
    }
}
