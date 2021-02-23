using System;
using System.Text;
using AnyConnection;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

public class ClientConnectionExample : MonoBehaviour
{
    public static readonly Guid kMsgSendEditorToPlayer = new Guid("01E82D47-267D-48F7-AAE3-D5AD4B2F22D6");
    public static readonly Guid kMsgSendPlayerToEditor = new Guid("4A7CF7BB-362D-4045-BB10-7A72D710DCB3");

    private void Start()
    {
        ClientConnection.Instance.Register(kMsgSendEditorToPlayer, OnMessage);
    }

    private void OnMessage(MessageEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.data);
        Debug.Log($"Message from editor:{message}");
    }
}
